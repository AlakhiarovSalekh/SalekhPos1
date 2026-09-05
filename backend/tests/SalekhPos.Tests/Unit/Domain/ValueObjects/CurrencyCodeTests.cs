// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Common;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain.ValueObjects;

public sealed class CurrencyCodeTests
{
    [Theory]
    [InlineData("GEL")]
    [InlineData("USD")]
    [InlineData("AZN")]
    [InlineData("EUR")]
    public void ValidCodeIsAccepted(string candidate)
    {
        var code = CurrencyCode.Create(candidate);
        code.Value.Should().Be(candidate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]         // 2 chars
    [InlineData("USDD")]       // 4 chars
    [InlineData("us1")]        // digit
    [InlineData("US$")]        // symbol
    [InlineData("US-")]        // dash
    public void MalformedCodeIsRejected(string candidate)
    {
        Action act = () => CurrencyCode.Create(candidate);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateUppercasesInput()
    {
        var code = CurrencyCode.Create("usd");
        code.Value.Should().Be("USD");
    }

    [Fact]
    public void TryCreateReturnsTrueForValid()
    {
        var ok = CurrencyCode.TryCreate("EUR", out var code);
        ok.Should().BeTrue();
        code.Value.Should().Be("EUR");
    }

    [Fact]
    public void TryCreateReturnsFalseForInvalid()
    {
        var ok = CurrencyCode.TryCreate("XX", out var code);
        ok.Should().BeFalse();
        code.Should().Be(default(CurrencyCode));
    }
}
