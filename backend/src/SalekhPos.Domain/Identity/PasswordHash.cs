// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Identity;

/// <summary>
/// Wraps an Argon2id encoded hash string ("$argon2id$v=19$m=...,t=...,p=...$salt$hash").
/// The raw password is never stored. Verification is done by
/// <c>IPasswordHasher</c> in the Application layer.
/// </summary>
public readonly record struct PasswordHash
{
    public string Encoded { get; }

    private PasswordHash(string encoded)
    {
        Encoded = encoded;
    }

    public static PasswordHash FromEncoded(string encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded))
        {
            throw new ArgumentException("Encoded password hash is required.", nameof(encoded));
        }

        // Argon2id encoded strings start with $argon2id$ (libsodium-style) or $argon2id$v=19$... (Isopoh style).
        if (!encoded.StartsWith("$argon2id$", StringComparison.Ordinal))
        {
            throw new ArgumentException("Only Argon2id hashes are supported.", nameof(encoded));
        }

        return new PasswordHash(encoded);
    }

    public override string ToString() => "***REDACTED***";
}
