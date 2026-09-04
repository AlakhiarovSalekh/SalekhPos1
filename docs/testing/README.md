# Testing

Testing follows the pyramid:

```text
Unit
 -> Integration
 -> API/UI
 -> E2E
```

Plus domain-specific suites: security, performance, offline, sync, hardware, accessibility, localization, recovery, migration, backup/restore.

## Backend

```text
backend/SalekhPos.Tests/
├── Unit/
├── Integration/
├── Security/
├── Performance/
└── Fixtures/
```

## Web

```text
web/salekhpos-web/tests/
├── unit/
├── components/
├── features/
├── integration/
├── accessibility/
└── e2e/
```

## Desktop POS

```text
desktop/SalekhPos.Desktop/Tests/
├── Unit/
├── Integration/
├── Offline/
├── Sync/
├── Hardware/
└── Recovery/
```

## Mobile

```text
mobile/SalekhPos.Mobile/Tests/
├── Unit/
├── Integration/
├── UI/
└── Security/
```

## Required suites

- **Security** (Section 74): authn, authz, tenant isolation, store isolation, BOLA/IDOR, privilege escalation, rate limits, token rotation/reuse, password reset, MFA, SQLi, XSS, CSRF (where applicable), SSRF, path traversal, file upload, webhook replay, offline manipulation, duplicate operations, device revoke.
- **Financial** (Section 75): rounding, discounts, tax, mixed payments, cash change, refunds, partial refunds, payment mismatch, sale rollback, duplicate sale/payment/refund, concurrent sales, cash difference, immutable completed transactions.
- **Inventory** (Section 76): opening, purchase, sale, return, adjustment, transfer, damage, expiry, concurrency, offline, sync, recovery; ledger invariant `opening + IN - OUT = current`.
- **Offline** (Section 77): internet loss, offline sale, app restart, internet restore, sync, duplicate sync, sync interruption, server timeout, device crash, conflict, recovery.
