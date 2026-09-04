# Database (PostgreSQL)

SalekhPos uses **PostgreSQL 16** as the authoritative database. Money is stored as `numeric`/`decimal`. Quantities support fractional precision where the product allows it. All timestamps are stored in UTC; tenant/store timezones are applied in presentation and business-context operations.

## Identity

```text
users
roles
permissions
user_roles
user_store_access
user_tenants
```

## Platform

```text
tenants
stores
devices
```

## Catalog

```text
products
categories
brands
units
product_variants
product_barcodes
product_prices
tax_rates
```

## Inventory

```text
inventories             -- snapshot, derived from ledger
inventory_transactions  -- ledger
stock_counts
stock_count_items
stock_transfers
stock_transfer_items
```

Inventory invariant: `opening + IN - OUT = current`. Movements are append-only.

## Purchasing

```text
suppliers
purchases
purchase_items
supplier_payments
```

Purchases: `DRAFT -> SUBMITTED -> APPROVED -> RECEIVED -> COMPLETED`. Receiving creates inventory movements.

## Sales

```text
sales
sale_items
sale_payments
sale_discounts
sale_taxes
returns
return_items
```

Sale items preserve historical snapshots (name, SKU, barcode, tax, price). Returns reference the original sale; partial returns are supported.

## Customers

```text
customers
customer_addresses
customer_accounts
customer_transactions
```

## Cash

```text
cash_registers
cash_register_sessions
cash_transactions
```

Session states: `OPEN, ACTIVE, CLOSING, CLOSED`. A session cannot be closed twice.

## Expenses

```text
expense_categories
expenses
```

## System

```text
audit_logs
notifications
sync_operations
idempotency_keys
```

## Subscriptions

```text
subscription_plans
subscriptions
subscription_events
```

States: `TRIAL, ACTIVE, PAST_DUE, GRACE, SUSPENDED, CANCELLED, DEACTIVATED`. Limits may include stores, users, devices, products, monthly sales, storage, reports, and advanced features.

## Migrations

Migrations are reviewed, tested, and backward-compatible where possible. Destructive changes require explicit impact analysis. The schema is owned by EF Core migrations under `backend/SalekhPos.Infrastructure/Migrations/`.

## Performance

Keyset pagination, projections, `AsNoTracking`, indexes, connection pooling, batching, and streaming are used where appropriate. Avoid N+1, unbounded queries, and giant payloads.
