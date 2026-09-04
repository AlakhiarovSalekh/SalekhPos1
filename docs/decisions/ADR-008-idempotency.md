# ADR-008: Idempotency

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

A cashier pressing "complete sale" twice, a sync engine replaying an operation, a payment provider retrying a webhook, or a network blip causing a client retry must not create duplicate financial effects. SalekhPos processes sales, payments, refunds, purchases, transfers, inventory adjustments, sync operations, and webhooks — all of which must be safely retryable.

## Decision

Financial and state-changing operations are **idempotent by key**. An `idempotency_keys` table records the operation type, key, tenant, result (or reference to the completed aggregate), and timestamps. When the same operation is submitted twice with the same key, the server returns the original result without re-executing the side effects.

Idempotency applies to at least: sale, payment, refund, purchase, transfer, inventory adjustment, synchronization, and webhook processing.

## Consequences

- Every financial endpoint accepts an idempotency key (e.g. `Idempotency-Key` header) and stores the outcome.
- The check-then-execute is atomic with the business transaction; a duplicate concurrent request is rejected or served the cached result, never both.
- Sync operations carry a globally unique operation ID; the server deduplicates on it.
- Webhook processing verifies signature, timestamp, replay protection, and idempotency before persisting/processing the event.
- UNKNOWN payment/fiscal states are never retried blindly; they are reconciled first.

## Alternatives considered

- **Client-side deduplication only.** Rejected: clients are untrusted; the server must be authoritative.
- **Tolerate duplicates and reconcile later.** Rejected: financial integrity requires prevention, not cleanup.

## References

- Constitution Section 21, 24, 26, 37, 38, 100, 101, 144
