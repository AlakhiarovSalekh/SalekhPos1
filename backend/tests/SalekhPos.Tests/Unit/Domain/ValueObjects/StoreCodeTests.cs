// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Tenants;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain.ValueObjects;

public sealed class StoreCodeTests
{
    [Theory]
    [InlineData("AB")]
    [InlineData("MAIN")]
    [InlineData("STORE001")]
    [InlineData("KALATA1")]
    public void ValidCodeIsAccepted(string candidate)
    {
        var result = StoreCode.Create(candidate);
        result.Value.Should().Be(candidate.ToUpperInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]              // 1 char (min is 2)
    [InlineData("AB1CD3EF5GH7IJ")] // 14 chars (max is 12)
    public void TooShortOrTooLongIsRejected(string candidate)
    {
        Action act = () => StoreCode.Create(candidate);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("1ABC")]      // starts with a digit
    [InlineData("AB-CD")]     // dash not allowed
    [InlineData("AB CD")]     // space not allowed
    [InlineData("AB#CD")]     // symbol not allowed
    public void MalformedCodeIsRejected(string candidate)
    {
        Action act = () => StoreCode.Create(candidate);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateUppercasesInput()
    {
        var code = StoreCode.Create("main01");
        code.Value.Should().Be("MAIN01");
    }

    [Fact]
    public void TryCreateReturnsTrueForValid()
    {
        var ok = StoreCode.TryCreate("MAIN", out var code);
        ok.Should().BeTrue();
        code.Value.Should().Be("MAIN");
    }

    [Fact]
    public void TryCreateReturnsFalseForInvalid()
    {
        var ok = StoreCode.TryCreate("", out var code);
        ok.Should().BeFalse();
        code.Should().Be(default(StoreCode));
    }
}
