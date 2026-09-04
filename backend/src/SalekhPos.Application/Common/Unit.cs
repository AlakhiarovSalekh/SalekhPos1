// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Common;

/// <summary>
/// Marker record used in lieu of a real return value from a use case
/// (mirrors the F# / MediatR convention). The Result&lt;Unit&gt; return
/// keeps the API uniform: every handler returns a Result.
/// </summary>
public sealed record Unit
{
    public static readonly Unit Value = new();
}
