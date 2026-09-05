// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// Lifecycle status of a tenant-scoped entity (Tenant, Store, Membership).
/// Persisted as a string column in Postgres so the value can grow without
/// a migration. A tenant (or store, or membership) that is <see cref="Suspended"/>
/// cannot be used for new operations but its historical data is retained
/// for audit and reporting.
/// </summary>
public enum TenantStatus
{
    /// <summary>The entity is active and accepts new operations.</summary>
    Active = 0,

    /// <summary>
    /// The entity is suspended. Logins fail with 423, mutations fail with
    /// 423, reads still succeed so an owner can see why they were suspended.
    /// </summary>
    Suspended = 1,
}
