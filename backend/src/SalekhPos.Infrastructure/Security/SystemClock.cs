// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// System clock. In tests, replace with a fake via DI override.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
