// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.VerifyMfa;

public sealed class VerifyMfaHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IMfaFactorRepository _factors;
    private readonly IMfaRecoveryCodeRepository _recoveryCodes;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenGenerator _tokens;
    private readonly IJwtSigner _jwt;
    private readonly ITotpService _totp;
    private readonly IAesGcmCipher _cipher;
    private readonly TokenLifetimeOptions _lifetimes;
    private readonly JwtIssuerOptions _jwtOptions;

    public VerifyMfaHandler(
        IClock clock,
        IUserRepository users,
        IMfaFactorRepository factors,
        IMfaRecoveryCodeRepository recoveryCodes,
        IRefreshTokenRepository refreshTokens,
        ITokenGenerator tokens,
        IJwtSigner jwt,
        ITotpService totp,
        IAesGcmCipher cipher,
        IOptions<TokenLifetimeOptions> lifetimes,
        IOptions<JwtIssuerOptions> jwtOptions)
    {
        _clock = clock;
        _users = users;
        _factors = factors;
        _recoveryCodes = recoveryCodes;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _jwt = jwt;
        _totp = totp;
        _cipher = cipher;
        _lifetimes = lifetimes.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<TokenPair>> HandleAsync(VerifyMfaCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdWithMfaAsync(command.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidCredentialsException();

        var factor = user.Mfa
            ?? await _factors.FindActiveByUserIdAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (factor is null)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.MfaCode, isReplay: false, "MFA is not enabled for this user.");
        }

        var nowUtc = _clock.UtcNow;

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.MfaCode, isReplay: false, "MFA code is required.");
        }

        // Try TOTP first.
        var secret = DecryptSecret(factor.EncryptedSecret);
        var validation = _totp.Validate(secret, command.Code, factor.LastCounter, nowUtc);

        if (validation.IsValid)
        {
            factor.RecordUse(validation.Counter, nowUtc);
        }
        else
        {
            // Fall back to recovery code.
            var codeHash = HashedToken.From(command.Code.Trim().ToLowerInvariant()).Hex;
            var codes = await _recoveryCodes.ListActiveByUserIdAsync(user.Id, cancellationToken).ConfigureAwait(false);
            var matched = codes.FirstOrDefault(c => c.HashedCode.Hex == codeHash && c.ConsumedAtUtc is null);
            if (matched is null)
            {
                throw new InvalidTokenException(InvalidTokenException.TokenKind.MfaCode, isReplay: false, "MFA code is invalid.");
            }

            matched.Consume(nowUtc);
        }

        user.RecordSuccessfulLogin(nowUtc);

        // Issue tokens.
        var accessExpires = nowUtc.Add(_lifetimes.AccessTokenLifetime);
        var accessToken = _jwt.Sign(new JwtAccessTokenClaims(
            Subject: user.Id,
            TenantId: user.TenantId,
            Role: user.Role.ToString(),
            TokenVersion: user.TokenVersion,
            Audiences: _jwtOptions.Audiences,
            Jti: _tokens.NewJti(),
            IssuedAtUtc: nowUtc,
            ExpiresAtUtc: accessExpires));

        var (rawRefresh, _) = _tokens.NewOpaqueToken();
        var refresh = RefreshToken.Issue(
            userId: user.Id,
            hashedToken: HashedToken.From(rawRefresh),
            familyId: Guid.Empty,
            issuedAtUtc: nowUtc,
            lifetime: _lifetimes.RefreshTokenLifetime);
        await _refreshTokens.AddAsync(refresh, cancellationToken).ConfigureAwait(false);

        return Result<TokenPair>.Success(new TokenPair(
            AccessToken: accessToken,
            RefreshTokenRaw: rawRefresh,
            AccessTokenExpiresAtUtc: accessExpires,
            RefreshTokenExpiresAtUtc: refresh.ExpiresAtUtc));
    }

    private byte[] DecryptSecret(byte[] blob)
    {
        // Layout produced by EnableMfaHandler.Serialize: [12][N][16]
        const int nonceSize = 12;
        const int tagSize = 16;
        if (blob.Length < nonceSize + tagSize)
        {
            throw new InvalidOperationException("Encrypted TOTP secret is malformed.");
        }

        var ciphertext = new byte[blob.Length - nonceSize - tagSize];
        var nonce = new byte[nonceSize];
        var tag = new byte[tagSize];
        Buffer.BlockCopy(blob, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(blob, nonceSize, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(blob, nonceSize + ciphertext.Length, tag, 0, tagSize);

        return _cipher.Decrypt(new AesGcmResult(ciphertext, nonce, tag));
    }
}
