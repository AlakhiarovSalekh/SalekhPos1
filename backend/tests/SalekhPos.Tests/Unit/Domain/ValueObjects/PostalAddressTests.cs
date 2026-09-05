// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Common;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain.ValueObjects;

public sealed class PostalAddressTests
{
    [Fact]
    public void MinimalValidAddressIsAccepted()
    {
        var country = CountryCode.Create("GE");
        var address = PostalAddress.Create("1 Rustaveli Ave", null, "Tbilisi", null, null, country);
        address.Line1.Should().Be("1 Rustaveli Ave");
        address.City.Should().Be("Tbilisi");
        address.Country.Should().Be(country);
        address.Line2.Should().BeNull();
        address.Region.Should().BeNull();
        address.PostalCode.Should().BeNull();
    }

    [Fact]
    public void FullAddressIsAccepted()
    {
        var country = CountryCode.Create("US");
        var address = PostalAddress.Create("350 5th Ave", "Floor 21", "New York", "NY", "10118", country);
        address.Line2.Should().Be("Floor 21");
        address.Region.Should().Be("NY");
        address.PostalCode.Should().Be("10118");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Line1IsRequired(string candidate)
    {
        Action act = () => PostalAddress.Create(candidate, null, "Tbilisi", null, null, CountryCode.Create("GE"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CityIsRequired()
    {
        Action act = () => PostalAddress.Create("1 Rustaveli Ave", null, "", null, null, CountryCode.Create("GE"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OverLengthLine1IsRejected()
    {
        var tooLong = new string('a', 121);
        Action act = () => PostalAddress.Create(tooLong, null, "Tbilisi", null, null, CountryCode.Create("GE"));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToStringConcatenatesAllParts()
    {
        var address = PostalAddress.Create("1 Rustaveli Ave", "Apt 5", "Tbilisi", null, "0108", CountryCode.Create("GE"));
        address.ToString().Should().Contain("1 Rustaveli Ave");
        address.ToString().Should().Contain("Apt 5");
        address.ToString().Should().Contain("Tbilisi");
        address.ToString().Should().Contain("0108");
        address.ToString().Should().Contain("GE");
    }
}
