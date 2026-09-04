// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.ChangePassword;

public sealed class ChangePasswordHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordHandler(
        IClock clock,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher hasher)
    {
        _clock = clock;
        _users = users;
        _refreshTokens = refreshTokens;
        _hasher = hasher;
    }

    public async Task<Result<Unit>> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(command.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidCredentialsException();

        if (!_hasher.Verify(command.CurrentPassword, user.PasswordHash.Encoded))
        {
            throw new InvalidCredentialsException();
        }

        var nowUtc = _clock.UtcNow;
        user.ChangePassword(PasswordHash.FromEncoded(_hasher.Hash(command.NewPassword)), nowUtc);

        // The current access token's token_version no longer matches,
        // so it will be rejected by the JwtBearer validator on the next
        // request. Other devices' refresh tokens are revoked so they
        // cannot silently continue.
        await _refreshTokens.RevokeFamilyAsync(Guid.Empty, "password.changed", nowUtc, cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }
}
