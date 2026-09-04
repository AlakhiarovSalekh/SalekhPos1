// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;

namespace SalekhPos.Application;

/// <summary>
/// Marker for the SalekhPos Application assembly. The Application layer
/// coordinates use cases (commands, queries) and depends on Domain and on
/// abstractions only. It does not depend on Infrastructure.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>
    /// Gets the Assembly reference for the Application layer. Used by
    /// reflection-based registration (FluentValidation, Mapster).
    /// </summary>
    public static readonly Assembly Assembly = typeof(AssemblyMarker).Assembly;
}
