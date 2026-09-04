// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Errors;

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// Issues and validates short-lived access tokens (JWT). The default
/// implementation signs with EdDSA (Ed25519) using a key file referenced
/// from configuration.
/// </summary>
public interface IJwtSigner
{
    /// <summary>Sign a compact JWT for the given claims.</summary>
    string Sign(JwtAccessTokenClaims claims);

    /// <summary>
    /// Validates a token's signature, lifetime, audience, and issuer.
    /// Throws <see cref="InvalidTokenException"/> on any failure.
    /// Returns the claims on success.
    /// </summary>
    JwtAccessTokenClaims Validate(string token);
}

/// <summary>
/// Access-token claim set. Serialised to JWT claims by the signer.
/// </summary>
public sealed record JwtAccessTokenClaims(
    Guid Subject,
    Guid TenantId,
    string Role,
    int TokenVersion,
    IEnumerable<string> Audiences,
    Guid Jti,
    DateTime IssuedAtUtc,
    DateTime ExpiresAtUtc);
