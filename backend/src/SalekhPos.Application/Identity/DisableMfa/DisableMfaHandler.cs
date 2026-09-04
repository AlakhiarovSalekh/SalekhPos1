// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Application.Identity.DisableMfa;

public sealed class DisableMfaHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IEmailSender _email;

    public DisableMfaHandler(
        IClock clock,
        IUserRepository users,
        ITenantRepository tenants,
        IEmailSender email)
    {
        _clock = clock;
        _users = users;
        _tenants = tenants;
        _email = email;
    }

    public async Task<Result<Unit>> HandleAsync(DisableMfaCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdWithMfaAsync(command.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        var nowUtc = _clock.UtcNow;
        user.DisableMfa(nowUtc);

        var tenant = await _tenants.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        await _email.SendMfaDisabledAsync(user.Email, user.FullName, tenant?.Name ?? string.Empty, cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }
}
