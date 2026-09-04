// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SalekhPosDbContext _db;

    public UserRepository(SalekhPosDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);

    public Task<bool> EmailExistsAsync(EmailAddress email, CancellationToken cancellationToken) =>
        _db.Users.AnyAsync(u => u.Email.Value == email.Value, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _db.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }

    public Task<User?> FindByIdWithMfaAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Users
            .Include(u => u.Mfa)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
}
