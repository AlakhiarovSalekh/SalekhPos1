# Implementation Status

> **Purpose.** Track the actual implementation state of SalekhPos. This document is updated in the same commit as the change it describes, per the constitution (Section 135). It must not lag behind reality.

## Snapshot

| Field | Value |
|---|---|
| Current phase | **Phase 1 — Infrastructure** (foundation work for Phase 0/1 merged at scaffold time) |
| Phases complete | Phase 0 (foundation, docs, ADRs, status doc) and partial Phase 1 (solution/web/docker/CI scaffolds) |
| Phases in progress | Phase 1 (CI gate, real builds, real tests) |
| Last verified commit | `4cad227` on `main` of the local `SalekhPos/` repository |
| Next phase | Phase 2 — Identity / Authentication |

## Phase progress

| # | Phase | State | Notes |
|---|---|---|---|
| 0 | Foundation | **done** | README, CONTRIBUTING, CHANGELOG, LICENSE, .gitignore, .editorconfig, docs/, ADRs, IMPLEMENTATION_STATUS.md, top-level folders. |
| 1 | Infrastructure | **partial** | .NET 8 solution scaffold (5 projects), React/TS/Vite web scaffold, Docker compose + Dockerfiles, CI workflow. Health endpoints stubbed. **Not yet compiled** — see "Toolchain gaps". |
| 2 | Identity / Auth | pending | Argon2id, password policy, email verification, password reset, access/refresh tokens, rotation, revocation, reuse detection, MFA/TOTP architecture, rate limiting, brute-force protection. |
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

## Pending work (immediate next steps)

1. Install .NET 8 SDK on the development machine and run `dotnet build` / `dotnet test` to verify the solution compiles and tests pass.
2. Run `npm install` and `npm run build` in `web/salekhpos-web/` to verify the Vite/TS app builds.
3. Add a working Postgres + Redis via Docker (or local) and run the integration test fixtures.
4. Wire the real `IFiscalDevice` and `IPaymentProvider` adapter skeletons (no provider-specific code without authoritative documentation).
5. Begin Phase 2: identity, authentication, refresh-token rotation, reuse detection, MFA architecture, rate limiting.

## Toolchain gaps (verified at scaffold time)

The following were **not present** on the machine that produced this scaffold:

- **.NET 8 SDK** — `dotnet` is on disk but `dotnet --list-sdks` reports no installed SDK. Source files compile in `dotnet new`-generated structure, but the user must install the SDK to actually build.
- **Docker / Docker Desktop** — no `docker` binary found. The `docker-compose.yml` is provided for use when Docker is available.
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

- See `docs/decisions/ADR-001..ADR-008.md`.

## Migrations

- None yet. EF Core migrations will be created from Phase 2 onward, in `backend/SalekhPos.Infrastructure/Migrations/`.

## Last verified commit

- `4cad227` — `chore(repo): initial SalekhPos foundation (Phase 0 + Phase 1 scaffold)`. 150 files, 4,455 insertions. Branch `main`, working tree clean. The `origin` remote from the unrelated home repository is not present here; only the placeholder `salekhpos` remote was added. **The home repository was not modified.**

## Next phase

- **Phase 2 — Identity / Authentication.** Argon2id, password policy, email verification, password reset, access/refresh tokens, rotation, revocation, reuse detection, MFA/TOTP architecture, rate limiting, brute-force protection.
