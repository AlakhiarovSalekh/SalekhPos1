// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography;
using Isopoh.Cryptography.Argon2;
using SalekhPos.Application.Abstractions;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// Argon2id password hasher. Parameters are conservative for a 2024-vintage
/// server: memory 19 MiB, iterations 2, parallelism 1, hash length 32
/// bytes, salt length 16 bytes. Verified against the OWASP Password
/// Storage Cheat Sheet (2024).
/// </summary>
public sealed class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string plaintext)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);

        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing,
            Version = Argon2Version.Nineteen,
            MemoryCost = 19_456,    // 19 MiB
            TimeCost = 2,
            Lanes = 1,              // parallelism
            Threads = 1,
            HashLength = 32,
            Password = Encoding.UTF8.GetBytes(plaintext),
            Salt = salt,
        };

        using var argon2 = new Argon2(config);
        using var hash = argon2.Hash();
        return config.EncodeString(hash.Buffer);
    }

    public bool Verify(string plaintext, string encoded)
    {
        if (string.IsNullOrEmpty(plaintext) || string.IsNullOrEmpty(encoded))
        {
            return false;
        }

        var passwordBytes = Encoding.UTF8.GetBytes(plaintext);

        // The simple static verifier is the canonical entry point and
        // handles decode + re-hash + constant-time compare internally.
        return Argon2.Verify(encoded, passwordBytes);
    }
}
