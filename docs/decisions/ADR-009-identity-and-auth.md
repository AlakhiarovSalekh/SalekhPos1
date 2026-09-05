# ADR-009: Identity and Authentication

**Date:** 2026-09-05
**Status:** Accepted
**Phase:** 2

## Context

Phase 2 introduces the first hard slice of the system: the identity and authentication surface that every later phase depends on. Without a `UserId` and a verified way to attribute a request, multi-tenancy (Phase 3) cannot scope data, the role/permission system cannot grant capabilities, the POS sale flow (Phase 7) cannot record who completed a sale, and the audit log (Phase 11) cannot attribute cash-register activity. This ADR locks in the seven cross-cutting decisions that the auth feature rests on; the API contract lives in [`docs/api/auth.md`](../api/auth.md) and the threat model in [`docs/security/auth.md`](../security/auth.md).

The relevant prior decisions are: [ADR-001](./ADR-001-modular-monolith.md) (modular monolith — auth lives in the existing `SalekhPos.Api` and is split by use cases, not by service), [ADR-002](./ADR-002-postgresql.md) (Postgres is the system of record, including for refresh tokens and MFA factors), [ADR-006](./ADR-006-multi-tenancy.md) (every authenticated request is scoped to a `TenantId`, which the auth flow must issue), and [ADR-008](./ADR-008-idempotency.md) (state-changing endpoints must be idempotent; this applies to `forgot-password` and `verify-email` because both are triggered by clicking a one-time link in an email).

## Decision

The auth feature rests on seven concrete decisions. Each is locked in here; the rationale and the alternatives we considered are below.

### 1. Access tokens are JWTs signed with EdDSA (Ed25519)

The access token is a JWS with three claims: `sub` (user id), `tid` (tenant id), `role` (the user's role inside the tenant), and `ver` (the user's current `token_version`). It is signed with the tenant-independent Ed25519 key stored in `SalekhPos.Infrastructure.Security.JwtSigner`. The lifetime is 15 minutes. The ASP.NET Core `JwtBearer` middleware does not support EdDSA in .NET 8, so the API ships a custom `AuthenticationHandler` ([`EdDsaAuthenticationHandler.cs`](../../backend/src/SalekhPos.Api/Authentication/EdDsaAuthenticationHandler.cs)) that calls the same `IJwtSigner` the issuer uses.

### 2. Passwords are hashed with Argon2id

Passwords are hashed with Argon2id (memory-hard, OWASP-recommended as of 2026). The parameters (memory, iterations, parallelism) live in `appsettings.Security.PasswordHashing` and are loaded at startup; defaults match the OWASP "cheat sheet" minimums for 2026 hardware. The hasher is a thin wrapper around BouncyCastle's `Argon2BytesGenerator`. Plaintext passwords never leave the request handler that consumes them; the API logs the request and the result but never the password.

### 3. Refresh tokens are opaque random strings, stored server-side

The refresh token is a 32-byte random value, base64url-encoded. The server stores the SHA-256 hash (not the plaintext) in `refresh_tokens` along with `user_id`, `tenant_id`, `expires_at`, `used_at`, and `replaced_by`. The client only ever sees the opaque string. On every successful refresh the token is rotated: the old row is marked `used_at = now()` and the new row's id is stored in `replaced_by` on the old one. Trade-off vs. stateless refresh JWTs: revocation is exact (delete the row) and reuse detection is exact (a row with `used_at` set that is presented again is a replay and revokes the whole family).

### 4. Token revocation via `token_version`

Every user row carries an integer `token_version`. Logout, password change, role change, and admin-initiated session invalidation all bump it. Every JWT carries the version in the `ver` claim. The custom authentication handler reads `ver` from the token and (in a future slice, via a per-request cache) compares it to the user's current `token_version`; on a mismatch the token is rejected with 401, which triggers the web client's refresh-on-401 interceptor; the refresh itself fails because the refresh token has been deleted (or because `token_version` has bumped and the new access token would also be stale).

### 5. MFA via TOTP (RFC 6238)

Multi-factor authentication uses TOTP. The secret is 20 random bytes, base32-encoded. The encrypted secret (AES-GCM with the platform key) and a per-factor counter live in `mfa_factors`. Recovery codes are 8 single-use codes generated at MFA setup, each one the format `xxxx-xxxx-xxxx`; the SHA-256 of each is stored in `mfa_recovery_codes`. The user sees the recovery codes exactly once. A separate `verify` endpoint consumes a recovery code instead of a TOTP value when the user has lost their device.

### 6. All auth errors surface as RFC 7807 ProblemDetails

Every auth endpoint returns errors as RFC 7807 ProblemDetails with a machine-readable `type` URI and a short symbolic `code` field. The mapping is centralized in [`DomainExceptionMapping.cs`](../../backend/src/SalekhPos.Api/ProblemDetails/DomainExceptionMapping.cs): `InvalidCredentials → 401 auth.invalid_credentials`, `AccountLocked → 423 auth.account_locked`, `EmailNotVerified → 403 auth.email_not_verified`, `PasswordPolicy → 400 auth.password_policy`, `RateLimited → 429 auth.rate_limited`, `InvalidToken → 400 or 410 auth.token_invalid / auth.token_replay` (410 for replays). The web client's [`AuthErrorBanner`](../../web/salekhpos-web/src/features/auth/components/AuthErrorBanner.tsx) maps `status`+`code` to the localized banner copy in `auth.errors.*`.

### 7. Web client stores tokens in `localStorage` (single JSON blob)

The web client stores both the access and the refresh token in one JSON blob under the localStorage key `salekhpos.auth`. The blob contains the tokens, the expiry timestamps, and the decoded identity claims (`userId`, `tenantId`, `role`, `tokenVersion`). A single `remove()` signs the user out. Trade-off vs. httpOnly cookies: the backend issues the refresh token as an opaque string inside the JSON body — there is no other way for the JavaScript to read it on subsequent requests. The XSS exposure this introduces is mitigated by a strict CSP (planned for Phase 18) and by the short access-token lifetime (15 min) combined with the refresh-on-401 interceptor in [`apiClient.ts`](../../web/salekhpos-web/src/services/api/apiClient.ts).

## Consequences

- **Every JWT verification requires a user lookup** to compare `ver` to the current `token_version`. For a multi-tenant system with low request volume per user this is acceptable; if/when it becomes a bottleneck the lookup can be cached in Redis with a short TTL (a Slice 4b item).
- **XSS in the web app is a session-stealing risk.** Strict CSP, Trusted Types, and the secret scanner in CI are listed as Phase 18 deliverables.
- **Refresh tokens are rotated on every use.** A client that tries to reuse a refresh token is treated as compromised: the entire token family is revoked and the user's `token_version` is bumped, signing them out everywhere.
- **The Argon2id parameters are a per-environment tuning knob.** A future CI benchmark step (Phase 19) will keep the hash cost under a target ceiling (e.g., 250 ms on the reference hardware).
- **MFA recovery codes are not regenerable by the user.** A user who has used all 8 codes must contact support to reset MFA, which is an explicit UX trade-off (it forces a human in the loop for the highest-risk recovery path).
- **Logout is a real network call.** `POST /api/v1/auth/logout` returns 204; it deletes the refresh token, bumps `token_version`, and the next API call from any other device will see 401.
- **The auth surface has no OpenAPI spec yet.** The doc in `docs/api/auth.md` is the source of truth until the OpenAPI slice lands.
- **The web client assumes the backend is reachable on the same origin via the `/api` proxy.** In dev, Vite proxies `/api` to `http://localhost:8080`; in prod the reverse proxy (Caddy) terminates TLS and forwards to the API. CORS is therefore not used; same-origin is the only supported deployment shape for the web client.

## Alternatives considered

- **RS256 instead of EdDSA for the access token.** Rejected: EdDSA produces smaller signatures (64 bytes vs. 256), verifies faster, and avoids the need for a PKI (the verification key is a single 32-byte secret rather than a certificate chain). The trade-off is that rotating the signing key invalidates every outstanding access token at once; in practice the 15-minute lifetime makes this acceptable.
- **Stateless refresh tokens (refresh JWTs).** Rejected: revocation requires either a deny-list (which we then have to scale) or a stateless mechanism (which means the same security properties as a JWT access token, plus the extra complexity of two token types). The opaque-token approach is simpler and the database hit per refresh is negligible.
- **SMS OTP for MFA.** Rejected: SIM-swap attacks, the operational burden of a third-party SMS provider, and the regulator's increasing scrutiny of SMS as a second factor (NIST SP 800-63B lists it as "restricted" as of 2026). TOTP is the default; SMS can be added as a backup later if the user explicitly asks for it.
- **Email-based "magic link" login instead of passwords.** Rejected as a default: it changes the operational shape of the product (no cashiers typing passwords), and it makes the local-dev experience worse. Magic links may be added later as a "remember this device" path, but the primary login flow is email + password (+ optional MFA).
- **`httpOnly` cookies for the refresh token.** Rejected: the backend issues the refresh token as an opaque string in the JSON body; switching to cookies would require a different issuance path, and the web client would need CSRF protection. The `localStorage` decision is explicit and the trade-off is documented in [`docs/security/auth.md`](../security/auth.md).
- **Bcrypt or scrypt for password hashing.** Rejected: Argon2id is the current OWASP recommendation and is more resistant to GPU-based cracking than both.
- **In-memory rate limiting only (no DB-backed limiter).** Rejected: an attacker that owns a botnet can hit the API from enough IPs to defeat per-IP limits. Slice 4b introduces the ASP.NET Core rate limiter with a Redis-backed partition so the limit is global per user and per IP.

## References

- API reference: [`docs/api/auth.md`](../api/auth.md)
- Security rationale: [`docs/security/auth.md`](../security/auth.md)
- Status: [`docs/development/IMPLEMENTATION_STATUS.md`](../development/IMPLEMENTATION_STATUS.md)
- Backend: [`backend/src/SalekhPos.Api/`](../../backend/src/SalekhPos.Api/), [`backend/src/SalekhPos.Application/Identity/`](../../backend/src/SalekhPos.Application/Identity/), [`backend/src/SalekhPos.Infrastructure/Security/`](../../backend/src/SalekhPos.Infrastructure/Security/)
- Web: [`web/salekhpos-web/src/features/auth/`](../../web/salekhpos-web/src/features/auth/), [`web/salekhpos-web/src/services/auth/`](../../web/salekhpos-web/src/services/auth/)
