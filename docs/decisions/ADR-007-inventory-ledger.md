# ADR-007: Inventory ledger

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

Inventory in a POS is financial-grade data. Direct mutable stock columns lead to lost movements, unexplained drift, and concurrency corruption when two cashiers sell the same item simultaneously. Last-write-wins is unacceptable for critical inventory.

## Decision

Model inventory as an **append-only ledger** of movements. The current stock for a product/store is:

```text
current = opening_stock + sum(IN movements) - sum(OUT movements)
```

Movement types:

```text
OPENING, PURCHASE, SALE, SALE_RETURN, PURCHASE_RETURN,
ADJUSTMENT_IN, ADJUSTMENT_OUT, TRANSFER_IN, TRANSFER_OUT, DAMAGE, EXPIRED
```

Snapshot tables may exist for read performance, but they are derived from the ledger and never the source of truth.

Costing uses **Weighted Average Cost** initially. The design must allow adding FIFO and specific identification later without rewriting the inventory system.

## Consequences

- Every meaningful adjustment has a reason, user, timestamp, store, quantity, and audit information.
- Concurrency is protected by transactions, row/version concurrency, atomic ledger writes, and locking where justified. The forbidden pattern is `read -> calculate -> write` without concurrency protection.
- Transfers move `TRANSFER_OUT` at the source and `TRANSFER_IN` at the destination through an explicit lifecycle; stock is never silently created at the destination.
- Returns reference the original sale; allowed return quantity is `originally sold - already returned`. Partial returns are supported.

## Alternatives considered

- **Mutable stock column.** Rejected: no audit trail, no concurrency safety, cannot reconstruct history.
- **Last-write-wins with a stock column.** Rejected by Section 15.

## References

- Constitution Section 14, 15, 16, 42, 43, 76, 104, 143
