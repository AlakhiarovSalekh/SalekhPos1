// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions.Persistence;

namespace SalekhPos.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SalekhPosDbContext _db;

    public UnitOfWork(SalekhPosDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
