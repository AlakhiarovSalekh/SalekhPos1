# Auth Security

> **Scope.** The security rationale and threat model for the Phase 2 authentication feature. Lives next to the top-level [`SECURITY.md`](./SECURITY.md) policy. The decisions referenced here are recorded in [ADR-009](../decisions/ADR-009-identity-and-auth.md).
>
> **Source of truth for code references.** Every link to a source file points at the commit that shipped the auth feature (`ef291ce`). The line numbers shift as the project evolves; the file paths are stable.

## Threat model

The asset is the **user's session**: the bearer of the access and refresh tokens can act as the user inside the tenant, including performing financial operations once Phase 7 lands. Compromising a session is therefore equivalent to identity theft inside the tenant.

The actors, the threats they pose, and the mitigations we ship today:

| Actor | Threat | Mitigation |
|---|---|---|
| Anonymous attacker on the public internet | Credential stuffing with leaked password lists | Argon2id password hashing (slow on purpose); account lockout after 5 failed logins; per-IP and per-account rate limits. |
| Anonymous attacker on the public internet | Phishing | The web client uses the standard `Origin` header check; CORS is not used in production. Email-verification and password-reset links are one-time. |
| Authenticated attacker on their own account | Privilege escalation | Tokens carry a `role` claim; every protected endpoint validates it. Cross-tenant access is impossible because the `tid` claim is part of the token and the API rejects any request where the path's tenant id does not match. |
| Attacker who has achieved XSS in the web app | Session theft via the `localStorage` token blob | Short access-token lifetime (15 min) so a stolen access token is short-lived; refresh tokens are rotated on every use so the stolen refresh token is invalidated the next time the legitimate user refreshes; the `ver` claim forces a DB lookup that catches the revoked token; a strict CSP is planned for Phase 18 to make XSS harder. |
| Attacker with physical access to a logged-in device | Use of the existing session | Idle-session timeouts (planned); the user can revoke all sessions from the account page (planned); "Sign out everywhere" is the natural extension of `/auth/change-password` (which already bumps `token_version`). |
| Malicious insider (employee of the operator) | Read another tenant's data | Every query is scoped by `TenantId`; the architecture tests (4 of them, in `SalekhPos.Tests`) enforce that the domain and application layers do not leak across tenants. |
| Malicious insider | Impersonate a user | Admin-initiated user impersonation is not implemented in Phase 2; when it lands it will be a separate audit-logged action that produces a clearly-flagged token. |

What is explicitly **out of scope** for this slice: defence against a compromised user device (a keylogger still wins), against physical seizure of an unlocked device, against a coerced user. These are the user's problem to mitigate with device-level controls.

## Password policy

The policy is in [`backend/src/SalekhPos.Application/Common/PasswordPolicy.cs`](../../backend/src/SalekhPos.Application/Common/PasswordPolicy.cs) and the validator in [`backend/src/SalekhPos.Application/Identity/Register/RegisterValidator.cs`](../../backend/src/SalekhPos.Application/Identity/Register/RegisterValidator.cs). The rules:

- **Minimum length: 12 characters.** Why 12, not 8: at 8 characters a high-entropy password is within reach of a modern GPU farm. 12 makes the same attack materially more expensive. Length matters more than composition: a long passphrase with no symbols is harder to crack than a short one with all four character classes. The length is also the only rule that is easy for humans to remember.
- **At least one of each: upper-case letter, lower-case letter, digit, symbol.** This is a "defense in depth" rule; the length rule is the primary defense. We do NOT enforce exotic rules like "no consecutive characters" or "no common dictionary words" because they produce passwords like `Password1!` that satisfy the rule but are easier to guess than a random string the user actually generated.
- **No maximum length (other than the protocol's 128-character ceiling).** We accept passphrases.
- **No password expiry.** Forced rotation produces weaker passwords (users append `1`, `2`, `3`) and provides negligible benefit when the password is unique to the service. The relevant event is "credential leaked in a breach", not "the password is N months old".
- **No password hints, no security questions.** Both are attack surfaces; the recovery path is email + a one-time reset link.
- **No breach-list check yet.** This is a follow-up: a Phase 19 hardening item will hash the password with a k-anonymity API (HaveIBeenPwned or a self-hosted equivalent) and reject any password that appears in a known breach.

The password is hashed with Argon2id, parameters from `appsettings.Security.PasswordHashing` (defaults match the OWASP 2026 cheat sheet). The hash is stored in `users.password_hash`; the plaintext is never logged and never leaves the request handler that consumed it.

## Token storage on the web client

The web client stores both the access and the refresh token in a single JSON blob under the localStorage key `salekhpos.auth`:

```ts
// from web/salekhpos-web/src/services/storage/storage.ts
const StoredAuth = {
  accessToken: string,
  refreshToken: string,
  accessTokenExpiresAtUtc: string,
  refreshTokenExpiresAtUtc: string,
  userId: string,
  tenantId: string,
  role: string,
  tokenVersion: number,
};
```

**The trade-off we are accepting.** `localStorage` is readable by any JavaScript running on the page. An XSS in the web app steals the entire session. We accept this because:

1. The backend issues the refresh token as an opaque string in the JSON body. The only way for the browser to re-send it on `/auth/refresh` is to read it from JavaScript. `httpOnly` cookies (the alternative) require a different issuance path (Set-Cookie instead of JSON body) and a CSRF defence, and they also require a same-site policy that the reverse proxy would have to enforce.
2. The cost of an XSS is bounded by the access-token lifetime (15 min) and the rotation-on-refresh strategy (the refresh token is invalidated the first time the legitimate user refreshes after a theft).
3. The full CSP, Trusted Types, and a secret scanner in CI are planned for Phase 18. Until then, the project's web client is a single-page React app that ships no third-party scripts; the XSS surface is the application's own code, which is small and reviewed.

**What we are doing now to reduce the XSS surface:**

- The web client renders no `dangerouslySetInnerHTML`. All user-visible strings are rendered as React text children, which escapes by default.
- The i18n library (`i18next`) is configured with `escapeValue: false` because React already escapes. This is correct.
- The error banner never renders `error.message` as HTML; it switches on the `code` and renders a localized string from the locale files.
- The user-controlled fields (tenant slug, business name) are validated by zod and re-validated on the server. A user who managed to bypass the client and submit a string with HTML tags would be rejected by the FluentValidation rules.

## Token rotation and reuse detection

Refresh tokens are stored in `refresh_tokens` with this shape (column names shortened for clarity):

| Column | Notes |
|---|---|
| `id` | UUID. |
| `user_id` | FK to `users.id`. |
| `tenant_id` | FK to `tenants.id` (denormalised so a tenant delete can cascade). |
| `token_hash` | SHA-256 of the opaque refresh token string. We never store the plaintext. |
| `created_at` | When this row was created. |
| `expires_at` | 30 days after creation. |
| `used_at` | NULL while the token is live; set to `now()` when the token is consumed by `/auth/refresh`. |
| `replaced_by` | FK to the row that replaced this one (NULL for the most recent token). |

On every call to `/auth/refresh`:

1. The request body is `{ refreshToken: "..." }`. The server hashes it and looks up the row by `token_hash`.
2. If the row does not exist → 401 `auth.invalid_credentials`.
3. If `expires_at < now()` → 401 `auth.invalid_credentials`.
4. If `used_at IS NOT NULL` → **replay detected.** The whole family is revoked: every row that is reachable via the `replaced_by` chain is marked `used_at = now()` and the user's `token_version` is bumped. The current request returns 410 `auth.token_replay`.
5. Otherwise: the row's `used_at` is set to `now()`, a new row is created with a fresh token, and the old row's `replaced_by` points at the new one. The new token is returned to the client. The next refresh from the same chain must present the new token; the old one is now `used` and will trigger the replay path.

The chain can be at most 30 days long (the refresh-token lifetime); a user who has not refreshed in 30 days has to sign in again.

The repository is in [`backend/src/SalekhPos.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`](../../backend/src/SalekhPos.Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs); the EF configuration in [`backend/src/SalekhPos.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`](../../backend/src/SalekhPos.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs); the use cases that call it are in [`backend/src/SalekhPos.Application/Identity/Refresh/`](../../backend/src/SalekhPos.Application/Identity/Refresh/) and [`backend/src/SalekhPos.Application/Identity/Logout/`](../../backend/src/SalekhPos.Application/Identity/Logout/).

## MFA recovery codes

When the user sets up MFA, the API generates 8 single-use recovery codes in the format `xxxx-xxxx-xxxx` (the four-character groups are a 16-symbol base32-like alphabet, deliberately omitting `0`/`1`/`O`/`I` for legibility). The user sees the codes exactly once. The server stores the SHA-256 of each code, never the plaintext.

When the user uses a recovery code at `/auth/mfa/verify`, the server:

1. Hashes the presented code and looks it up in `mfa_recovery_codes`.
2. If the row does not exist → 401 `auth.invalid_recovery_code`.
3. If the row exists but `used_at IS NOT NULL` → 401 `auth.invalid_recovery_code`.
4. Otherwise: marks the row `used_at = now()`, and issues the token pair.

When the user has used all 8 codes, they cannot recover via recovery codes any more. The current path is to contact support, who can reset MFA on the user's behalf (this is logged as an audit event). A future slice will add a self-service "I lost all my recovery codes" path that requires proving control of the email AND the password.

## Account lockout and rate limiting

- **Failed logins:** 5 within 15 minutes locks the account for 15 minutes. The counter is keyed by `(tenantId, email)` and resets on a successful login. The lockout is enforced in the `LoginHandler` use case and surfaced to the client as a 423 response with `auth.account_locked` and a `lockedUntilUtc` timestamp. The web client renders this as the `auth.errors.accountLocked` banner.
- **MFA attempts:** 5 within 5 minutes per user id. Exceeded → 429 `auth.rate_limited` with `retryAfterSeconds`.
- **Refresh attempts:** a per-IP limit of 60/min. (The Slice 4b ASP.NET Core rate limiter will replace this hand-rolled check.)
- **General API rate limit:** 100 requests per minute per IP, also hand-rolled in the use cases for now. Slice 4b replaces it with the framework limiter.

The decision to enforce these in the application layer (not at the database or the reverse proxy) is intentional: the limits are per-user (not per-IP) and the lockout counter has to be transactional with the login attempt. The reverse proxy is the second line of defence (Caddy can refuse a flood of connections), not the first.

## Logout semantics

`POST /api/v1/auth/logout`:

1. The bearer token is validated; on failure → 401.
2. The current refresh token is looked up and marked `used_at = now()`.
3. The user's `token_version` is bumped. This invalidates every outstanding access token for the user across every device.
4. The response is 204.

The "local-only" sign-out (just deleting the `localStorage` blob) is what the web client does when the API call fails (e.g., the user is offline). In that case the access token is still technically valid for the remainder of its 15-minute lifetime, and any other tab that has the same blob will continue to work. The next refresh from the other tab will fail and the user will be redirected to `/login`. The local-only path is a UX concession, not a security feature.

## What we deliberately do NOT do yet

These are called out so a future reader does not think they were forgotten:

- **Security questions** for account recovery. They are an attack surface (the answers are usually findable on social media) and the email-based reset is sufficient.
- **Device fingerprinting** for session binding. It produces false positives when a user upgrades their browser, and the underlying assumption (the device id is hard to forge) is not true for a determined attacker.
- **Binding sessions to IP or User-Agent.** The same reason. Plus, the web client is used from networks where the IP can change mid-session (mobile networks, corporate VPNs).
- **Push-based MFA.** Requires a mobile app (Phase 14) and a notification provider; TOTP is the default until then.
- **Anomaly detection** (impossible-travel alerts, brute-force heuristics beyond the simple rate limits). Planned for Phase 18.
- **Penetration testing.** The first external pentest is scheduled for the end of Phase 18.

## Audit log

Every state-changing auth event is logged to the standard structured logger with a stable event id:

| Event id | When |
|---|---|
| `auth.register.succeeded` | After a successful register. Includes `userId`, `tenantId`. |
| `auth.register.failed` | On any register failure. Includes the failure `code` and (for password-policy failures) the list of reasons; never the password. |
| `auth.login.succeeded` | After a successful login. Includes `userId`, `tenantId`, `mfa = true|false`. |
| `auth.login.failed` | On a 401/403/423/429. Includes the failure `code` and the masked email. |
| `auth.refresh.succeeded` | After a successful refresh. |
| `auth.refresh.replay_detected` | When a presented refresh token had `used_at` set. Includes the affected `userId`; the family is already revoked at this point. |
| `auth.logout.succeeded` | After a successful logout. |
| `auth.mfa.setup.succeeded` | After a successful MFA setup. |
| `auth.mfa.verify.succeeded` / `auth.mfa.verify.failed` | Per call. |
| `auth.password.changed` | After a successful change-password or reset-password. Includes the `userId`; the new hash is never logged. |

The logger is Serilog with the request-id enrichment from the `RequestIdMiddleware`; the log destination is configured in `appsettings` (a future Slice 4b item wires it to the production aggregator).
