// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Identity;
using SalekhPos.Application.Identity.ChangePassword;
using SalekhPos.Application.Identity.DisableMfa;
using SalekhPos.Application.Identity.EnableMfa;
using SalekhPos.Application.Identity.ForgotPassword;
using SalekhPos.Application.Identity.Login;
using SalekhPos.Application.Identity.Logout;
using SalekhPos.Application.Identity.Refresh;
using SalekhPos.Application.Identity.Register;
using SalekhPos.Application.Identity.RequestPasswordReset;
using SalekhPos.Application.Identity.ResetPassword;
using SalekhPos.Application.Identity.VerifyEmail;
using SalekhPos.Application.Identity.VerifyMfa;

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
    /// Registers the Application layer services. Phase 2 (Identity / Auth)
    /// wires up the use-case handlers and FluentValidation validators.
    /// Concrete implementations of the abstractions (IPasswordHasher,
    /// IJwtSigner, ITokenGenerator, ...) are registered by Infrastructure.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Scoped: one handler instance per request.
        services.AddScoped<RegisterHandler>();
        services.AddScoped<VerifyEmailHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<RequestPasswordResetHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<EnableMfaHandler>();
        services.AddScoped<VerifyMfaHandler>();
        services.AddScoped<DisableMfaHandler>();

        // FluentValidation: scan the Application assembly for validators.
        services.AddValidatorsFromAssembly(AssemblyMarker.Assembly, includeInternalTypes: false);

        return services;
    }
}
