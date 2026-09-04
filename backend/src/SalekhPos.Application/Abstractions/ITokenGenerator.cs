// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// Generates opaque URL-safe tokens (refresh, email-verification, password-reset,
/// MFA recovery). The same token string is returned to the caller (to embed in
/// an email or hand to the client) and the SHA-256 hash is what the
/// repository stores.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>Returns a new opaque token: (raw, hash) where the raw value
    /// is what the client/email receives and the hash is what gets persisted.</summary>
    (string Raw, string Hash) NewOpaqueToken(int byteLength = 32);

    /// <summary>Generates a new JWT ID (UUID v4).</summary>
    Guid NewJti();
}
