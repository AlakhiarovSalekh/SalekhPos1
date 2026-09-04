// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.ResetPassword;

public sealed class ResetPasswordHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;

    public ResetPasswordHandler(
        IClock clock,
        IUserRepository users,
        IPasswordResetTokenRepository tokens,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher)
    {
        _clock = clock;
        _users = users;
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
    }

    public async Task<Result<Unit>> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken))
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.PasswordReset, isReplay: false, "Reset token is required.");
        }

        var hashed = HashedToken.From(command.RawToken).Hex;
        var token = await _tokens.FindActiveByHashAsync(hashed, cancellationToken).ConfigureAwait(false);
        var nowUtc = _clock.UtcNow;
        if (token is null)
        {
            throw new InvalidTokenException(InvalidTokenException.TokenKind.PasswordReset, isReplay: false, "Reset token is invalid or expired.");
        }

        token.Consume(nowUtc);

        var user = await _users.FindByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User for active reset token was not found.");

        user.ChangePassword(PasswordHash.FromEncoded(_hasher.Hash(command.NewPassword)), nowUtc);

        // Force all existing sessions for this user to log out. This is
        // the canonical "password was just changed" defence.
        await _refreshTokens.RevokeFamilyAsync(Guid.Empty, "password.reset", nowUtc, cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }
}
