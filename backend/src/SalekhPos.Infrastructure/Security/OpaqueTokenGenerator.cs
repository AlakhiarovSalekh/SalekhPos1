// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Security.Cryptography;
using SalekhPos.Application.Abstractions;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// Opaque token generator. Returns base64url-encoded random bytes
/// (default 32 bytes = 256 bits) and the SHA-256 hash for storage.
/// </summary>
public sealed class OpaqueTokenGenerator : ITokenGenerator
{
    public (string Raw, string Hash) NewOpaqueToken(int byteLength = 32)
    {
        if (byteLength < 16)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "Opaque tokens must be at least 16 bytes.");
        }

        var buffer = new byte[byteLength];
        RandomNumberGenerator.Fill(buffer);
        var raw = Base64UrlEncode(buffer);
        var hash = HashedToken.From(raw).Hex;
        return (raw, hash);
    }

    public Guid NewJti() => Guid.NewGuid();

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
