// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// A tenant (a single SalekhPos customer / business). In Phase 2 this was
/// a stub with only the fields the auth flow needed. Phase 3 adds the
/// operational fields every later phase will rely on: default currency,
/// default time-zone, and lifecycle status. All Phase 2 callers
/// continue to work because the new properties default to sensible
/// values and the existing constructor signature is preserved.
/// </summary>
public sealed class Tenant : AuditableEntity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>URL-safe identifier (lowercase, dash-separated). Unique.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>
    /// The default currency for this tenant. Per-store overrides live on
    /// <c>Store.Currency</c>. Defaults to GEL on creation.
    /// </summary>
    public CurrencyCode Currency { get; private set; }

    /// <summary>
    /// The default IANA time-zone for this tenant. Per-store overrides
    /// live on <c>Store.TimeZone</c>. Defaults to UTC on creation.
    /// </summary>
    public TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Utc;

    public TenantStatus Status { get; private set; }

    private Tenant()
    {
    }

    private Tenant(string name, string slug, CurrencyCode currency, TimeZoneInfo timeZone, DateTime nowUtc)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Currency = currency;
        TimeZone = timeZone;
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

        // Phase 3 defaults: GEL, UTC. A future slice will let the
        // registering Owner pick a currency/time-zone; for now the auth
        // flow creates the tenant with these defaults.
        return new Tenant(name, slug, CurrencyCode.Create("GEL"), TimeZoneInfo.Utc, nowUtc);
    }

    public void ChangeCurrency(CurrencyCode newCurrency, DateTime nowUtc)
    {
        if (Status == TenantStatus.Suspended)
        {
            throw new InvalidOperationException("Cannot change currency of a suspended tenant.");
        }
        Currency = newCurrency;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangeTimeZone(TimeZoneInfo newTimeZone, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(newTimeZone);
        if (Status == TenantStatus.Suspended)
        {
            throw new InvalidOperationException("Cannot change time-zone of a suspended tenant.");
        }
        TimeZone = newTimeZone;
        UpdatedAtUtc = nowUtc;
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
