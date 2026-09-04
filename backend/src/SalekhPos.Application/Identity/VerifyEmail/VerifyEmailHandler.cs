// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.VerifyEmail;

public sealed class VerifyEmailHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IEmailVerificationTokenRepository _tokens;

    public VerifyEmailHandler(IClock clock, IUserRepository users, IEmailVerificationTokenRepository tokens)
    {
        _clock = clock;
        _users = users;
        _tokens = tokens;
    }

    public async Task<Result<Unit>> HandleAsync(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RawToken))
        {
            return Result<Unit>.Failure("auth.token_invalid", "Token is required.");
        }

        var hashed = HashedToken.From(command.RawToken).Hex;
        var token = await _tokens.FindActiveByHashAsync(hashed, cancellationToken).ConfigureAwait(false);

        var nowUtc = _clock.UtcNow;
        if (token is null)
        {
            // Either unknown, expired, or already used. We do NOT
            // distinguish to the client (no enumeration).
            throw new InvalidTokenException(InvalidTokenException.TokenKind.EmailVerification, isReplay: false, "Email verification token is invalid or expired.");
        }

        token.Consume(nowUtc);

        var user = await _users.FindByIdAsync(token.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User for active verification token was not found.");
        user.MarkEmailVerified(nowUtc);

        return Result<Unit>.Success(Unit.Value);
    }
}
