// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Common;

/// <summary>
/// A typed, structured domain error. Use cases return <see cref="Result{T}"/>
/// with a non-null <see cref="DomainError"/> instead of throwing for expected
/// business-rule failures (wrong password, locked account, expired token, ...).
/// Unexpected failures still throw exceptions.
/// </summary>
public sealed record DomainError(string Code, string Message, string? Field = null)
{
    public static DomainError Of(string code, string message, string? field = null) =>
        new(code, message, field);
}
