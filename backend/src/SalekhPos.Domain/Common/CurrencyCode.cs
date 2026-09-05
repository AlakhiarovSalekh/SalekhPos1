// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Text.RegularExpressions;

namespace SalekhPos.Domain.Common;

/// <summary>
/// ISO 4217 three-letter currency code (for example "GEL", "USD", "AZN").
/// Always stored in canonical uppercase form. The domain does not own a
/// list of valid codes — the application layer (Phase 3 Slice 2) is
/// responsible for looking the code up against the official ISO 4217
/// list and rejecting unknown values via <c>UnknownCurrencyException</c>.
/// </summary>
public readonly record struct CurrencyCode
{
    private const int Length = 3;
    private static readonly Regex Pattern = new(
        "^[A-Z]{3}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private CurrencyCode(string value)
    {
        Value = value;
    }

    public static CurrencyCode Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Currency code is required.", nameof(raw));
        }

        var trimmed = raw.Trim().ToUpperInvariant();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"Currency code must be exactly {Length} uppercase letters (ISO 4217).",
                nameof(raw));
        }

        return new CurrencyCode(trimmed);
    }

    public static bool TryCreate(string raw, out CurrencyCode result)
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
