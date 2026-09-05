// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when a country code is syntactically well-formed (2 uppercase
/// letters) but is not in the official ISO 3166-1 alpha-2 list. The
/// application layer (Phase 3 Slice 2) catches this when validating
/// user-supplied values and surfaces it to the API as 400 with a typed
/// code.
/// </summary>
public sealed class UnknownCountryException : DomainException
{
    public string Code { get; }

    public UnknownCountryException(string code)
        : base($"Country code '{code}' is not a known ISO 3166-1 alpha-2 code.")
    {
        Code = code;
    }
}
