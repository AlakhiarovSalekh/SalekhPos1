// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Security.Cryptography;
using System.Text;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// SHA-256 hex of an opaque token string. The raw token is never persisted;
/// only its hash is stored. Lookups go through this value object.
/// </summary>
public readonly record struct HashedToken
{
    public string Hex { get; }

    private HashedToken(string hex)
    {
        Hex = hex;
    }

    public static HashedToken From(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken))
        {
            throw new ArgumentException("Token is required.", nameof(rawToken));
        }

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(rawToken), hash);
        return new HashedToken(Convert.ToHexString(hash).ToLowerInvariant());
    }

    public override string ToString() => "***REDACTED***";
}
