// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Identity;

/// <summary>
/// Configuration of brute-force protection. Bound from configuration.
/// </summary>
public sealed class AuthLockoutOptions
{
    public const string SectionName = "Auth:Lockout";

    /// <summary>Number of consecutive failed logins that triggers a lockout.</summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>How long the account is locked.</summary>
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Configuration of token lifetimes. Bound from configuration.
/// </summary>
public sealed class TokenLifetimeOptions
{
    public const string SectionName = "Auth:Tokens";

    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(30);

    public TimeSpan EmailVerificationTokenLifetime { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan PasswordResetTokenLifetime { get; set; } = TimeSpan.FromMinutes(30);
}

/// <summary>
/// Issuer/audience for the JWT. Bound from configuration.
/// </summary>
public sealed class JwtIssuerOptions
{
    public const string SectionName = "Auth:Jwt";

    public string Issuer { get; set; } = "salekhpos.api";

    public string[] Audiences { get; set; } = new[]
    {
        "salekhpos.web",
        "salekhpos.desktop",
        "salekhpos.mobile",
    };
}
