// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Security.Cryptography;
using System.Text;
using OtpNet;
using SalekhPos.Application.Abstractions;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// TOTP (RFC 6238) service backed by OtpNet. 6-digit codes, 30-second
/// window, SHA-1 (the standard profile). The 1-step skew window
/// accommodates clock drift.
/// </summary>
public sealed class OtpNetTotpService : ITotpService
{
    private const int SecretSizeBytes = 20;
    private const int RecoveryCodeLength = 10;

    public byte[] GenerateSecret()
    {
        var secret = new byte[SecretSizeBytes];
        RandomNumberGenerator.Fill(secret);
        return secret;
    }

    public string BuildProvisioningUri(byte[] secret, string accountLabel, string issuer)
    {
        ArgumentNullException.ThrowIfNull(secret);
        if (string.IsNullOrWhiteSpace(accountLabel))
        {
            throw new ArgumentException("Account label is required.", nameof(accountLabel));
        }

        var base32 = Base32Encoding.ToString(secret);
        return new OtpUri(OtpType.Totp, base32, accountLabel, issuer).ToString();
    }

    public TotpValidationResult Validate(byte[] secret, string code, long lastCounter, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(secret);
        if (string.IsNullOrWhiteSpace(code))
        {
            return new TotpValidationResult(false, lastCounter);
        }

        var totp = new Totp(secret, mode: OtpHashMode.Sha1, step: 30, totpSize: 6);

        // Compute the current 30-second window index.
        var nowWindow = (long)(new DateTimeOffset(nowUtc, TimeSpan.Zero).ToUnixTimeSeconds() / 30);

        // Acceptable window: [now-1, now+1] for clock drift.
        for (var w = nowWindow - 1; w <= nowWindow + 1; w++)
        {
            if (w <= lastCounter)
            {
                continue; // replay defence
            }

            var candidate = totp.ComputeTotp(DateTimeOffset.FromUnixTimeSeconds(w * 30).UtcDateTime);
            if (FixedTimeEquals6Digits(candidate, code))
            {
                return new TotpValidationResult(true, w);
            }
        }

        return new TotpValidationResult(false, lastCounter);
    }

    public IReadOnlyList<string> GenerateRecoveryCodes(int count = 10)
    {
        if (count <= 0)
        {
            return Array.Empty<string>();
        }

        const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        var codes = new string[count];
        var buffer = new byte[RecoveryCodeLength];
        for (var i = 0; i < count; i++)
        {
            RandomNumberGenerator.Fill(buffer);
            var chars = new char[RecoveryCodeLength];
            for (var j = 0; j < RecoveryCodeLength; j++)
            {
                chars[j] = alphabet[buffer[j] % alphabet.Length];
            }
            codes[i] = new string(chars);
        }
        return codes;
    }

    private static bool FixedTimeEquals6Digits(string a, string b)
    {
        var aPadded = a.Length >= 6 ? a : a.PadLeft(6, '0');
        var bPadded = b.Trim().Length >= 6 ? b.Trim() : b.Trim().PadLeft(6, '0');
        var aBytes = Encoding.ASCII.GetBytes(aPadded);
        var bBytes = Encoding.ASCII.GetBytes(bPadded);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
