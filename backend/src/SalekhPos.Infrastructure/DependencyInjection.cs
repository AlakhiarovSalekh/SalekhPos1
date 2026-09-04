// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Identity;
using SalekhPos.Infrastructure.Persistence;
using SalekhPos.Infrastructure.Persistence.Repositories;
using SalekhPos.Infrastructure.Security;

namespace SalekhPos.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services: the EF Core DbContext, all
    /// repositories, the unit of work, the security primitives
    /// (Argon2id password hasher, EdDSA JWT signer, TOTP service,
    /// AES-GCM cipher, opaque token generator, system clock, logging
    /// email sender), and the configuration option classes used by the
    /// Application layer's use cases.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Configuration options.
        services.Configure<AuthLockoutOptions>(configuration.GetSection(AuthLockoutOptions.SectionName));
        services.Configure<TokenLifetimeOptions>(configuration.GetSection(TokenLifetimeOptions.SectionName));
        services.Configure<JwtIssuerOptions>(configuration.GetSection(JwtIssuerOptions.SectionName));
        services.Configure<JwtSigningKeyOptions>(configuration.GetSection(JwtSigningKeyOptions.SectionName));
        services.Configure<DataProtectionOptions>(configuration.GetSection(DataProtectionOptions.SectionName));

        // EF Core (PostgreSQL).
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");
        services.AddDbContext<SalekhPosDbContext>(opts => opts.UseNpgsql(
            connectionString,
            npg => npg.MigrationsAssembly(typeof(SalekhPosDbContext).Assembly.GetName().Name)));

        // Unit of work + repositories.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IMfaFactorRepository, MfaFactorRepository>();
        services.AddScoped<IMfaRecoveryCodeRepository, MfaRecoveryCodeRepository>();

        // Security services.
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITokenGenerator, OpaqueTokenGenerator>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ITotpService, OtpNetTotpService>();
        services.AddSingleton<IAesGcmCipher, AesGcmCipher>();
        services.AddSingleton<IJwtSigner, Ed25519JwtSigner>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        return services;
    }
}
