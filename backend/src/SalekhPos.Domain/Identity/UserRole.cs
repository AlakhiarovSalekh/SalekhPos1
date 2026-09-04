// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Identity;

/// <summary>
/// Coarse role of a user within a tenant (or platform-wide).
/// Phase 3 expands this into per-store permissions; for now the role is
/// embedded in the JWT and used to drive coarse-grained UI affordances
/// and the Super Admin MFA requirement.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Platform-level administrator. NOT a tenant user. Requires MFA on
    /// first login (enforced by Application layer). The "tenantId" claim
    /// for this role is a reserved platform tenant id.
    /// </summary>
    PlatformSuperAdmin = 0,

    /// <summary>Owner of a tenant. Full access within the tenant.</summary>
    TenantOwner = 1,

    /// <summary>Manager within a tenant. Most operations, not billing.</summary>
    TenantManager = 2,

    /// <summary>Cashier. Constrained to POS operations.</summary>
    TenantCashier = 3,

    /// <summary>Inventory clerk. Products, stock, no pricing authority.</summary>
    TenantInventory = 4,

    /// <summary>Accountant. Reports, no operational changes.</summary>
    TenantAccountant = 5,
}
