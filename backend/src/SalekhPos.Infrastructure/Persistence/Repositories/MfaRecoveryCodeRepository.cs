// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class MfaRecoveryCodeRepository : IMfaRecoveryCodeRepository
{
    private readonly SalekhPosDbContext _db;

    public MfaRecoveryCodeRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public async Task AddManyAsync(IEnumerable<MfaRecoveryCode> codes, CancellationToken cancellationToken)
    {
        await _db.MfaRecoveryCodes.AddRangeAsync(codes, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MfaRecoveryCode>> ListActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _db.MfaRecoveryCodes
            .Where(c => c.UserId == userId && c.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
