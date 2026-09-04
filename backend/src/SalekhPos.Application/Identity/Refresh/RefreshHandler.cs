// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.Refresh;

public sealed class RefreshHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenGenerator _tokens;
    private readonly IJwtSigner _jwt;
    private readonly TokenLifetimeOptions _lifetimes;
    private readonly JwtIssuerOptions _jwtOptions;

    public RefreshHandler(
        IClock clock,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        ITokenGenerator tokens,
        IJwtSigner jwt,
        IOptions<TokenLifetimeOptions> lifetimes,
        IOptions<JwtIssuerOptions> jwtOptions)
    {
        _clock = clock;
        _users = users;
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _jwt = jwt;
        _lifetimes = lifetimes.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<TokenPair>> HandleAsync(RefreshCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RawRefreshToken))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, isReplay: false, "Refresh token is required.");
        }

        var hashed = HashedToken.From(command.RawRefreshToken).Hex;
        var presented = await _refreshTokens.FindByHashAsync(hashed, cancellationToken).ConfigureAwait(false);
        if (presented is null)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, isReplay: false, "Refresh token is invalid or expired.");
        }

        var nowUtc = _clock.UtcNow;

        // Reuse detection: if the presented token was already consumed,
        // this is a strong signal of theft. Revoke the entire family and
        // bump the user's token_version so even valid access tokens are
        // rejected until re-auth.
        if (presented.ConsumedAtUtc is not null)
        {
            await _refreshTokens.RevokeFamilyAsync(presented.FamilyId, "refresh.reuse_detected", nowUtc, cancellationToken).ConfigureAwait(false);
            var stolenUser = await _users.FindByIdAsync(presented.UserId, cancellationToken).ConfigureAwait(false);
            stolenUser?.RevokeAllTokens(nowUtc);
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, isReplay: true, "Refresh token reuse detected; all sessions for this user have been revoked.");
        }

        if (!presented.IsActive(nowUtc))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, isReplay: false, "Refresh token is invalid or expired.");
        }

        var user = await _users.FindByIdAsync(presented.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User for active refresh token was not found.");

        if (user.Status is UserStatus.Disabled or UserStatus.Locked)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.RefreshToken, isReplay: false, "User is not allowed to refresh tokens.");
        }

        // Issue a new refresh token in the same family and link it to the
        // presented one. The presented token is then marked consumed in
        // the same transaction.
        var (newRaw, newHash) = _tokens.NewOpaqueToken();
        var newRefresh = RefreshToken.Issue(
            userId: user.Id,
            hashedToken: HashedToken.From(newRaw),
            familyId: presented.FamilyId,
            issuedAtUtc: nowUtc,
            lifetime: _lifetimes.RefreshTokenLifetime);
        await _refreshTokens.AddAsync(newRefresh, cancellationToken).ConfigureAwait(false);

        presented.Consume(newRefresh.Id, nowUtc);

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

        return Result<TokenPair>.Success(new TokenPair(
            AccessToken: accessToken,
            RefreshTokenRaw: newRaw,
            AccessTokenExpiresAtUtc: accessExpires,
            RefreshTokenExpiresAtUtc: newRefresh.ExpiresAtUtc));
    }
}
