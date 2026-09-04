// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SalekhPos.Infrastructure;

/// <summary>
/// Composition-root extensions for the Infrastructure layer. The exact
/// registrations (DbContext, Redis, payment adapters, fiscal adapters, etc.)
/// are added incrementally per phase; this file is the single entry point
/// Api and Worker use.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services. Currently a placeholder; concrete
    /// registrations (PostgreSQL, Redis, providers) land in their respective
    /// phases (Phase 1: PostgreSQL/Redis/health; Phase 16: integrations).
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Phase 1 deliberately does not bind DbContext / Redis here yet.
        // Those land when EF Core mappings and Redis usage are introduced.
        // Health checks for PostgreSQL/Redis are added in the same change set.

        return services;
    }
}
