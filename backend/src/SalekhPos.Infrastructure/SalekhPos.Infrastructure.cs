// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;

namespace SalekhPos.Infrastructure;

/// <summary>
/// Marker for the SalekhPos Infrastructure assembly.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>
    /// Gets the Assembly reference for the Infrastructure layer.
    /// </summary>
    public static readonly Assembly Assembly = typeof(AssemblyMarker).Assembly;
}
