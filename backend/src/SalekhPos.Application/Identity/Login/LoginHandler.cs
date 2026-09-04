// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.Login;

public sealed class LoginHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokens;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtSigner _jwt;
    private readonly AuthLockoutOptions _lockout;
    private readonly TokenLifetimeOptions _lifetimes;
    private readonly JwtIssuerOptions _jwtOptions;

    public LoginHandler(
        IClock clock,
        IUserRepository users,
        IPasswordHasher hasher,
        ITokenGenerator tokens,
        IRefreshTokenRepository refreshTokens,
        IJwtSigner jwt,
        IOptions<AuthLockoutOptions> lockout,
        IOptions<TokenLifetimeOptions> lifetimes,
        IOptions<JwtIssuerOptions> jwtOptions)
    {
        _clock = clock;
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _jwt = jwt;
        _lockout = lockout.Value;
        _lifetimes = lifetimes.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<LoginOutcome>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        if (!EmailAddress.TryCreate(command.Email, out var email))
        {
            // Deliberately the same response as a wrong password, so we
            // never reveal whether the email exists.
            throw new InvalidCredentialsException();
        }

        var user = await _users.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var nowUtc = _clock.UtcNow;

        if (user.Status == UserStatus.Disabled)
        {
            // Disabled is not the same as locked: do not leak which one
            // applies by wording. We use 401 for both. The
            // infrastructure's ProblemDetails mapper turns InvalidCredentials
            // into 401.
            throw new InvalidCredentialsException();
        }

        if (user.IsLockedAt(nowUtc))
        {
            throw new AccountLockedException(user.LockedUntilUtc!.Value);
        }

        if (!_hasher.Verify(command.Password, user.PasswordHash.Encoded))
        {
            user.RecordFailedLogin(nowUtc, _lockout.MaxFailedAttempts, _lockout.LockoutDuration);
            throw new InvalidCredentialsException();
        }

        if (user.Status == UserStatus.PendingVerification)
        {
            throw new EmailNotVerifiedException();
        }

        // Step 1 succeeded: clear failed-login counter and record the
        // successful login. We issue tokens in a second step.
        user.RecordSuccessfulLogin(nowUtc);

        if (user.Mfa is not null)
        {
            // MFA is enabled. We do not issue tokens yet. The web client
            // must call /auth/mfa/verify with the next TOTP code, and
            // a one-time MfaTicket is returned instead.
            return Result<LoginOutcome>.Success(new LoginOutcome.MfaRequired(user.Id, user.TenantId));
        }

        var pair = await IssueTokensAsync(user, cancellationToken).ConfigureAwait(false);
        return Result<LoginOutcome>.Success(new LoginOutcome.Authenticated(pair));
    }

    private async Task<TokenPair> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var nowUtc = _clock.UtcNow;
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

        var (rawRefresh, refreshHash) = _tokens.NewOpaqueToken();
        var refreshToken = RefreshToken.Issue(
            userId: user.Id,
            hashedToken: HashedToken.From(rawRefresh),
            familyId: Guid.Empty, // new family
            issuedAtUtc: nowUtc,
            lifetime: _lifetimes.RefreshTokenLifetime);
        await _refreshTokens.AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);

        return new TokenPair(
            AccessToken: accessToken,
            RefreshTokenRaw: rawRefresh,
            AccessTokenExpiresAtUtc: accessExpires,
            RefreshTokenExpiresAtUtc: refreshToken.ExpiresAtUtc);
    }
}

/// <summary>Discriminated login outcome. Either fully authenticated, or
/// MFA is required and the client must complete the second factor.</summary>
public abstract record LoginOutcome
{
    public sealed record Authenticated(TokenPair Tokens) : LoginOutcome;

    public sealed record MfaRequired(Guid UserId, Guid TenantId) : LoginOutcome;
}
