# ADR-003: Offline-first Desktop POS

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

A cashier cannot stop serving customers because the network is down. SalekhPos supports stores with unreliable connectivity, and a POS terminal must remain fast during normal cashier operations (Section 85: barcode scanning must not require a server round trip for every scan).

## Decision

The Desktop POS is **local-first and native**, not a browser wrapper around the web app. It maintains an encrypted local database (catalog cache, prices, categories, store configuration, pending sales, completed local transactions, sync queue, device state). Allowed POS operations continue offline. A durable sync queue with globally unique operation IDs replays operations to the server when connectivity returns.

Sync uses:

- Durable queue with status `PENDING | PROCESSING | COMPLETED | FAILED | CONFLICT`.
- Exponential backoff with jitter and retry limits (e.g. 2s, 5s, 15s, 30s, 60s, configurable).
- Duplicate operation detection via idempotency keys.
- Explicit conflict reconciliation for stock, financial records, payments, and critical sales — never blind last-write-wins.
- Crash recovery: a restart inspects durable local state, finds incomplete operations, and resumes/reconciles.

## Consequences

- A crash must not silently duplicate a sale, lose a completed operation, or corrupt the queue.
- The server ledger remains authoritative; the client is a cache and a queue.
- Sensitive local data is encrypted; credentials/secrets use OS secure storage.
- Hardware (scanner, printer, drawer, scale, fiscal, payment terminal) is accessed through interfaces (`IBarcodeScanner`, `IReceiptPrinter`, `ICashDrawer`, `IWeighingScale`, `IFiscalDevice`, `IPaymentTerminal`) with vendor adapters.

## Alternatives considered

- **Browser-based POS.** Rejected: violates Sections 7 and 85; cannot meet local-first / offline / hardware requirements.
- **Always-online POS.** Rejected: violates the offline-first requirement and the cash-flow continuity requirement.

## References

- Constitution Sections 7, 22, 23, 24, 25, 26, 27, 28, 55, 77, 85, 149
