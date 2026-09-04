# Integrations

External integrations sit behind interfaces. Provider-specific code lives in adapter assemblies under `SalekhPos.Infrastructure/Integrations/`. Core business logic must not depend on a vendor SDK.

```text
IFiscalDevice
IPaymentProvider
IBarcodeScanner
IReceiptPrinter
ICashDrawer
IWeighingScale
IPaymentTerminal
IEmailProvider
ISmsProvider
IPushNotificationProvider
IObjectStorage
IAccountingProvider
IReconciliationService
```

## Fiscal

Provider-independent. `RS.ge` is one adapter; reconciliation supports `NOT_REQUIRED, PENDING, PROCESSING, FISCALIZED, FAILED, UNKNOWN, REQUIRES_RECONCILIATION`. `UNKNOWN` stays `UNKNOWN` until verified.

Do not invent `RS.ge` endpoints, request formats, authentication, or fiscal rules. Before implementing provider-specific behaviour, verify against the current authoritative documentation. Where the exact current behaviour is unavailable, build the boundary and a mock first.

## Payments

Provider-independent. Lifecycle: `CREATED, PENDING, AUTHORIZED, CAPTURED, FAILED, UNKNOWN`. UNKNOWN requires reconciliation.

Webhook flow:

```text
Provider -> HTTPS webhook
  -> Signature verification
  -> Timestamp validation
  -> Replay protection
  -> Idempotency
  -> Persist event
  -> Background processing
  -> Update internal state
```

Never store PAN/CVV/PIN. Store only safe external references and status information.

## Hardware

USB HID barcode scanners initially; architecture allows Serial, Bluetooth, Network, vendor-specific implementations. Receipt printers and cash drawers sit behind their interfaces; a printer failure must not delete or rollback a completed sale. Weighing scales support fractional quantities where the product allows it.
