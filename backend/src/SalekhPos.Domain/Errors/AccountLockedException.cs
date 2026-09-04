// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when a user attempts to authenticate while their account is
/// temporarily locked. Surfaces as ProblemDetails 423 (Locked).
/// </summary>
public sealed class AccountLockedException : DomainException
{
    public DateTime LockedUntilUtc { get; }

    public AccountLockedException(DateTime lockedUntilUtc)
        : base("Account is temporarily locked.")
    {
        LockedUntilUtc = lockedUntilUtc;
    }
}
