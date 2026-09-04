// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when a user attempts to authenticate before verifying the
/// email address. Surfaces as ProblemDetails 403.
/// </summary>
public sealed class EmailNotVerifiedException : DomainException
{
    public EmailNotVerifiedException()
        : base("Email address has not been verified.")
    {
    }
}
