// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.HashedToken).HasColumnName("hashed_token").HasColumnType("char(64)").IsRequired();
        builder.Property(t => t.IssuedAtUtc).HasColumnName("issued_at_utc").IsRequired();
        builder.Property(t => t.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(t => t.ConsumedAtUtc).HasColumnName("consumed_at_utc");
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(t => t.HashedToken).IsUnique();
    }
}
