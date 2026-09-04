// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly SalekhPosDbContext _db;

    public PasswordResetTokenRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken)
    {
        await _db.PasswordResetTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
    }

    public Task<PasswordResetToken?> FindActiveByHashAsync(string hashedToken, CancellationToken cancellationToken) =>
        _db.PasswordResetTokens.FirstOrDefaultAsync(
            t => t.HashedToken.Hex == hashedToken && t.ConsumedAtUtc == null,
            cancellationToken);
}
