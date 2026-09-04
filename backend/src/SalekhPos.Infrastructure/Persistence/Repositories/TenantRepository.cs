// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly SalekhPosDbContext _db;

    public TenantRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public Task<Tenant?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken) =>
        _db.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        await _db.Tenants.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
    }
}
