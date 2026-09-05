// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when a currency code is syntactically well-formed (3 uppercase
/// letters) but is not in the official ISO 4217 list. The application
/// layer (Phase 3 Slice 2) catches this when validating user-supplied
/// values and surfaces it to the API as 400 with a typed code.
/// </summary>
public sealed class UnknownCurrencyException : DomainException
{
    public string Code { get; }

    public UnknownCurrencyException(string code)
        : base($"Currency code '{code}' is not a known ISO 4217 code.")
    {
        Code = code;
    }
}
