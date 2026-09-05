// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Tenants;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain;

public sealed class StoreTests
{
    private static readonly DateTime Now = new(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Store NewStore(string code = "MAIN")
    {
        return Store.Create(
            TenantId,
            "Main Store",
            StoreCode.Create(code),
            TimeZoneInfo.Utc,
            CurrencyCode.Create("GEL"),
            address: null,
            Now);
    }

    [Fact]
    public void CreateInitialisesDefaults()
    {
        var store = NewStore();
        store.Id.Should().NotBe(Guid.Empty);
        store.TenantId.Should().Be(TenantId);
        store.Name.Should().Be("Main Store");
        store.Code.Value.Should().Be("MAIN");
        store.TimeZone.Should().Be(TimeZoneInfo.Utc);
        store.Currency.Value.Should().Be("GEL");
        store.Address.Should().BeNull();
        store.Status.Should().Be(TenantStatus.Active);
        store.CreatedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void EmptyTenantIdIsRejected()
    {
        Action act = () => Store.Create(
            Guid.Empty, "Main", StoreCode.Create("MAIN"),
            TimeZoneInfo.Utc, CurrencyCode.Create("GEL"), null, Now);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyNameIsRejected(string candidate)
    {
        Action act = () => Store.Create(
            TenantId, candidate, StoreCode.Create("MAIN"),
            TimeZoneInfo.Utc, CurrencyCode.Create("GEL"), null, Now);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OverLengthNameIsRejected()
    {
        var tooLong = new string('a', 81);
        Action act = () => Store.Create(
            TenantId, tooLong, StoreCode.Create("MAIN"),
            TimeZoneInfo.Utc, CurrencyCode.Create("GEL"), null, Now);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RenameUpdatesNameAndStampsUpdatedAt()
    {
        var store = NewStore();
        var later = Now.AddMinutes(5);
        store.Rename("Flagship Store", later);
        store.Name.Should().Be("Flagship Store");
        store.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void UpdateAddressAcceptsAndClears()
    {
        var store = NewStore();
        var address = PostalAddress.Create("1 Rustaveli Ave", null, "Tbilisi", null, null, CountryCode.Create("GE"));
        store.UpdateAddress(address, Now.AddMinutes(1));
        store.Address.Should().Be(address);
        store.UpdateAddress(null, Now.AddMinutes(2));
        store.Address.Should().BeNull();
    }

    [Fact]
    public void SuspendAndReactivateToggleStatus()
    {
        var store = NewStore();
        store.Suspend(Now.AddMinutes(1));
        store.Status.Should().Be(TenantStatus.Suspended);
        store.Reactivate(Now.AddMinutes(2));
        store.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void ChangeCurrencyAndTimeZone()
    {
        var store = NewStore();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tbilisi");
        store.ChangeTimeZone(tz, Now.AddMinutes(1));
        store.TimeZone.Should().Be(tz);
        store.ChangeCurrency(CurrencyCode.Create("USD"), Now.AddMinutes(2));
        store.Currency.Value.Should().Be("USD");
    }

    [Fact]
    public void CreateRejectsNullTimeZone()
    {
        Action act = () => Store.Create(
            TenantId, "Main Store", StoreCode.Create("MAIN"),
            null!, CurrencyCode.Create("GEL"), null, Now);
        act.Should().Throw<ArgumentNullException>();
    }
}
