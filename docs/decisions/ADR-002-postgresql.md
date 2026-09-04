# ADR-002: PostgreSQL as the authoritative database

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

SalekhPos needs a database that supports strong transactional integrity, exact numeric types for financial values, row-level concurrency for inventory, UUID primary keys for offline/distributed identity, and good operational tooling for backups and point-in-time recovery.

## Decision

Use **PostgreSQL 16** as the authoritative database via EF Core. Money is stored as `numeric`/`decimal` (never floating point). Quantities support fractional precision where the product allows it. All timestamps are stored in UTC; tenant/store timezones are applied only in presentation. UUID primary keys are used for distributed/offline identity; human-readable business numbers (e.g. `SL-20260904-000182`) are used where a human-facing identifier is needed.

## Consequences

- Financial arithmetic is exact at the database level.
- EF Core migrations are the schema-evolution mechanism, reviewed and tested.
- Connection pooling, keyset pagination, `AsNoTracking`, and proper indexing are used to meet the performance targets in Section 62.
- Redis may be used for cache, rate limiting, and coordination but is **never** the source of truth for sales, inventory, financial records, or payments.

## Alternatives considered

- **SQL Server.** Rejected: the stack is locked to PostgreSQL by the constitution.
- **MongoDB / document store.** Rejected: financial integrity and transactional ledger semantics are core; a relational ledger is the correct model.

## References

- Constitution Section 4, 9, 14, 15, 16, 54, 64
