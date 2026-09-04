# SalekhPos Technical Constitution (condensed reference)

The full 153-section constitution is reflected across the documentation in `docs/`. This file is a condensed, scannable reference and points to the canonical sources.

## Stack (locked)

- **Backend:** C#, ASP.NET Core, .NET 8, EF Core, PostgreSQL 16, Redis 7, SignalR, background workers, REST `/api/v1/`.
- **Web:** React 18, TypeScript, Vite.
- **Mobile:** .NET MAUI (Android, iOS).
- **Desktop POS:** native .NET, not a browser shell. Local-first, offline-capable.
- **Database:** PostgreSQL 16, UUID PKs, numeric decimals for money, UTC timestamps.

## Module / dependency rules

```text
Api -> Application -> Domain <- Infrastructure
```

Domain has no infrastructure dependencies. Application depends on abstractions. Infrastructure implements those abstractions. Api is HTTP-only — no business rules.

## Key invariants

- Money: numeric/decimal only. No floats.
- Inventory: ledger model. Invariant `opening + IN - OUT = current`. Concurrency-protected.
- Sales: server-authoritative totals, historical snapshots, idempotency, audit, outbox.
- Tenants: derived from authenticated identity, never trusted from the client.
- External integrations: behind interfaces, never inside critical transactions, with retry/jitter, circuit breakers, and reconciliation. `UNKNOWN` stays `UNKNOWN` until verified.

## Mandatory patterns

- **Idempotency keys** for sale, payment, refund, purchase, transfer, adjustment, sync, webhook.
- **Outbox** for side-effects triggered by a successful transaction.
- **SignalR** for realtime updates scoped to `tenant:{id}` / `store:{id}`; never source of truth.
- **DTOs** at the API boundary; entities are not exposed.
- **ProblemDetails** for errors. No leaky stack traces in production.
- **Structured logs** with `requestId`, `userId`, `tenantId`, `storeId`, `deviceId`. Never log secrets.
- **Health endpoints** `/health/live` and `/health/ready`.
- **Migrations** are reviewed and tested. Backward-compatible where possible.

## Authoritative sources for external behaviour

- Microsoft docs for .NET/ASP.NET Core/EF Core/.NET MAUI/SignalR.
- PostgreSQL official documentation.
- React, TypeScript, Vite official documentation.
- Official payment provider and official RS.ge documentation.

Invented APIs, invented endpoints, invented protocol details, and invented provider behaviour are forbidden.

## What lives where

| Concern | Backend | Web | Desktop POS | Mobile |
|---|---|---|---|---|
| Authentication flow | ✓ | UX only | ✓ | ✓ |
| Authorization decisions | ✓ | never | never | never |
| Tenant / store context | ✓ | never | requests it | requests it |
| Product, price, tax, totals | ✓ (authoritative) | display | display | display |
| Inventory ledger | ✓ | display | local cache + sync | display |
| Sale creation | ✓ | n/a | n/a | n/a |
| Hardware (scanner/printer/drawer/scale) | n/a | n/a | ✓ | n/a |
| Fiscal / payment integration | ✓ via adapters | n/a | uses backend | n/a |
| Offline operation | n/a | n/a | ✓ local-first | cache only |
| Realtime | ✓ SignalR | ✓ subscribe | ✓ subscribe | ✓ subscribe |

## Phases

26 phases, 0–25. Each phase has a gate. The current phase, completed work, and pending work are tracked in `docs/development/IMPLEMENTATION_STATUS.md`.

## Security principles

- Security by Design, Defense in Depth, Least Privilege, Zero Trust, Auditability, Fail Secure.
- Threat model: assume HTTP requests, tenant IDs, store IDs, prices, replay, duplicate, stolen sessions, stolen devices, manipulated offline data, SQLi, XSS, file upload abuse, webhook replay, API abuse, resource exhaustion.

## Performance principles

- Engineering targets, not magical guarantees. Measure first; identify bottleneck; smallest effective change; benchmark; verify no regression.
- Never improve performance by weakening correctness, security, tenant isolation, transaction integrity, inventory integrity, or financial consistency.
