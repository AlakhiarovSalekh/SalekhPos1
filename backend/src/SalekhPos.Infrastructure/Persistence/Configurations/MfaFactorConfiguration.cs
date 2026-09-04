// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Persistence.Configurations;

public sealed class MfaFactorConfiguration : IEntityTypeConfiguration<MfaFactor>
{
    public void Configure(EntityTypeBuilder<MfaFactor> builder)
    {
        builder.ToTable("mfa_factors");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(t => t.EncryptedSecret).HasColumnName("encrypted_secret").HasColumnType("bytea").IsRequired();
        builder.Property(t => t.EnabledAtUtc).HasColumnName("enabled_at_utc").IsRequired();
        builder.Property(t => t.LastUsedAtUtc).HasColumnName("last_used_at_utc");
        builder.Property(t => t.LastCounter).HasColumnName("last_counter").IsRequired();
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(t => t.UserId).IsUnique();
    }
}
