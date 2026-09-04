// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// AES-256-GCM cipher. The key is read from configuration
/// (32 raw bytes, base64-encoded) and held in process memory. Rotation
/// is a one-time operation: the Infrastructure layer's data-protection
/// options tell the application which key version is current; rows
/// store a key-version byte alongside the encrypted blob so a future
/// rotation can re-encrypt in a single migration.
/// </summary>
public sealed class AesGcmCipher : IAesGcmCipher
{
    public const int KeySizeBytes = 32;
    public const int NonceSizeBytes = 12;
    public const int TagSizeBytes = 16;

    private readonly byte[] _key;

    public AesGcmCipher(IOptions<DataProtectionOptions> options)
    {
        var raw = options.Value.CurrentKey;
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException("DataProtection:CurrentKey is not configured.");
        }

        var key = Convert.FromBase64String(raw);
        if (key.Length != KeySizeBytes)
        {
            throw new InvalidOperationException(
                $"DataProtection:CurrentKey must decode to {KeySizeBytes} bytes (got {key.Length}).");
        }

        _key = key;
    }

    public AesGcmResult Encrypt(byte[] plaintext, ReadOnlySpan<byte> associatedData = default)
    {
        ArgumentNullException.ThrowIfNull(plaintext);

        var ciphertext = new byte[plaintext.Length];
        var nonce = new byte[NonceSizeBytes];
        var tag = new byte[TagSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        using var cipher = new AesGcm(_key, TagSizeBytes);
        cipher.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

        return new AesGcmResult(ciphertext, nonce, tag);
    }

    public byte[] Decrypt(AesGcmResult cipher, ReadOnlySpan<byte> associatedData = default)
    {
        ArgumentNullException.ThrowIfNull(cipher.Ciphertext);
        ArgumentNullException.ThrowIfNull(cipher.Nonce);
        ArgumentNullException.ThrowIfNull(cipher.Tag);

        var plaintext = new byte[cipher.Ciphertext.Length];
        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Decrypt(cipher.Nonce, cipher.Ciphertext, cipher.Tag, plaintext, associatedData);
        return plaintext;
    }
}

/// <summary>Data-protection options bound from configuration.</summary>
public sealed class DataProtectionOptions
{
    public const string SectionName = "DataProtection";

    /// <summary>Base64-encoded 32-byte key used to encrypt sensitive blobs (TOTP secrets).</summary>
    public string CurrentKey { get; set; } = string.Empty;
}
