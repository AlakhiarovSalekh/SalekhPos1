# SalekhPos Architecture Overview

SalekhPos is a multi-tenant retail Point-of-Sale and business management platform. It is engineered to be deployed to real businesses operating multiple stores, registers, employees, devices, and integration points, with strict requirements on financial correctness, inventory integrity, tenant isolation, and offline behaviour.

This document is the authoritative architectural overview. Detailed decisions live as ADRs in `docs/decisions/`.

## 1. High-level system

```text
                    +----------------------------+
                    |   Internet (TLS 1.2+)      |
                    +-------------+--------------+
                                  |
                                  v
                    +----------------------------+
                    | DNS -> Reverse Proxy / WAF |
                    |      (Caddy / nginx)       |
                    +-------------+--------------+
                                  |
              +-------------------+-------------------+
              |                                       |
              v                                       v
   +--------------------+                +-------------------------+
   |   Web (React/TS)   |                | Desktop POS (.NET)      |
   |   Owner / Manager  |                | Local-first, offline    |
   +--------------------+                +-------------------------+
              |                                       |
              |                                       |
              v                                       v
   +------------------------------------------------------------+
   |                 Mobile (.NET MAUI)                         |
   |   Owner / Manager monitoring, approvals, notifications     |
   +------------------------------------------------------------+
                                  |
                                  v
   +------------------------------------------------------------+
   |              ASP.NET Core Backend / API (/api/v1)          |
   |   SignalR (/hubs/v1)            Outbox -> Workers          |
   +---------+--------------+-----------------+-----------------+
             |              |                 |
             v              v                 v
        PostgreSQL        Redis        External Integrations
        (ledger)         (cache,       (fiscal, payment, SMS,
                         coordination)  email, push, accounting)
```

## 2. Module boundaries (modular monolith)

```text
HTTP
 |
 v
SalekhPos.Api          -- thin: routing, DTOs, auth pipeline, ProblemDetails
 |
 v
SalekhPos.Application  -- use cases, commands, queries, transactions
 |
 v
SalekhPos.Domain       -- entities, value objects, domain events, rules
 ^
 |
SalekhPos.Infrastructure -- EF Core, repositories, external adapters, Redis, jobs
```

Dependency rules:

- `Domain` depends on nothing project-internal.
- `Application` depends on `Domain` and on abstractions (interfaces) only.
- `Infrastructure` depends on `Application` (to implement its abstractions) and on `Domain`.
- `Api` depends on `Application` and `Infrastructure` (for composition root / DI). It does not contain business rules.
- `Tests` is a separate project that exercises the others.

## 3. Multi-tenancy

Each business is a tenant. A tenant owns one or more stores. Users have explicit membership and per-store access. Tenant identity is **never** accepted from the client; the backend derives the user's tenant context from the authenticated identity and verified memberships.

Every resource access goes through:

```text
Authenticated? -> Tenant valid? -> Store access valid?
-> Permission valid? -> Resource ownership valid?
-> Business rules valid? -> Execute
```

Changing a UUID must never allow access across tenants.

## 4. Inventory

Inventory is a **ledger**, not a mutable snapshot. The invariant is:

```text
opening_stock + sum(IN movements) - sum(OUT movements) = current_stock
```

Snapshot tables exist for performance. Concurrent writes are protected at the database level (row-level locks, ledger writes, transactions). Last-write-wins is **not** used for inventory.

Initial costing strategy: **Weighted Average Cost**. The design leaves room for FIFO / specific identification without rewriting the inventory system.

## 5. Sales

A sale consists of `Sale`, `SaleItems`, `SalePayments`, `SaleDiscounts`, `SaleTaxes`, audit, and outbox event. Items preserve historical snapshots of product, SKU, barcode, tax, and price so later product edits do not rewrite history.

Totals are calculated server-side using configured rounding rules. Client-supplied totals are validated, never trusted.

The critical sale transaction is conceptually:

```text
BEGIN TRANSACTION
  Authenticate -> Tenant -> Store -> Device -> Idempotency
  Load products, validate status, prices, quantities
  Calculate subtotal, apply discounts, calculate taxes
  Validate payments, register, session
  Create Sale, SaleItems, SalePayments
  Create InventoryTransactions, update snapshot
  Create cash transaction
  Create audit, outbox event
COMMIT
```

External calls (fiscal, payment, email, push) are not run inside the transaction; they are delivered via the outbox and processed by workers.

## 6. Offline-first POS

The Desktop POS is local-first. It maintains an encrypted local store, a sync queue, and a sync engine. When the network is unavailable, the cashier can continue allowed operations; on reconnection the sync engine replays operations to the server using globally unique operation IDs. Conflicts on critical state (stock, finance) are surfaced for reconciliation rather than silently overwritten.

## 7. Realtime

Realtime updates are delivered via SignalR groups scoped to `tenant:{tenantId}` and `store:{storeId}`. The server authorizes group membership. SignalR is not the source of truth; a missed realtime message is recovered via API reconciliation.

## 8. Integrations

All external integrations sit behind interfaces:

```text
IFiscalDevice         IPaymentProvider      IBarcodeScanner
IReceiptPrinter       ICashDrawer           IWeighingScale
IPaymentTerminal      IEmailProvider        ISmsProvider
IPushNotificationProvider                   IObjectStorage
IAccountingProvider   IReconciliationService
```

The architecture must never couple core business logic to a vendor SDK. Provider adapters live in `SalekhPos.Infrastructure/Integrations/`. The fiscal adapter for `RS.ge` is implemented behind `IFiscalDevice`; provider-specific behaviour is verified against authoritative documentation before being implemented.

## 9. Security posture

- Server-side authorization is authoritative. Frontend permission checks are UX only.
- All authenticated users are issued short-lived access tokens and refresh tokens with rotation, revocation, and reuse detection.
- Argon2id (where compatible) is the password hashing baseline; MFA/TOTP architecture is in place.
- Tenant isolation is enforced in every query path. UUID guessing must not cross tenants.
- BOLA/IDOR, SQLi, XSS, CSRF (where applicable), SSRF, path traversal, file upload abuse, mass assignment, and replay attacks are explicitly in the threat model and tested.
- Production traffic is HTTPS only. HSTS, CSP, secure cookies, strict CORS, and appropriate security headers are mandatory.
- Secrets are never committed. They come from configuration / secret management and are never logged.

## 10. Quality and operations

- Migrations are reviewed, tested, and backward-compatible where possible.
- Automated backups are encrypted and **verified by restore**, not by assumption.
- Health endpoints (`/health/live`, `/health/ready`) and structured logs with correlation IDs are mandatory.
- Performance targets are engineering targets: e.g. local barcode lookup < 50 ms, normal API read p95 < 300 ms, sale completion p95 < 1 s excluding external latency.

## 11. Phasing

The platform is implemented in 26 phases (0 through 25), each gated by implementation, tests, build, security review, documentation, verification, and a logical commit. See `docs/development/IMPLEMENTATION_STATUS.md` for current state.
