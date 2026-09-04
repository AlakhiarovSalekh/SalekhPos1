# ADR-005: SignalR for realtime

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

Owners, managers, and devices benefit from realtime updates (new sale, inventory change, low stock, refund, register status, notifications, device status). Realtime must be scoped to authorized tenants and stores.

## Decision

Use **SignalR** with groups scoped to `tenant:{tenantId}` and `store:{storeId}`. The server authorizes group membership based on the authenticated identity's verified memberships. SignalR is **not the source of truth**: any missed message is recovered through API reconciliation.

## Consequences

- Clients subscribe to their tenant/store groups; the server validates membership before adding a connection to a group.
- Optimistic UI updates are permitted only when failure behaviour is safe and reconciliation is available.
- A missed realtime event never silently corrupts state.
- Redis backplane is used when scaling out to multiple backend instances (where justified).

## Alternatives considered

- **WebSockets with a custom protocol.** Rejected: SignalR provides the same capability with auth, reconnection, and group semantics for less risk.
- **Polling only.** Rejected: poor UX for owners monitoring live sales; however, reconciliation via API is always available as the fallback.

## References

- Constitution Section 40, 64
