# Security principles

SalekhPos treats security as a design constraint, not a feature. The threat model assumes:

- HTTP requests can be modified.
- Tenant IDs, store IDs, prices, and quantities can be modified.
- Requests can be replayed or duplicated.
- Sessions and devices can be stolen.
- Offline data on a stolen device can be manipulated.
- Permissions can be probed.
- APIs can be overloaded.
- SQL injection, XSS, SSRF, path traversal, malicious file uploads, CSRF (where applicable), and webhook replay are realistic threats.

## Principles

- **Security by Design.** Threats and mitigations are considered during design, not after.
- **Defense in Depth.** Layers: TLS, WAF/reverse proxy, authentication, authorization, tenant isolation, input validation, parameterized queries, DTOs, audit, monitoring.
- **Least Privilege.** Each user, role, and service has the minimum scope required.
- **Zero Trust.** No implicit trust between components; the API validates every request.
- **Auditability.** Critical actions are auditable; completed financial records are immutable.
- **Fail Secure.** On error, deny by default. Never leak internal details.

## Pipeline (every API call)

```text
Authenticated?
 -> Tenant valid?
 -> Store access valid?
 -> Permission valid?
 -> Resource ownership valid?
 -> Business rules valid?
 -> Execute
```

The frontend performs permission checks for UX only. The backend is authoritative.

## Authentication

- Argon2id (where compatible) for password hashing; otherwise BCrypt with strong cost.
- Password policy enforced at registration and reset.
- Email verification before activation.
- Short-lived access tokens; refresh tokens with rotation, revocation, and reuse detection.
- Brute-force protection and progressive throttling.
- MFA/TOTP architecture; Platform Super Admin requires stronger protection.
- Sessions are server-side revocable.

## Authorization

Granular permissions such as `products.view`, `products.create`, `inventory.adjust`, `sales.refund`, `reports.profit`, `settings.manage`. The cashier role is constrained: no employee management, no global settings, no other stores, no profit reports, no platform administration, no unrestricted inventory adjustments.

## Data protection

- Secrets are never committed and never logged.
- PAN/CVV/PIN are never stored — only safe external references.
- Database backups are encrypted at rest and verified by restore.
- Production database is private, least-privilege, and not exposed to the public internet.

## API hardening

- DTOs at the API boundary. No entity exposure.
- Rate limiting and progressive throttling.
- Strict CORS in production.
- HSTS, CSP, X-Content-Type-Options, Referrer-Policy, Permissions-Policy, frame protection.
- Idempotency keys on financial endpoints.
- Webhook signature verification, timestamp validation, replay protection, idempotency.

## Device security

- Device activation with short-lived, one-time, non-reusable, auditable codes.
- Local sensitive data is encrypted.
- Credentials use OS secure storage.
- Devices are revocable from authorized administration.

## Required test coverage

See Section 74. Authentication, authorization, tenant isolation, store isolation, BOLA/IDOR, privilege escalation, rate limiting, token rotation, token reuse, password reset, MFA, SQLi, XSS, CSRF, SSRF, path traversal, file upload, webhook replay, offline manipulation, duplicate operations, and device revoke are all explicitly tested.

## Reporting

Suspected vulnerabilities are not to be filed in public issue trackers. Contact the security team through the channel defined in `infrastructure/security/` (to be created).
