// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Identity;
using SalekhPos.Domain.Errors;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// EdDSA (Ed25519) compact JWT signer. The JWT is built and parsed
/// manually (no <c>JwtSecurityTokenHandler</c>) because .NET 8's
/// built-in handlers do not natively support EdDSA. The signature is
/// computed with BouncyCastle's Ed25519Signer, which is the canonical
/// FIPS-compatible implementation and is widely audited.
///
/// The signing key is a 32-byte raw Ed25519 seed loaded from a file
/// referenced in configuration. The corresponding 32-byte public key
/// is held in memory; it is also persisted alongside the seed so
/// deploys that share the key do not need to recompute it.
/// </summary>
public sealed class Ed25519JwtSigner : IJwtSigner, IDisposable
{
    private const string EdDsaAlg = "EdDSA";

    private readonly JwtIssuerOptions _options;
    private readonly Ed25519PrivateKeyParameters _privateKey;
    private readonly Ed25519PublicKeyParameters _publicKey;
    private readonly byte[] _publicKeyBytes;

    public Ed25519JwtSigner(IOptions<JwtIssuerOptions> options, IOptions<JwtSigningKeyOptions> keyOptions)
    {
        _options = options.Value;

        var keyPath = keyOptions.Value.PrivateKeyPath;
        if (string.IsNullOrWhiteSpace(keyPath))
        {
            throw new InvalidOperationException("Auth:Jwt:SigningKey:PrivateKeyPath is not configured.");
        }

        if (!File.Exists(keyPath))
        {
            throw new InvalidOperationException($"JWT signing key file '{keyPath}' was not found.");
        }

        // We accept either a 32-byte raw seed (Ed25519 private key) or
        // a 64-byte secret key (the BcEd25519PrivateKeyParameters
        // representation). The script that creates the key writes the
        // 32-byte seed.
        var bytes = File.ReadAllBytes(keyPath);
        if (bytes.Length == 32)
        {
            _privateKey = new Ed25519PrivateKeyParameters(bytes, 0);
        }
        else if (bytes.Length == 64)
        {
            _privateKey = new Ed25519PrivateKeyParameters(bytes, 0);
        }
        else
        {
            throw new InvalidOperationException(
                $"JWT signing key must be 32 or 64 bytes; got {bytes.Length}.");
        }

        _publicKey = _privateKey.GeneratePublicKey();
        _publicKeyBytes = _publicKey.GetEncoded();
    }

    public string Sign(JwtAccessTokenClaims claims)
    {
        var header = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, string>
        {
            ["alg"] = EdDsaAlg,
            ["typ"] = "JWT",
        });

        var payload = JsonSerializer.SerializeToUtf8Bytes(BuildPayload(claims));

        var headerB64 = Base64UrlEncode(header);
        var payloadB64 = Base64UrlEncode(payload);
        var signingInput = Encoding.ASCII.GetBytes($"{headerB64}.{payloadB64}");

        var signer = new Ed25519Signer();
        signer.Init(true, _privateKey);
        signer.BlockUpdate(signingInput, 0, signingInput.Length);
        var signature = signer.GenerateSignature();

        return $"{headerB64}.{payloadB64}.{Base64UrlEncode(signature)}";
    }

    public JwtAccessTokenClaims Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is required.");
        }

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token format is invalid.");
        }

        var headerJson = Base64UrlDecodeToBytes(parts[0]);
        using (var headerDoc = JsonDocument.Parse(headerJson))
        {
            if (headerDoc.RootElement.GetProperty("alg").GetString() != EdDsaAlg)
            {
                throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token uses an unsupported algorithm.");
            }
        }

        var signingInput = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
        var signature = Base64UrlDecodeToBytes(parts[2]);

        var verifier = new Ed25519Signer();
        verifier.Init(false, _publicKey);
        verifier.BlockUpdate(signingInput, 0, signingInput.Length);
        var ok = verifier.VerifySignature(signature);
        if (!ok)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token signature is invalid.");
        }

        var payloadJson = Base64UrlDecodeToBytes(parts[1]);
        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        var nowUtc = DateTime.UtcNow;

        if (root.TryGetProperty("exp", out var expEl) && expEl.TryGetInt64(out var expUnix))
        {
            var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            if (nowUtc >= expUtc)
            {
                throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token has expired.");
            }
        }
        else
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is missing 'exp' claim.");
        }

        if (root.TryGetProperty("nbf", out var nbfEl) && nbfEl.TryGetInt64(out var nbfUnix))
        {
            var nbfUtc = DateTimeOffset.FromUnixTimeSeconds(nbfUnix).UtcDateTime;
            if (nowUtc < nbfUtc.AddSeconds(-30))
            {
                throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is not yet valid.");
            }
        }

        var iss = root.TryGetProperty("iss", out var issEl) ? issEl.GetString() : null;
        if (!string.Equals(iss, _options.Issuer, StringComparison.Ordinal))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token issuer is invalid.");
        }

        var audOk = false;
        if (root.TryGetProperty("aud", out var audEl))
        {
            if (audEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in audEl.EnumerateArray())
                {
                    if (_options.Audiences.Contains(a.GetString()))
                    {
                        audOk = true;
                        break;
                    }
                }
            }
            else if (audEl.ValueKind == JsonValueKind.String)
            {
                audOk = _options.Audiences.Contains(audEl.GetString());
            }
        }
        if (!audOk)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token audience is invalid.");
        }

        var sub = root.GetProperty("sub").GetString()
            ?? throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is missing 'sub' claim.");
        var tid = root.GetProperty("tid").GetString()
            ?? throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is missing 'tid' claim.");
        var role = root.GetProperty("role").GetString()
            ?? throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is missing 'role' claim.");
        var ver = root.GetProperty("ver").GetInt32();
        var jti = root.GetProperty("jti").GetString()
            ?? throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, false, "Access token is missing 'jti' claim.");

        return new JwtAccessTokenClaims(
            Subject: Guid.Parse(sub),
            TenantId: Guid.Parse(tid),
            Role: role,
            TokenVersion: ver,
            Audiences: _options.Audiences,
            Jti: Guid.Parse(jti),
            IssuedAtUtc: DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("iat").GetInt64()).UtcDateTime,
            ExpiresAtUtc: DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("exp").GetInt64()).UtcDateTime);
    }

    private Dictionary<string, object> BuildPayload(JwtAccessTokenClaims claims)
    {
        // JWTs serialize integers as JSON numbers and ids as JSON strings.
        // We use Unix-seconds (int64) for iat/nbf/exp, and explicit .ToString()
        // for the GUID claims.
        return new Dictionary<string, object>
        {
            ["iss"] = _options.Issuer,
            ["aud"] = claims.Audiences.ToArray(),
            ["sub"] = claims.Subject.ToString(),
            ["tid"] = claims.TenantId.ToString(),
            ["role"] = claims.Role,
            ["ver"] = claims.TokenVersion,
            ["jti"] = claims.Jti.ToString(),
            ["iat"] = new DateTimeOffset(claims.IssuedAtUtc, TimeSpan.Zero).ToUnixTimeSeconds(),
            ["nbf"] = new DateTimeOffset(claims.IssuedAtUtc, TimeSpan.Zero).ToUnixTimeSeconds(),
            ["exp"] = new DateTimeOffset(claims.ExpiresAtUtc, TimeSpan.Zero).ToUnixTimeSeconds(),
        };
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecodeToBytes(string text)
    {
        var s = text.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    public void Dispose()
    {
        // Ed25519 key parameters hold raw key bytes; let them be GC'd.
    }
}

/// <summary>Configuration for the JWT signing key.</summary>
public sealed class JwtSigningKeyOptions
{
    public const string SectionName = "Auth:Jwt:SigningKey";

    /// <summary>Path to a 32-byte raw Ed25519 seed used to sign access tokens.</summary>
    public string PrivateKeyPath { get; set; } = string.Empty;
}
