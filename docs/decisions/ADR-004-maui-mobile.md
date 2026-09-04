# ADR-004: .NET MAUI for mobile

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

SalekhPos needs Android and iOS applications for owners, managers, and administrators: monitoring, approvals, reports, inventory, products, employees, notifications. The mobile experience is not a clone of the full Desktop POS — cashiers use the Desktop POS.

## Decision

Use **.NET MAUI** for the mobile application, sharing the .NET ecosystem with the backend and desktop. The mobile app calls the same `/api/v1/` backend and SignalR hubs; it never performs authoritative calculations. The mobile app uses secure token storage, biometric unlock where appropriate, session timeout, secure logout, encrypted sensitive cache, and authorizes deep links (deep links must not bypass authorization).

## Consequences

- Code-sharing with backend/domain abstractions where useful, while keeping mobile UX distinct.
- Mobile is a thin, untrusted client. Authorization, prices, totals, and tenant access are enforced server-side.
- Device revocation is honoured: a stolen device can be revoked from authorized administration.

## Alternatives considered

- **Native Swift + Kotlin.** Rejected: doubles the implementation surface and diverges from the locked .NET stack.
- **React Native.** Rejected: the constitution locks the mobile stack to .NET MAUI.

## References

- Constitution Section 8, 14, 148
