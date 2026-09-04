// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// A Time-based One-Time Password (RFC 6238) factor enrolled by a user.
/// The shared secret is stored as an encrypted blob
/// (ciphertext | nonce | tag) — never plaintext. The Infrastructure
/// layer's <c>IAesGcmCipher</c> is responsible for the encryption.
/// </summary>
public sealed class MfaFactor : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>
    /// The encrypted TOTP shared secret. Format is implementation
    /// specific (ciphertext, then nonce, then tag) — see
    /// <c>IAesGcmCipher</c>.
    /// </summary>
    public byte[] EncryptedSecret { get; private set; } = Array.Empty<byte>();

    public DateTime EnabledAtUtc { get; private set; }

    public DateTime? LastUsedAtUtc { get; private set; }

    /// <summary>
    /// Last successfully verified TOTP counter window. Defends against
    /// replay of a one-time code inside the same 30-second window.
    /// </summary>
    public long LastCounter { get; private set; }

    private MfaFactor()
    {
    }

    private MfaFactor(Guid userId, byte[] encryptedSecret, DateTime enabledAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        EncryptedSecret = encryptedSecret;
        EnabledAtUtc = enabledAtUtc;
        CreatedAtUtc = enabledAtUtc;
    }

    public static MfaFactor Enable(Guid userId, byte[] encryptedSecret, DateTime nowUtc) =>
        new(userId, encryptedSecret, nowUtc);

    public void RecordUse(long counter, DateTime nowUtc)
    {
        LastCounter = counter;
        LastUsedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
