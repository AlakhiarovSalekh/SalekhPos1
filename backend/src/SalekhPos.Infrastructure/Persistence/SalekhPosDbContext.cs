// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Infrastructure.Persistence;

/// <summary>
/// The SalekhPos EF Core DbContext. Owns the table sets for tenants,
/// users, refresh / verification / reset tokens, MFA factors, and
/// recovery codes. Schema is PostgreSQL 16 (snake_case columns, UUID
/// primary keys, UTC timestamps).
/// </summary>
public sealed class SalekhPosDbContext : DbContext
{
    public SalekhPosDbContext(DbContextOptions<SalekhPosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public DbSet<MfaFactor> MfaFactors => Set<MfaFactor>();

    public DbSet<MfaRecoveryCode> MfaRecoveryCodes => Set<MfaRecoveryCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalekhPosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
