// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.Logout;

public sealed class LogoutHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutHandler(IClock clock, IUserRepository users, IRefreshTokenRepository refreshTokens)
    {
        _clock = clock;
        _users = users;
        _refreshTokens = refreshTokens;
    }

    /// <summary>
    /// Logout-everywhere: bumps the user's token_version, which makes
    /// every previously-issued access token invalid (the JwtBearer
    /// validator checks the version against the DB). All refresh
    /// tokens in the family are revoked.
    /// </summary>
    public async Task<Result<Unit>> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(command.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        var nowUtc = _clock.UtcNow;
        user.RevokeAllTokens(nowUtc);
        await _refreshTokens.RevokeFamilyAsync(Guid.Empty, "logout", nowUtc, cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }
}
