// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// Lifecycle state of a tenant. The Phase 2 stub only honours
/// <see cref="Active"/>; full onboarding/suspension lives in Phase 16.
/// </summary>
public enum TenantStatus
{
    /// <summary>Tenant is fully active.</summary>
    Active = 0,

    /// <summary>Tenant is temporarily suspended (billing, abuse, ...).</summary>
    Suspended = 1,
}
