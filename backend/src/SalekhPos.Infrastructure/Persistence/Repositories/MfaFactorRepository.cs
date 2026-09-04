// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class MfaFactorRepository : IMfaFactorRepository
{
    private readonly SalekhPosDbContext _db;

    public MfaFactorRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(MfaFactor factor, CancellationToken cancellationToken)
    {
        await _db.MfaFactors.AddAsync(factor, cancellationToken).ConfigureAwait(false);
    }

    public Task<MfaFactor?> FindActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        _db.MfaFactors.FirstOrDefaultAsync(f => f.UserId == userId, cancellationToken);
}
