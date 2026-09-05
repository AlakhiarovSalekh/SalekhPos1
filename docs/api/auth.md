# Auth API

> **Scope.** The public authentication surface of the SalekhPos API. All endpoints are versioned under `/api/v1/auth/`. Every request and response is JSON. All errors are RFC 7807 ProblemDetails (see [Error responses](#error-responses)).
>
> **Why this doc.** The OpenAPI/Swagger spec is a deferred Slice 4b item. Until then, this file is the authoritative API reference; the TypeScript request/response types in [`web/salekhpos-web/src/services/auth/authApi.ts`](../../web/salekhpos-web/src/services/auth/authApi.ts) and the C# DTOs in [`backend/src/SalekhPos.Api/Auth/Dtos/AuthDtos.cs`](../../backend/src/SalekhPos.Api/Auth/Dtos/AuthDtos.cs) are auto-generated from this contract.
>
> **Auth required?** The `Auth` column in each section is one of:
> - **No** — the endpoint is anonymous; the client must NOT attach a bearer token.
> - **Yes** — the endpoint requires a valid access token in the `Authorization: Bearer …` header.
> - **No — MFA second leg** — anonymous, but only used after `/auth/login` returned `status: mfa_required`.
>
> **Conventions.**
> - All `*Utc` fields are ISO 8601 strings in UTC with a `Z` suffix.
> - All `*Id` fields are UUID v4.
> - All passwords must be at least 12 characters and contain at least one upper-case letter, one lower-case letter, one digit, and one symbol. The exact rule is in the password policy error responses below.

## Table of contents

- [POST /api/v1/auth/register](#post-apiv1authregister)
- [POST /api/v1/auth/verify-email](#post-apiv1authverify-email)
- [POST /api/v1/auth/login](#post-apiv1authlogin)
- [POST /api/v1/auth/refresh](#post-apiv1authrefresh)
- [POST /api/v1/auth/logout](#post-apiv1authlogout)
- [POST /api/v1/auth/forgot-password](#post-apiv1authforgot-password)
- [POST /api/v1/auth/reset-password](#post-apiv1authreset-password)
- [POST /api/v1/auth/change-password](#post-apiv1authchange-password)
- [POST /api/v1/auth/mfa/setup](#post-apiv1authmfasetup)
- [POST /api/v1/auth/mfa/verify](#post-apiv1authmfaverify)
- [POST /api/v1/auth/mfa/disable](#post-apiv1authmfadisable)
- [Error responses](#error-responses)

---

## POST /api/v1/auth/register

Create a new user and tenant. The user must verify their email before they can sign in.

- **Auth:** No
- **Source:** [`AuthController.cs:Register`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{
  "email": "owner@acme.test",
  "password": "CorrectHorse-123!",
  "fullName": "Ada Lovelace",
  "tenantName": "Acme Retail",
  "tenantSlug": "acme"
}
```

| Field | Type | Constraints |
|---|---|---|
| `email` | string | valid email, ≤ 254 chars, unique |
| `password` | string | ≥ 12 chars, must include upper/lower/digit/symbol (see password policy) |
| `fullName` | string | 1–120 chars |
| `tenantName` | string | 1–120 chars |
| `tenantSlug` | string | 3–40 chars, lowercase letters/digits/dashes, must start and end with a letter or digit, unique |

### Response — 202 Accepted

```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "tenantId": "22222222-2222-2222-2222-222222222222",
  "tenantSlug": "acme"
}
```

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | A FluentValidation rule failed (password too short, bad slug, …). The `errors` array in the body lists the per-field problems. |
| 409 | `auth.email_taken` | The email is already registered. |
| 409 | `auth.tenant_slug_taken` | The tenant slug is already taken. |

### Example

```bash
curl -X POST https://api.salekhpos.com/api/v1/auth/register \
  -H 'Content-Type: application/json' \
  -d '{"email":"owner@acme.test","password":"CorrectHorse-123!","fullName":"Ada Lovelace","tenantName":"Acme Retail","tenantSlug":"acme"}'
```

---

## POST /api/v1/auth/verify-email

Consume a one-time email-verification token. The token is sent in the verification email after `register`. After a successful call the user can sign in.

- **Auth:** No
- **Source:** [`AuthController.cs:VerifyEmail`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{ "token": "opaque-token-from-email" }
```

### Response — 204 No Content

Empty body.

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `auth.invalid_token` | The token is malformed or does not exist. |
| 410 | `auth.token_replay` | The token has already been used or has expired. |

---

## POST /api/v1/auth/login

Sign in with email and password. Returns either a token pair (success) or an MFA challenge.

- **Auth:** No
- **Source:** [`AuthController.cs:Login`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{ "email": "owner@acme.test", "password": "CorrectHorse-123!" }
```

### Response — 200 OK (authenticated)

```json
{
  "status": "authenticated",
  "tokens": {
    "accessToken": "eyJ…",
    "refreshToken": "opaque-refresh-token",
    "accessTokenExpiresAtUtc": "2026-09-05T12:15:00Z",
    "refreshTokenExpiresAtUtc": "2026-09-06T12:00:00Z"
  }
}
```

### Response — 200 OK (MFA required)

```json
{
  "status": "mfa_required",
  "mfa": {
    "userId": "11111111-1111-1111-1111-111111111111",
    "tenantId": "22222222-2222-2222-2222-222222222222"
  }
}
```

### Error responses

| Status | `code` | When |
|---|---|---|
| 401 | `auth.invalid_credentials` | Email does not exist or password is wrong. |
| 403 | `auth.email_not_verified` | The user has not verified their email. |
| 423 | `auth.account_locked` | Too many failed attempts. The body includes `lockedUntilUtc`. |
| 429 | `auth.rate_limited` | The IP has exceeded the login rate limit. The body includes `retryAfterSeconds`. |

---

## POST /api/v1/auth/refresh

Exchange a refresh token for a new token pair. The old refresh token is invalidated; the same family is tracked so a replayed token revokes the whole family.

- **Auth:** No (the refresh token itself is the credential)
- **Source:** [`AuthController.cs:Refresh`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{ "refreshToken": "opaque-refresh-token" }
```

### Response — 200 OK

```json
{
  "accessToken": "eyJ…",
  "refreshToken": "new-opaque-refresh-token",
  "accessTokenExpiresAtUtc": "2026-09-05T12:15:00Z",
  "refreshTokenExpiresAtUtc": "2026-09-06T12:00:00Z"
}
```

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `auth.invalid_token` | The refresh token is malformed. |
| 401 | `auth.invalid_credentials` | The refresh token does not exist. |
| 410 | `auth.token_replay` | The refresh token was already used; the whole token family has been revoked. |

---

## POST /api/v1/auth/logout

Invalidate the current refresh token and bump the user's `token_version` (which invalidates every outstanding access token for the same user across every device).

- **Auth:** Yes
- **Source:** [`AuthController.cs:Logout`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

No body. The user id is taken from the `sub` claim of the access token.

### Response — 204 No Content

Empty body.

### Error responses

| Status | `code` | When |
|---|---|---|
| 401 | `auth.unauthenticated` | The bearer token is missing or invalid. |

---

## POST /api/v1/auth/forgot-password

Send a password-reset email if the account exists. Always returns 204 to avoid leaking which emails are registered.

- **Auth:** No
- **Source:** [`AuthController.cs:ForgotPassword`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{ "email": "owner@acme.test" }
```

### Response — 204 No Content

Empty body.

### Error responses

None. The endpoint returns 204 in all cases (the email is sent asynchronously; the response is sent before the email is dispatched).

---

## POST /api/v1/auth/reset-password

Consume a password-reset token (delivered in the email from `/auth/forgot-password`) and set a new password. Bumps the user's `token_version`, which signs the user out everywhere.

- **Auth:** No
- **Source:** [`AuthController.cs:ResetPassword`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{
  "token": "opaque-reset-token-from-email",
  "newPassword": "NewCorrectHorse-456!"
}
```

### Response — 204 No Content

Empty body.

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | The new password does not satisfy the policy. |
| 400 | `auth.invalid_token` | The token is malformed. |
| 410 | `auth.token_replay` | The token has already been used or has expired. |

---

## POST /api/v1/auth/change-password

Change the password of the currently authenticated user. Bumps the user's `token_version`, which signs the user out everywhere except the current device (because the current device's access token is still valid; the next refresh will succeed because the new refresh token was issued in the same change-password transaction). The current implementation simply calls the same internal mechanism; on a future iteration it should return the new token pair so the user does not see a flicker.

- **Auth:** Yes
- **Source:** [`AuthController.cs:ChangePassword`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{
  "currentPassword": "CorrectHorse-123!",
  "newPassword": "NewCorrectHorse-456!"
}
```

### Response — 204 No Content

Empty body.

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | The new password does not satisfy the policy. |
| 401 | `auth.invalid_credentials` | The current password is wrong. |
| 401 | `auth.unauthenticated` | The bearer token is missing or invalid. |

---

## POST /api/v1/auth/mfa/setup

Generate a TOTP secret and 8 one-time recovery codes for the authenticated user. Idempotent: calling this twice resets MFA and issues a new secret (the old factor is deleted and all old recovery codes are invalidated). The user must scan the secret with an authenticator app and confirm a code to "commit" the factor; the current implementation does the commit inline at setup time (a `POST /auth/mfa/verify` with the first code). A future slice will add an explicit commit step.

- **Auth:** Yes
- **Source:** [`AuthController.cs:MfaSetup`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

No body.

### Response — 200 OK

```json
{
  "provisioningUri": "otpauth://totp/SalekhPos:owner@acme.test?secret=ABCDEFGHIJKLMNOP&issuer=SalekhPos",
  "recoveryCodes": ["aaaa-bbbb-cccc", "dddd-eeee-ffff", "…", "…"]
}
```

### Error responses

| Status | `code` | When |
|---|---|---|
| 401 | `auth.unauthenticated` | The bearer token is missing or invalid. |
| 409 | `auth.mfa_already_enabled` | A future iteration will return this when MFA commit is split out. |

---

## POST /api/v1/auth/mfa/verify

Consume a TOTP code (or a recovery code) and complete an in-flight MFA login. Returns the token pair.

- **Auth:** No — MFA second leg
- **Source:** [`AuthController.cs:MfaVerify`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "code": "123456"
}
```

| Field | Type | Constraints |
|---|---|---|
| `userId` | UUID | The `userId` returned by the previous `/auth/login` call. |
| `code` | string | 6 digits (TOTP) OR a recovery code in the format `xxxx-xxxx-xxxx`. |

### Response — 200 OK

```json
{
  "accessToken": "eyJ…",
  "refreshToken": "opaque-refresh-token",
  "accessTokenExpiresAtUtc": "2026-09-05T12:15:00Z",
  "refreshTokenExpiresAtUtc": "2026-09-06T12:00:00Z"
}
```

### Error responses

| Status | `code` | When |
|---|---|---|
| 400 | `validation_failed` | The code is not 6 digits. |
| 401 | `auth.invalid_mfa_code` | The TOTP code is wrong, expired, or already used. |
| 401 | `auth.invalid_recovery_code` | The recovery code is unknown or already used. |
| 429 | `auth.rate_limited` | Too many failed MFA attempts. The body includes `retryAfterSeconds`. |

---

## POST /api/v1/auth/mfa/disable

Disable MFA for the authenticated user. The current implementation does not require a re-confirmation; a future slice will require the user to re-enter their password or a TOTP code before disabling.

- **Auth:** Yes
- **Source:** [`AuthController.cs:MfaDisable`](../../backend/src/SalekhPos.Api/Auth/AuthController.cs)

### Request

No body.

### Response — 204 No Content

Empty body.

### Error responses

| Status | `code` | When |
|---|---|---|
| 401 | `auth.unauthenticated` | The bearer token is missing or invalid. |

---

## Error responses

Every error response is an RFC 7807 `application/problem+json` document with this shape:

```json
{
  "type": "https://salekhpos.com/errors/auth.invalid_credentials",
  "title": "Authentication failed",
  "status": 401,
  "detail": "Invalid credentials.",
  "instance": "/api/v1/auth/login",
  "code": "auth.invalid_credentials",
  "message": "Invalid credentials."
}
```

- **`type`** is a stable, machine-readable URI. Clients should switch on this.
- **`code`** is the short symbolic name; same identifier as the last path segment of `type`.
- **`message`** is human-readable in English; the web client does NOT use this directly — it switches on `code` and renders a localized message from `auth.errors.*` in the locale files.

### Status code → meaning

| Status | Meaning |
|---|---|
| 200 | Success, response body present. |
| 202 | Accepted; the request is being processed asynchronously (used by `/auth/register`). |
| 204 | Success, no response body. |
| 400 | Validation failed. The body has a `code` (or a `validation_failed` envelope with an `errors` array). |
| 401 | The request is unauthenticated, the credentials are wrong, or the token is invalid. |
| 403 | The request is authenticated but the user is not allowed (e.g., email not verified). |
| 410 | The one-time token has already been used or has expired. |
| 423 | The account is temporarily locked. |
| 429 | The rate limit has been exceeded. The body includes `retryAfterSeconds`. |

### `code` → meaning

| `code` | Status | Where it is raised |
|---|---|---|
| `auth.invalid_credentials` | 401 | login (email/password) or refresh |
| `auth.email_not_verified` | 403 | login |
| `auth.account_locked` | 423 | login (after too many failed attempts) |
| `auth.rate_limited` | 429 | login, refresh, mfa/verify |
| `auth.invalid_token` | 400 | verify-email, reset-password, mfa/verify |
| `auth.token_replay` | 410 | verify-email, reset-password, refresh |
| `auth.password_policy` | 400 | register, reset-password, change-password (extends the response with a `reasons` array) |
| `auth.email_taken` | 409 | register |
| `auth.tenant_slug_taken` | 409 | register |
| `auth.invalid_mfa_code` | 401 | mfa/verify |
| `auth.invalid_recovery_code` | 401 | mfa/verify |
| `auth.mfa_already_enabled` | 409 | mfa/setup (future) |
| `auth.unauthenticated` | 401 | any endpoint that requires a bearer token |
| `validation_failed` | 400 | FluentValidation envelope; the body has an `errors` array |
