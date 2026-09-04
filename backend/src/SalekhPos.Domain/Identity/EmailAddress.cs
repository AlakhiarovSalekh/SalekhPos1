// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Text.RegularExpressions;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// Email address value object. Always stored in canonical lowercase form
/// so case-sensitive lookups can't fail on "Alice@Example.com" vs
/// "alice@example.com". Validated with a conservative pattern; the
/// authoritative check is always "send a verification email".
/// </summary>
public readonly record struct EmailAddress
{
    private const int MaxLength = 254; // RFC 5321
    private static readonly Regex Pattern = new(
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Email is required.", nameof(raw));
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Email must be at most {MaxLength} characters.", nameof(raw));
        }

        if (!Pattern.IsMatch(trimmed))
        {
            throw new ArgumentException("Email format is invalid.", nameof(raw));
        }

        return new EmailAddress(trimmed.ToLowerInvariant());
    }

    public static bool TryCreate(string raw, out EmailAddress result)
    {
        try
        {
            result = Create(raw);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    public override string ToString() => Value;
}
