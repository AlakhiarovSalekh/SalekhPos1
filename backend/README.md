# SalekhPos backend (.NET 8)

Modular monolith organised by bounded modules inside the layered project structure required by the constitution:

```text
src/
  SalekhPos.Domain            (entities, value objects, domain events, invariants)
  SalekhPos.Application       (use cases, commands, queries, orchestration)
  SalekhPos.Infrastructure    (EF Core, PostgreSQL, Redis, external adapters)
  SalekhPos.Api               (HTTP, DTOs, auth pipeline, ProblemDetails, health)
  SalekhPos.Worker            (background services: outbox, sync, reports, webhooks)
tests/
  SalekhPos.Tests             (Unit, Integration, Security, Performance, Fixtures)
```

## Build & test

```bash
cd backend
dotnet restore
dotnet build
dotnet test
```

`global.json` pins the SDK to `8.0.0` (with `rollForward: latestFeature`). The solution uses central package management via `Directory.Packages.props` and shared build settings via `Directory.Build.props`.

## API

- Base version: `/api/v1/`
- Health: `GET /health/live`, `GET /health/ready`
- System: `GET /api/v1/system/info`

Business endpoints (sales, products, inventory, etc.) are introduced from Phase 2 onward.

## Notes

- The `Application` layer must not reference `Infrastructure`. This is verified by `SalekhPos.Tests.Unit.ArchitectureBoundaryTests`.
- Money is stored as `numeric`/`decimal`. UTC timestamps.
- Secrets are not committed; use user-secrets in development and a secret manager in production.
