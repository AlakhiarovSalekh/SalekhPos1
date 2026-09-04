// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Configurations;

public sealed class MfaRecoveryCodeConfiguration : IEntityTypeConfiguration<MfaRecoveryCode>
{
    public void Configure(EntityTypeBuilder<MfaRecoveryCode> builder)
    {
        builder.ToTable("mfa_recovery_codes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.HashedCode).HasColumnName("hashed_code").HasColumnType("char(64)").IsRequired();
        builder.Property(t => t.IssuedAtUtc).HasColumnName("issued_at_utc").IsRequired();
        builder.Property(t => t.ConsumedAtUtc).HasColumnName("consumed_at_utc");
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(t => new { t.UserId, t.HashedCode }).IsUnique();
    }
}
