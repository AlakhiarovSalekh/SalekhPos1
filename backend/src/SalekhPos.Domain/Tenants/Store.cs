// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// A point of sale inside a tenant. A tenant has one or more stores; a
/// <see cref="Membership"/> can be scoped to a subset of the tenant's
/// stores (or to all of them, when <c>StoreIds</c> is empty).
///
/// Cross-aggregate invariants (enforced in the application / infrastructure
/// layers, not in the domain):
/// <list type="bullet">
///   <item>StoreCode is unique within the tenant. (Enforced in the
///         application layer's <c>CreateStore</c> use case.)</item>
///   <item>Every store id referenced from a membership belongs to the
///         membership's tenant. (Enforced in <c>GrantStoreAccess</c>.)</item>
/// </list>
/// </summary>
public sealed class Store : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public StoreCode Code { get; private set; }

    public TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Utc;

    public CurrencyCode Currency { get; private set; }

    public PostalAddress? Address { get; private set; }

    public TenantStatus Status { get; private set; }

    private Store()
    {
    }

    private Store(
        Guid tenantId,
        string name,
        StoreCode code,
        TimeZoneInfo timeZone,
        CurrencyCode currency,
        PostalAddress? address,
        DateTime nowUtc)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Code = code;
        TimeZone = timeZone;
        Currency = currency;
        Address = address;
        Status = TenantStatus.Active;
        CreatedAtUtc = nowUtc;
    }

    public static Store Create(
        Guid tenantId,
        string name,
        StoreCode code,
        TimeZoneInfo timeZone,
        CurrencyCode currency,
        PostalAddress? address,
        DateTime nowUtc)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Store name is required.", nameof(name));
        }
        if (name.Length > 80)
        {
            throw new ArgumentException("Store name must be at most 80 characters.", nameof(name));
        }
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(currency);

        return new Store(tenantId, name, code, timeZone, currency, address, nowUtc);
    }

    public void Rename(string newName, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Store name is required.", nameof(newName));
        }
        if (newName.Length > 80)
        {
            throw new ArgumentException("Store name must be at most 80 characters.", nameof(newName));
        }
        Name = newName.Trim();
        UpdatedAtUtc = nowUtc;
    }

    public void UpdateAddress(PostalAddress? newAddress, DateTime nowUtc)
    {
        Address = newAddress;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangeTimeZone(TimeZoneInfo newTimeZone, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(newTimeZone);
        TimeZone = newTimeZone;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangeCurrency(CurrencyCode newCurrency, DateTime nowUtc)
    {
        Currency = newCurrency;
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
