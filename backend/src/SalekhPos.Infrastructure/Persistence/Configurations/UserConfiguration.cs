// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasColumnType("text").IsRequired();
        builder.Property(u => u.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(u => u.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(u => u.TokenVersion).HasColumnName("token_version").IsRequired();
        builder.Property(u => u.EmailVerifiedAtUtc).HasColumnName("email_verified_at_utc");
        builder.Property(u => u.LockedUntilUtc).HasColumnName("locked_until_utc");
        builder.Property(u => u.FailedLoginCount).HasColumnName("failed_login_count").IsRequired();
        builder.Property(u => u.LastLoginAtUtc).HasColumnName("last_login_at_utc");
        builder.Property(u => u.LastFailedLoginAtUtc).HasColumnName("last_failed_login_at_utc");
        builder.Property(u => u.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(u => u.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(u => u.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(u => u.UpdatedByUserId).HasColumnName("updated_by_user_id");

        builder.HasOne<Tenant>().WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(u => u.Mfa).WithOne().HasForeignKey<MfaFactor>(m => m.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        builder.HasIndex(u => u.Email);
    }
}
