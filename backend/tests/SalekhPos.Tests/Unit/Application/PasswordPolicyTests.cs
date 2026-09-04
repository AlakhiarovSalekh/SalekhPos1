// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

// CA1861 prefers static readonly arrays for repeated literal allocations;
// the personal-field arrays here are test data, not perf-critical, so we
// suppress the rule for the file.
#pragma warning disable CA1861

using FluentAssertions;
using SalekhPos.Application.Common;
using Xunit;

namespace SalekhPos.Tests.Unit.Application;

public sealed class PasswordPolicyTests
{
    [Fact]
    public void ValidPasswordIsAccepted()
    {
        var result = PasswordPolicy.Validate("CorrectHorseBatteryStaple!9");
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Short1!")] // 7 chars
    public void TooShortOrEmptyIsRejected(string candidate)
    {
        var result = PasswordPolicy.Validate(candidate);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("password.policy");
    }

    [Fact]
    public void PasswordWithoutUppercaseIsRejected()
    {
        var result = PasswordPolicy.Validate("correcthorsebatterystaple!9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordWithoutLowercaseIsRejected()
    {
        var result = PasswordPolicy.Validate("CORRECTHORSEBATTERYSTAPLE!9");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordWithoutDigitIsRejected()
    {
        var result = PasswordPolicy.Validate("CorrectHorseBatteryStaple!!!");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordWithoutSymbolIsRejected()
    {
        var result = PasswordPolicy.Validate("CorrectHorseBatteryStaple99");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordWithWhitespaceIsRejected()
    {
        var result = PasswordPolicy.Validate("Correct Horse Battery!9");
        result.IsSuccess.Should().BeFalse();
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password1")]
    [InlineData("SalekhPos1")]
    [InlineData("welcome123")]
    public void CommonPasswordIsRejected(string candidate)
    {
        var result = PasswordPolicy.Validate(candidate);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordContainingEmailIsRejected()
    {
        var result = PasswordPolicy.Validate("alice@example.com!9", personalFields: new[] { "alice@example.com" });
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PasswordContainingFullNameIsRejected()
    {
        var result = PasswordPolicy.Validate("Alice Wonderland!9", personalFields: new[] { "Alice Wonderland" });
        result.IsSuccess.Should().BeFalse();
    }
}
