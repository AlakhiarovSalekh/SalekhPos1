// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Text.RegularExpressions;

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// Short, human-readable identifier for a <see cref="Store"/> within a tenant.
/// Cashiers type this at the register to select the active store. Rules:
/// 2–12 characters, uppercase letters (A–Z) and digits (0–9), must start
/// with a letter. Always stored in canonical uppercase form. Unique within
/// the tenant (enforced in the application / infrastructure layer).
/// </summary>
public readonly record struct StoreCode
{
    private const int MinLength = 2;
    private const int MaxLength = 12;
    private static readonly Regex Pattern = new(
        $"^[A-Z][A-Z0-9]{{{MinLength - 1},{MaxLength - 1}}}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private StoreCode(string value)
    {
        Value = value;
    }

    public static StoreCode Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Store code is required.", nameof(raw));
        }

        var trimmed = raw.Trim().ToUpperInvariant();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"Store code must be {MinLength}-{MaxLength} characters, " +
                "uppercase letters and digits, starting with a letter.",
                nameof(raw));
        }

        return new StoreCode(trimmed);
    }

    public static bool TryCreate(string raw, out StoreCode result)
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
