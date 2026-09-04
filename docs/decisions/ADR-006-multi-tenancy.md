# ADR-0006: Multi-tenancy

**Date:** 2026-09-04
**Status:** Accepted
**Phase:** 0

## Context

SalekhPos serves many independent businesses (tenants), each with multiple stores, registers, employees, and devices. Tenant isolation is mandatory. A user in one tenant must never reach another tenant's data, and a stolen or guessed UUID must never cross tenant boundaries.

## Decision

Implement a **shared-database, tenant-scoped** multi-tenant model:

- Every tenant-owned table carries a `tenant_id` column.
- The backend derives the tenant context from the authenticated identity and verified membership, never from a client-supplied `tenantId`.
- Every query path is scoped to the user's verified tenant (and, where relevant, verified store) via a centralised filter applied at the infrastructure/repository layer.
- The authorization pipeline is: `Authenticated? -> Tenant valid? -> Store access valid? -> Permission valid? -> Resource ownership valid? -> Business rules valid? -> Execute`.

A separate Platform Super Admin scope (not a tenant role with every permission) governs platform-level administration through a dedicated PlatformShell.

## Consequences

- Tenant isolation is enforced at the data layer and at the API layer; both are tested by the required tenant-isolation and BOLA/IDOR test suites.
- Super Admin actions require stronger protection (reauthentication, MFA, full audit, scoped and time-limited access).
- Migrations must not introduce a path that forgets to scope by tenant.

## Alternatives considered

- **Database-per-tenant.** Rejected at this stage: higher operational cost and cross-tenant reporting complexity; the shared-database model with strong scoping meets the requirement and can be revisited if a tenant requires physical isolation.
- **Client-supplied tenantId.** Rejected explicitly by the constitution (Section 11).

## References

- Constitution Section 11, 12, 13, 50, 51, 57, 58, 74, 102, 142
