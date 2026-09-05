// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when a store id is referenced from a tenant context that does
/// not own the store. Used by the membership store-access grant/revoke
/// use cases (Phase 3 Slice 2) when a cross-tenant store id is passed
/// in error.
/// </summary>
public sealed class StoreNotInTenantException : DomainException
{
    public Guid StoreId { get; }
    public Guid TenantId { get; }

    public StoreNotInTenantException(Guid storeId, Guid tenantId)
        : base($"Store {storeId} does not belong to tenant {tenantId}.")
    {
        StoreId = storeId;
        TenantId = tenantId;
    }
}
