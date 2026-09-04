# ADR-001: Modular monolith

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

SalekhPos must serve many tenants, stores, and POS terminals with tight coupling between sales, inventory, cash, payments, and audit. Cross-aggregate transactions are the norm (a sale touches inventory, cash, audit, outbox). Early microservices would impose network cost, distributed transaction risk, and operational complexity before the architecture has stabilised.

## Decision

Build SalekhPos as a **modular monolith**: a single deployable backend whose internal structure is organised by bounded module (Identity, Tenancy, Catalog, Inventory, Purchasing, Sales, Customers, Cash, Expenses, Reporting, Platform), with each module owning its domain logic and exposing contracts through abstractions rather than reaching into another module's internals.

The four backend projects (`SalekhPos.Api`, `SalekhPos.Application`, `SalekhPos.Domain`, `SalekhPos.Infrastructure`) enforce the high-level dependency rules:

```text
Api -> Application -> Domain <- Infrastructure
```

## Consequences

- Transactions can span aggregates safely within the monolith.
- Module boundaries keep the code clean and make future extraction (if measured requirements justify it) feasible without a rewrite.
- We do not pay distributed-systems cost prematurely.
- The trade-off is a single deployment unit and shared database; horizontal scaling is by running more instances behind a load balancer, with PostgreSQL as the shared authoritative ledger.

## Alternatives considered

- **Microservices from day one.** Rejected: premature, no measured justification, increases correctness and operational risk for financial flows.
- **Single unstructured project.** Rejected: violates Section 5 of the constitution, which mandates the layered project split.

## References

- Constitution Section 5 (backend project structure)
- Constitution Section 138 (no premature microservices)
