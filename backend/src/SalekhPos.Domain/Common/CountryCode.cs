// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Text.RegularExpressions;

namespace SalekhPos.Domain.Common;

/// <summary>
/// ISO 3166-1 alpha-2 country code (for example "GE", "US", "AZ"). Always
/// stored in canonical uppercase form. The domain does not own a list of
/// valid codes — the application layer is responsible for looking the code
/// up against the official ISO list and rejecting unknown values via
/// <c>UnknownCountryException</c>.
/// </summary>
public readonly record struct CountryCode
{
    private const int Length = 2;
    private static readonly Regex Pattern = new(
        "^[A-Z]{2}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private CountryCode(string value)
    {
        Value = value;
    }

    public static CountryCode Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Country code is required.", nameof(raw));
        }

        var trimmed = raw.Trim().ToUpperInvariant();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"Country code must be exactly {Length} uppercase letters (ISO 3166-1 alpha-2).",
                nameof(raw));
        }

        return new CountryCode(trimmed);
    }

    public static bool TryCreate(string raw, out CountryCode result)
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
