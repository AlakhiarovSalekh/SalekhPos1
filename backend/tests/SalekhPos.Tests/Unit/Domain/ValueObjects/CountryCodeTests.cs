// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Common;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain.ValueObjects;

public sealed class CountryCodeTests
{
    [Theory]
    [InlineData("GE")]
    [InlineData("US")]
    [InlineData("AZ")]
    public void ValidCodeIsAccepted(string candidate)
    {
        var code = CountryCode.Create(candidate);
        code.Value.Should().Be(candidate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("G")]      // 1 char
    [InlineData("GEO")]    // 3 chars
    [InlineData("G1")]     // digit
    [InlineData("G-")]     // dash
    public void MalformedCodeIsRejected(string candidate)
    {
        Action act = () => CountryCode.Create(candidate);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateUppercasesInput()
    {
        var code = CountryCode.Create("ge");
        code.Value.Should().Be("GE");
    }

    [Fact]
    public void TryCreateReturnsTrueForValid()
    {
        var ok = CountryCode.TryCreate("US", out var code);
        ok.Should().BeTrue();
        code.Value.Should().Be("US");
    }

    [Fact]
    public void TryCreateReturnsFalseForInvalid()
    {
        var ok = CountryCode.TryCreate("USA", out var code);
        ok.Should().BeFalse();
        code.Should().Be(default(CountryCode));
    }
}
