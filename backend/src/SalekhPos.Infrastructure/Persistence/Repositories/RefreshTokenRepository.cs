// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SalekhPosDbContext _db;

    public RefreshTokenRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        await _db.RefreshTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
    }

    public Task<RefreshToken?> FindByHashAsync(string hashedToken, CancellationToken cancellationToken) =>
        _db.RefreshTokens.FirstOrDefaultAsync(t => t.HashedToken.Hex == hashedToken, cancellationToken);

    public async Task RevokeFamilyAsync(Guid familyId, string reason, DateTime nowUtc, CancellationToken cancellationToken)
    {
        // We accept Guid.Empty to mean "every family for this user is not
        // known to the caller; the global revoke is implemented by
        // bumping the user's token_version, which is the application's
        // job, not the repository's". This is a no-op for empty.
        if (familyId == Guid.Empty)
        {
            return;
        }

        var rows = await _db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevocationReason == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var row in rows)
        {
            row.Revoke(reason, nowUtc);
        }
    }
}
