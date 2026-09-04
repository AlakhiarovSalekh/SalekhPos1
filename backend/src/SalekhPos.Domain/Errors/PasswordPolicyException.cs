// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when a password does not satisfy the policy. Surfaces as
/// ProblemDetails 400 with field-level details.
/// </summary>
public sealed class PasswordPolicyException : DomainException
{
    public IReadOnlyList<string> Reasons { get; }

    public PasswordPolicyException(IReadOnlyList<string> reasons)
        : base("Password does not satisfy the security policy.")
    {
        Reasons = reasons;
    }
}
