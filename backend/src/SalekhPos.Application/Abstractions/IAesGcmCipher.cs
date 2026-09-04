// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// AES-256-GCM authenticated encryption. Used to encrypt TOTP shared
/// secrets at rest. The default implementation uses .NET 8's
/// <c>System.Security.Cryptography.AesGcm</c>.
/// </summary>
public interface IAesGcmCipher
{
    /// <summary>Encrypts plaintext with the configured key; returns
    /// (ciphertext, nonce, tag).</summary>
    AesGcmResult Encrypt(byte[] plaintext, ReadOnlySpan<byte> associatedData = default);

    /// <summary>Decrypts the result of <see cref="Encrypt"/>.</summary>
    byte[] Decrypt(AesGcmResult cipher, ReadOnlySpan<byte> associatedData = default);
}

public sealed record AesGcmResult(byte[] Ciphertext, byte[] Nonce, byte[] Tag);
