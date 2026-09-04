// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// A tenant (a single SalekhPos customer / business). This is a Phase 2
/// stub: it carries only the fields the auth flow needs. The full
/// tenant model (settings, billing, plan, stores, memberships, ... lands
/// in Phase 3 with a backward-compatible migration.
/// </summary>
public sealed class Tenant : AuditableEntity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>URL-safe identifier (lowercase, dash-separated). Unique.</summary>
    public string Slug { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    private Tenant()
    {
    }

    private Tenant(string name, string slug, DateTime nowUtc)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Status = TenantStatus.Active;
        CreatedAtUtc = nowUtc;
    }

    public static Tenant Create(string name, string slug, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Tenant slug is required.", nameof(slug));
        }

        if (slug.Length > 64 || !System.Text.RegularExpressions.Regex.IsMatch(slug, "^[a-z0-9][a-z0-9-]{1,62}[a-z0-9]$"))
        {
            throw new ArgumentException("Tenant slug must be 2-64 chars, lowercase letters/digits/dashes.", nameof(slug));
        }

        return new Tenant(name, slug, nowUtc);
    }

    public void Suspend(DateTime nowUtc)
    {
        Status = TenantStatus.Suspended;
        UpdatedAtUtc = nowUtc;
    }

    public void Reactivate(DateTime nowUtc)
    {
        Status = TenantStatus.Active;
        UpdatedAtUtc = nowUtc;
    }
}
