// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Tenants;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken);

    Task AddAsync(Tenant tenant, CancellationToken cancellationToken);
}
