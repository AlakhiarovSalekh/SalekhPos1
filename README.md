# SalekhPos

Professional multi-tenant retail Point-of-Sale and business management platform.

SalekhPos is intended to be deployed to real businesses, multiple stores, multiple computers, POS terminals, mobile devices, and a large number of independent business tenants. It prioritises correctness, security, financial integrity, inventory integrity, tenant isolation, reliability, performance, maintainability, observability, and recoverability.

## Platform components

| Component | Stack | Purpose |
|---|---|---|
| Web | React, TypeScript, Vite | Business / manager / owner administration |
| Desktop POS | .NET (native Windows) | Local-first cashier terminal with offline mode |
| Mobile | .NET MAUI (Android, iOS) | Owner / manager monitoring, approvals, notifications |
| Backend | ASP.NET Core, EF Core, PostgreSQL, SignalR | Central API, realtime, outbox, integrations |
| Workers | .NET background services | Sync reconciliation, report generation, webhooks, fiscal jobs |
| Database | PostgreSQL 16 | Authoritative ledger for inventory, sales, payments, audit |
| Cache / state | Redis 7 | Rate limiting, coordination, short-lived state (never financial source of truth) |

## Repository layout

```text
SalekhPos/
├── README.md
├── CONTRIBUTING.md
├── CHANGELOG.md
├── LICENSE
├── .gitignore
├── .editorconfig
├── docs/
│   ├── architecture/
│   ├── api/
│   ├── database/
│   ├── security/
│   ├── deployment/
│   ├── testing/
│   ├── integrations/
│   ├── decisions/
│   ├── operations/
│   └── development/
├── backend/                 # SalekhPos.Api, .Application, .Domain, .Infrastructure, .Tests
├── web/                     # salekhpos-web (React + TS + Vite)
├── desktop/                 # SalekhPos.Desktop (native Windows POS)
├── mobile/                  # SalekhPos.Mobile (.NET MAUI)
├── infrastructure/          # Docker, compose, reverse proxy, observability config
├── scripts/                 # Local dev, codegen, maintenance scripts
└── tests/                   # Cross-cutting / e2e / contract tests
```

API base: `/api/v1/`. Architecture: `Api -> Application -> Domain <- Infrastructure`. Controllers stay thin; business rules live in Domain/Application.

## Status

This repository is in **Phase 0 / early Phase 1** of the documented implementation plan. The current implementation state, toolchain requirements, and next steps live in:

[`docs/development/IMPLEMENTATION_STATUS.md`](docs/development/IMPLEMENTATION_STATUS.md)

The detailed technical constitution (153 sections) is summarised inline in `docs/architecture/ARCHITECTURE.md` and tracked per-ADR in `docs/decisions/`.

## Toolchain requirements (verified at scaffold time)

| Tool | Version | Status |
|---|---|---|
| .NET SDK | 8.0 | **Not installed on the development machine used to scaffold.** Source files are written; a SDK is required to compile. |
| Node.js | 20 LTS or 24 | Installed (24.18.0). |
| npm | 10+ | Installed (11.16.0). |
| Docker Desktop | 4.x | Not installed; `docker-compose.yml` is provided for when it is available. |
| PostgreSQL | 16 | Provided via Docker. |
| Redis | 7 | Provided via Docker. |

## Development

```bash
# Backend (once .NET 8 SDK is installed)
cd backend
dotnet build
dotnet test

# Web
cd web/salekhpos-web
npm install
npm run dev

# Full local stack (once Docker is installed)
docker compose -f infrastructure/docker/docker-compose.yml up
```

## License

See [LICENSE](LICENSE).
