// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Common;

/// <summary>
/// A postal address value object. Used by stores (and, in later phases,
/// customers, suppliers, and the business profile). All components are
/// required except <see cref="Line2"/>, <see cref="Region"/>, and
/// <see cref="PostalCode"/>. The <see cref="Country"/> is a value object
/// itself, validated against the ISO 3166-1 alpha-2 format.
/// </summary>
public sealed record PostalAddress
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string? Region { get; }
    public string? PostalCode { get; }
    public CountryCode Country { get; }

    private PostalAddress(
        string line1,
        string? line2,
        string city,
        string? region,
        string? postalCode,
        CountryCode country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        Region = region;
        PostalCode = postalCode;
        Country = country;
    }

    public static PostalAddress Create(
        string line1,
        string? line2,
        string city,
        string? region,
        string? postalCode,
        CountryCode country)
    {
        if (string.IsNullOrWhiteSpace(line1))
        {
            throw new ArgumentException("Address line 1 is required.", nameof(line1));
        }
        if (line1.Length > 120)
        {
            throw new ArgumentException("Address line 1 must be at most 120 characters.", nameof(line1));
        }
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.", nameof(city));
        }
        if (city.Length > 80)
        {
            throw new ArgumentException("City must be at most 80 characters.", nameof(city));
        }
        if (region is not null && region.Length > 80)
        {
            throw new ArgumentException("Region must be at most 80 characters.", nameof(region));
        }
        if (postalCode is not null && postalCode.Length > 20)
        {
            throw new ArgumentException("Postal code must be at most 20 characters.", nameof(postalCode));
        }
        if (line2 is not null && line2.Length > 120)
        {
            throw new ArgumentException("Address line 2 must be at most 120 characters.", nameof(line2));
        }

        return new PostalAddress(
            line1.Trim(),
            line2?.Trim(),
            city.Trim(),
            region?.Trim(),
            postalCode?.Trim(),
            country);
    }

    public override string ToString()
    {
        var parts = new List<string> { Line1 };
        if (!string.IsNullOrEmpty(Line2))
        {
            parts.Add(Line2);
        }
        parts.Add(City);
        if (!string.IsNullOrEmpty(Region))
        {
            parts.Add(Region);
        }
        if (!string.IsNullOrEmpty(PostalCode))
        {
            parts.Add(PostalCode);
        }
        parts.Add(Country.Value);
        return string.Join(", ", parts);
    }
}
