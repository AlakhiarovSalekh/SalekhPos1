// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SalekhPos.Application;

/// <summary>
/// Composition-root extensions for the Application layer. Infrastructure
/// (DbContext, external services) is registered by the Infrastructure layer;
/// Api calls <see cref="AddApplication"/> together with
/// <c>AddInfrastructure</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Application layer services. Concrete registrations
    /// (FluentValidation validators, Mapster mappings, MediatR-style
    /// dispatcher) are added in the phase that introduces the first
    /// command / query / DTO.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Foundation only: no application services yet. The first
        // application services land in Phase 2 (Identity / Auth) where
        // the password hasher, token service, and user-related use cases
        // are introduced.
        _ = AssemblyMarker.Assembly;
        return services;
    }
}
