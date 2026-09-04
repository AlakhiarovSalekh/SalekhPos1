// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// Password hashing abstraction. The default implementation uses Argon2id
/// with parameters from configuration; the interface stays small so unit
/// tests can use a deterministic fake.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plaintext password to an Argon2id encoded string.</summary>
    string Hash(string plaintext);

    /// <summary>Verifies a plaintext password against an Argon2id encoded string.</summary>
    bool Verify(string plaintext, string encoded);
}
