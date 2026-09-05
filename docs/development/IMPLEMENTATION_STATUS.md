# Implementation Status

> **Purpose.** Track the actual implementation state of SalekhPos. This document is updated in the same commit as the change it describes, per the constitution (Section 135). It must not lag behind reality.

## Snapshot

| Field | Value |
|---|---|
| Current phase | **Phase 2 — Identity / Authentication** (complete) |
| Phases complete | Phase 0 (foundation, docs, ADRs, status doc); Phase 1 (infrastructure scaffold + .NET 8 build verification); Phase 2 (identity & auth — domain, application, infrastructure, API, web client, documentation) |
| Phases in progress | None |
| Last verified commit | `ef291ce` on `main` of `AlakhiarovSalekh/SalekhPos` |
| Next phase | Phase 3 — Multi-tenancy (tenants, stores, memberships, roles, permissions) |

## Phase progress

| # | Phase | State | Notes |
|---|---|---|---|
| 0 | Foundation | **done** | README, CONTRIBUTING, CHANGELOG, LICENSE, .gitignore, .editorconfig, docs/, ADRs, IMPLEMENTATION_STATUS.md, top-level folders. |
| 1 | Infrastructure | **partial** | .NET 8 solution scaffold (5 projects), React/TS/Vite web scaffold, Docker compose + Dockerfiles, CI workflow. Health endpoints stubbed. **Compiles on .NET 8 SDK 8.0.424**: `dotnet build` 0 warnings / 0 errors, `dotnet test` 4/4 passed, `npm run build` clean (Vite 5, 111 modules). Pending: full CI green, Postgres+Redis via Docker, OpenAPI/Swagger registration. |
| 2 | Identity / Auth | **done** | EdDSA-signed access tokens, opaque rotated refresh tokens with reuse detection, Argon2id password hashing, TOTP MFA + recovery codes, RFC 7807 error mapping, `token_version` revocation. 6 commits (`4f12a70`, `d51dee0`, `fee7496`, `faf8f09`, `91f9a00`, `ef291ce`). Documented in [ADR-009](../decisions/ADR-009-identity-and-auth.md), [docs/api/auth.md](../api/auth.md), [docs/security/auth.md](../security/auth.md). |
| 3 | Multi-tenancy | pending | tenants, stores, memberships, roles, permissions, store access, tenant isolation tests. |
| 4 | Catalog | pending | categories, brands, units, products, variants, barcodes, prices, taxes. |
| 5 | Inventory | pending | ledger, snapshot, movements, adjustments, transfers, stock counts, weighted average cost, concurrency control. |
| 6 | Purchasing | pending | suppliers, purchases, approval, receiving, supplier payments. |
| 7 | Core POS sales | pending | sale, items, pricing, discounts, tax, payments, inventory deduction, cash, audit, outbox, idempotency. |
| 8 | Desktop POS | pending | activation, login, register, shift, barcode, cart, checkout, payment, receipt, hardware abstractions, offline local DB. |
| 9 | Sync engine | pending | queue, operation IDs, retries, backoff, ordering, duplicate detection, conflicts, reconciliation, crash recovery. |
| 10 | Returns / refunds | pending | returns, partial returns, refunds, inventory disposition, cash/card refund rules. |
| 11 | Cash / expenses | pending | cash registers, sessions, cash transactions, expenses, expected/actual cash, difference, audit. |
| 12 | Reporting / analytics | pending | reports, dashboards, analytics, background generation, exports. |
| 13 | Realtime | pending | SignalR, tenant/store groups, realtime sales, inventory, notifications, reconnect, reconciliation. |
| 14 | Mobile | pending | .NET MAUI auth, dashboard, monitoring, inventory, products, employees, reports, notifications, approvals, settings. |
| 15 | Super Admin | pending | businesses, devices, subscriptions, usage, system health, security center, audit, support. |
| 16 | Integrations | pending | provider-independent fiscal/payment/email/SMS/push/storage/accounting. |
| 17 | Hardware certification | pending | scanner, printer, drawer, scale, fiscal, payment terminal — where available. |
| 18 | Security hardening | pending | full security review and test coverage. |
| 19 | Performance | pending | load, stress, spike, soak, concurrency, DB, sync, SignalR, reports, memory/CPU. |
| 20 | Production hardening | pending | HTTPS, reverse proxy, private DB, firewall, secrets, backups, monitoring, alerting, health checks, CI/CD, deployment strategy. |
| 21 | Disaster recovery | pending | backup restore, DB failure, API failure, worker failure, Redis failure, storage failure, device failure, network failure. |
| 22 | Final E2E | pending | full register-verify-business-store-product-supplier-purchase-receive-inventory-activate-POS-shift-sale-return-refund-close-report flow. |
| 23 | Release candidate | pending | feature freeze, only bugfix/security/critical perf/doc. |
| 24 | Final quality gate | pending | build, unit, integration, API, E2E, security, tenant isolation, perf, offline/sync, hardware, migrations, backup/restore, a11y, l10n, docs, health, smoke. |
| 25 | Final audit | pending | end-to-end review of architecture, DB, API, security, web, desktop, mobile, sync, realtime, integrations, performance, testing, DevOps, documentation, git. |

## Completed work in this scaffold (Phase 0 + partial Phase 1)

- Repository foundation: README, CONTRIBUTING, CHANGELOG, LICENSE, .gitignore, .editorconfig.
- Top-level folder structure: `backend/`, `web/`, `mobile/`, `desktop/`, `infrastructure/`, `scripts/`, `tests/`.
- Documentation under `docs/`: `architecture/`, `api/`, `database/`, `security/`, `deployment/`, `testing/`, `integrations/`, `decisions/`, `operations/`, `development/`.
- Initial ADRs: ADR-001 through ADR-008 (modular monolith, PostgreSQL, offline-first POS, .NET MAUI mobile, SignalR, multi-tenancy, inventory ledger, idempotency).
- Backend .NET 8 solution skeleton (`SalekhPos.sln` + `SalekhPos.Api`, `.Application`, `.Domain`, `.Infrastructure`, `.Tests`) with project references following the architecture rules.
- Health endpoints (`/health/live`, `/health/ready`) and a `ProblemDetails` writer in `SalekhPos.Api`.
- Web scaffold: React 18 + TypeScript + Vite, with the feature-folder layout from the constitution. i18n for `ka`, `en`, `az`. BusinessShell placeholder.
- Docker dev environment: `docker-compose.yml` with PostgreSQL 16, Redis 7, Caddy reverse proxy, backend, web, and worker services. Multi-stage non-root Dockerfiles.
- CI workflow: checkout, restore, build, unit tests, lint, security scan, container build (no deploy).
- Placeholder remote config note: a `salekhpos` remote placeholder was added (URL pending) per user instruction not to touch the existing `origin`.

## Phase 1 (Infrastructure) — complete (verified on commit `b4ea603`)

`.NET 8 SDK 8.0.424`: `dotnet build` 0 warnings / 0 errors, `dotnet test` 4/4 passed, `npm run build` clean (Vite 5, 111 modules). The CI gate remains a follow-up to this slice.

## Phase 2 (Identity / Auth) — complete (verified on commit `ef291ce`)

Six slices shipped in order:

| Slice | Commit | What |
|---|---|---|
| 1 — Domain | `4f12a70` | `User`, `Tenant`, value objects (`Email`, `Password`, `TenantSlug`), token primitives, error model (6 exceptions: `InvalidCredentials`, `AccountLocked`, `EmailNotVerified`, `PasswordPolicy`, `RateLimited`, `InvalidToken`). |
| 2 — Application | `d51dee0` | 15 abstractions, `PasswordPolicy`, 11 use-case handlers, FluentValidation, `DependencyInjection`. |
| 3a — Infrastructure crypto | `fee7496` | Argon2id, Ed25519 JWT signer, TOTP, AES-GCM, opaque tokens, structured email logger, system clock. |
| 3b — Infrastructure persistence | `faf8f09` | DbContext, 7 EF configurations, 7 repositories, UnitOfWork, DI updates. |
| 4 — API surface | `91f9a00` | `AuthController` (11 endpoints), DTOs, `EdDsaAuthenticationHandler`, `DomainExceptionMapping` (RFC 7807). Build 0/0, tests 23/23. |
| 5 — Web client | `ef291ce` | React 18 + TS auth feature: `AuthProvider` with mount-time silent refresh, 8 pages, 4 UI primitives, 4 auth components, router with `RequireAuth`/`RequireAnonymous`/`RequireMfaPending` guards, i18n in 3 locales (en/ka/az) with a parity test, 48 unit tests + Playwright e2e smoke. `tsc 0 errors`, `eslint 0 warnings`, `vitest 48/48`, `vite build` clean. |

New documentation in this phase:

- [ADR-009 — Identity and authentication](../decisions/ADR-009-identity-and-auth.md) — the seven decisions: EdDSA JWT, Argon2id, opaque rotated refresh tokens, `token_version` revocation, TOTP MFA, RFC 7807, `localStorage` trade-off.
- [API reference — auth](../api/auth.md) — every endpoint, request/response shapes, error codes, status codes, source links.
- [Security reference — auth](../security/auth.md) — threat model, password policy rationale, token storage trade-off, rotation strategy, MFA recovery codes, account lockout, audit log.

Deferred to a Slice 4b follow-up: ASP.NET Core rate limiter (Redis-backed partition), EF Core initial migration, `appsettings.Development.json` Postgres connection string, OpenAPI/Swagger registration.

## Pending work (immediate next steps)

1. Begin Phase 3: multi-tenancy (tenants, stores, memberships, roles, permissions, store access, tenant isolation tests).
2. Slice 4b (optional, can land any time after Phase 2): rate limiter, Redis partition, EF Core migration, `appsettings.Development.json` connection string, OpenAPI registration.

## Toolchain gaps (verified at scaffold time)

The following were **not present** on the machine that produced this scaffold:

- **.NET 8 SDK** — RESOLVED. `dotnet --list-sdks` reports `8.0.424`. The solution builds and tests pass on the user's machine.
- **Docker / Docker Desktop** — Installed at `C:\Program Files\Docker\Docker\resources\bin\docker` (not on the bash PATH by default; PowerShell or full path works). `docker compose` is available. The dev `docker-compose.yml` can be brought up.
- **PostgreSQL / psql client** — not installed locally; not required because the dev environment provides PostgreSQL through Docker.

Node.js 24.18.0 and npm 11.16.0 are installed.

## Toolchain / verification evidence

| Check | Command | Result |
|---|---|---|
| Git remote exists | `git remote -v` | `salekhpos` (placeholder URL `…/AlakhiarovSalekh/SalekhPos.git`). The `origin` of the home repo is not present in this repo. |
| Working tree before commit | `git status` | only the files added by this scaffold (150 files, 4,450 insertions). |
| `git log` after commit | `git log --oneline` | `4cad227 chore(repo): initial SalekhPos foundation (Phase 0 + Phase 1 scaffold)`. |
| `.NET 8 SDK` | `dotnet --list-sdks` | empty (no SDKs found). Build was not run; will be verified by the user. |
| Node | `node --version` | `v24.18.0`. |
| npm | `npm --version` | `11.16.0`. |
| Docker | `docker --version` | not found. |
| psql | `psql --version` | not found. |
| Secret scan | grep staged diff for RSA / OPENSSH / API key / password / secret patterns | no matches. |
| Home repo integrity | `cd C:/Users/ASUS && git log --oneline -1` | still `1a3bc6b Initial commit`. **Not modified.** |

## Repository hygiene

- No secrets are committed. All `*.local.json` and `*.env*` files are ignored; an `.env.example` is provided.
- No force pushes. No history rewrite. The existing initial commit (which contains a separate, unrelated project) is preserved.
- `origin` is not modified. The `salekhpos` remote is added as a placeholder so the user can point it at the new GitHub `SalekhPos` repository.

## Known issues / blockers

- The initial commit `1a3bc6b` does not contain SalekhPos code. It contains a separate, unrelated project. It is kept in history for traceability. The first real SalekhPos commit is added on top.
- The `origin` remote points to the unrelated project. The user will create a new GitHub repository named `SalekhPos` and configure a remote (e.g. `salekhpos`) to point at it. The scaffold does not push.
- A fresh git repository was initialised inside `SalekhPos/` (the only `.git` in this directory). The user's home git repository (`C:/Users/ASUS/.git`, the unrelated Qt expense tracker) was not modified.
- A placeholder remote named `salekhpos` was added at `https://github.com/AlakhiarovSalekh/SalekhPos.git`. The user should create the GitHub `SalekhPos` repository, then run `git remote set-url salekhpos <actual-url>` and `git push -u salekhpos main` from inside `SalekhPos/`.

## Architectural decisions

- See `docs/decisions/ADR-001..ADR-009.md` (ADR-009 covers Phase 2 identity and auth).

## Migrations

- None yet. EF Core migrations will be created from Phase 2 onward, in `backend/SalekhPos.Infrastructure/Migrations/`.

## Last verified commit

- `ef291ce` — `feat(web): Phase 2 auth feature — login/register/MFA/reset, refresh-on-401, routing (Slice 5)`. 57 files, +3,929 / -86. Brings the auth feature end-to-end: service layer with refresh-on-401 single-flight, `AuthProvider` with mount-time silent refresh, 4 UI primitives, 4 auth components, 9 auth pages, router with 3 guards, 404 page, i18n in en/ka/az with a parity test, 48 unit tests + Playwright e2e smoke. Pushed to `AlakhiarovSalekh/SalekhPos` (remote `salekhpos`). **Verified:** backend `dotnet build` 0/0, `dotnet test` 23/23; web `tsc 0 errors`, `eslint 0 warnings`, `vitest 48/48`, `vite build` clean.

## Next phase

- **Phase 3 — Multi-tenancy.** Tenant management, store management, memberships, roles, permissions, store access, tenant isolation tests. The Phase 2 auth feature issues the `UserId` and `TenantId` that Phase 3 will scope every request by.
