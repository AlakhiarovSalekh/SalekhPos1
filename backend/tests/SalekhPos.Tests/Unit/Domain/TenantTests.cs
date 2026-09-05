// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Tenants;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain;

public sealed class TenantTests
{
    private static readonly DateTime Now = new(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateInitialisesDefaults()
    {
        var tenant = Tenant.Create("Acme Retail", "acme", Now);
        tenant.Id.Should().NotBe(Guid.Empty);
        tenant.Name.Should().Be("Acme Retail");
        tenant.Slug.Should().Be("acme");
        tenant.Currency.Value.Should().Be("GEL");
        tenant.TimeZone.Should().Be(TimeZoneInfo.Utc);
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.CreatedAtUtc.Should().Be(Now);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void EmptyNameIsRejected(string candidate)
    {
        Action act = () => Tenant.Create(candidate, "acme", Now);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad slug with spaces")]
    [InlineData("a")] // 1 char
    public void MalformedSlugIsRejected(string candidate)
    {
        Action act = () => Tenant.Create("Acme", candidate, Now);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SlugIsLowercased()
    {
        var tenant = Tenant.Create("Acme", "acme-retail", Now);
        tenant.Slug.Should().Be("acme-retail");
    }

    [Fact]
    public void ChangeCurrencyUpdatesAndStampsUpdatedAt()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        var later = Now.AddMinutes(5);
        tenant.ChangeCurrency(CurrencyCode.Create("USD"), later);
        tenant.Currency.Value.Should().Be("USD");
        tenant.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void ChangeTimeZoneUpdatesAndStampsUpdatedAt()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        var later = Now.AddMinutes(5);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tbilisi");
        tenant.ChangeTimeZone(tz, later);
        tenant.TimeZone.Should().Be(tz);
        tenant.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void SuspendSetsStatusToSuspended()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        var later = Now.AddMinutes(5);
        tenant.Suspend(later);
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void ReactivateSetsStatusToActive()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        tenant.Suspend(Now.AddMinutes(1));
        tenant.Reactivate(Now.AddMinutes(2));
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void CannotChangeCurrencyWhileSuspended()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        tenant.Suspend(Now.AddMinutes(1));
        Action act = () => tenant.ChangeCurrency(CurrencyCode.Create("USD"), Now.AddMinutes(2));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CannotChangeTimeZoneWhileSuspended()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        tenant.Suspend(Now.AddMinutes(1));
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tbilisi");
        Action act = () => tenant.ChangeTimeZone(tz, Now.AddMinutes(2));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeTimeZoneRejectsNull()
    {
        var tenant = Tenant.Create("Acme", "acme", Now);
        Action act = () => tenant.ChangeTimeZone(null!, Now);
        act.Should().Throw<ArgumentNullException>();
    }
}
