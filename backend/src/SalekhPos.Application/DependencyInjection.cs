// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;
using FluentValidation;
using Mapster;
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
    /// Registers the Application layer services, FluentValidation validators,
    /// and Mapster mappings. The MediatR-style dispatcher is intentionally
    /// omitted in the foundation phase; it is added when the first commands
    /// and queries are introduced in Phase 2+.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssembly(AssemblyMarker.Assembly, includeInternalTypes: false);
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();

        return services;
    }
}
