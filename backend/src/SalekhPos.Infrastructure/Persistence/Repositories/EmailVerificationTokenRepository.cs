// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly SalekhPosDbContext _db;

    public EmailVerificationTokenRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken)
    {
        await _db.EmailVerificationTokens.AddAsync(token, cancellationToken).ConfigureAwait(false);
    }

    public Task<EmailVerificationToken?> FindActiveByHashAsync(string hashedToken, CancellationToken cancellationToken) =>
        _db.EmailVerificationTokens.FirstOrDefaultAsync(
            t => t.HashedToken.Hex == hashedToken && t.ConsumedAtUtc == null,
            cancellationToken);
}
