// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// Abstracts <see cref="DateTime.UtcNow"/> so use cases are testable.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
