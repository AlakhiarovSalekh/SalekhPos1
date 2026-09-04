// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when credentials (password) do not match. Surfaces as
/// ProblemDetails 401 with no enumeration (we never say "user not
/// found" vs "wrong password" to the client).
/// </summary>
public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid credentials.")
    {
    }
}
