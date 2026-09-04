// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Common;

/// <summary>
/// Base class for domain exceptions. Use these for unexpected invariants
/// (corrupted state, programming errors) — not for expected business
/// outcomes, which use <see cref="Result{T}"/>.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }

    protected DomainException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
