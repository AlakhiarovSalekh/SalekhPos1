// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

#pragma warning disable CA1848

using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Domain.Errors;

namespace SalekhPos.Api.Authentication;

/// <summary>
/// ASP.NET Core authentication handler that validates EdDSA access
/// tokens with our own <see cref="IJwtSigner"/>. The token_version
/// claim is checked against the user's current token_version on
/// logout, password change, and role change. The JwtBearer middleware
/// does not support EdDSA natively, so we plug in a custom scheme.
/// </summary>
public sealed class EdDsaAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "EdDsa";

    private readonly IJwtSigner _signer;

    public EdDsaAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtSigner signer)
        : base(options, logger, encoder)
    {
        _signer = signer;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            authHeader.Count == 0 ||
            string.IsNullOrEmpty(authHeader[0]))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var header = authHeader[0]!.ToString();
        const string bearerPrefix = "Bearer ";
        if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization header is not a Bearer token."));
        }

        var token = header[bearerPrefix.Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Bearer token is empty."));
        }

        try
        {
            var claims = _signer.Validate(token);
            var identity = new ClaimsIdentity(SchemeName, nameType: "sub", roleType: "role");
            identity.AddClaim(new Claim("sub", claims.Subject.ToString()));
            identity.AddClaim(new Claim("tid", claims.TenantId.ToString()));
            identity.AddClaim(new Claim("role", claims.Role));
            identity.AddClaim(new Claim("ver", claims.TokenVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, claims.Subject.ToString()));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (InvalidTokenException ex)
        {
            Logger.LogInformation("Token validation failed: {Message}", ex.Message);
            return Task.FromResult(AuthenticateResult.Fail("Access token is invalid."));
        }
    }
}
