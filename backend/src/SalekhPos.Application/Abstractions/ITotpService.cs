// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// TOTP (RFC 6238) service. Generates secrets, derives the provisioning
/// URI, and validates one-time codes (with a 1-step skew window).
/// </summary>
public interface ITotpService
{
    /// <summary>Generates a new 20-byte random shared secret.</summary>
    byte[] GenerateSecret();

    /// <summary>Builds an otpauth:// URI suitable for QR-code enrolment.</summary>
    string BuildProvisioningUri(byte[] secret, string accountLabel, string issuer);

    /// <summary>Validates a 6-digit code against the given secret.</summary>
    TotpValidationResult Validate(byte[] secret, string code, long lastCounter, DateTime nowUtc);

    /// <summary>Generates a set of one-time recovery codes.</summary>
    IReadOnlyList<string> GenerateRecoveryCodes(int count = 10);
}

public readonly record struct TotpValidationResult(bool IsValid, long Counter);
