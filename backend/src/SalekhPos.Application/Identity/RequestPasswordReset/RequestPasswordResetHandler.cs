// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Application.Identity.RequestPasswordReset;

public sealed class RequestPasswordResetHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IPasswordResetTokenRepository _tokens;
    private readonly ITokenGenerator _generator;
    private readonly IEmailSender _email;
    private readonly TokenLifetimeOptions _lifetimes;

    public RequestPasswordResetHandler(
        IClock clock,
        IUserRepository users,
        ITenantRepository tenants,
        IPasswordResetTokenRepository tokens,
        ITokenGenerator generator,
        IEmailSender email,
        IOptions<TokenLifetimeOptions> lifetimes)
    {
        _clock = clock;
        _users = users;
        _tenants = tenants;
        _tokens = tokens;
        _generator = generator;
        _email = email;
        _lifetimes = lifetimes.Value;
    }

    public async Task<Result<Unit>> HandleAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        // We always return Success, regardless of whether the email
        // exists. Otherwise this is a user-enumeration oracle.
        if (!EmailAddress.TryCreate(command.Email, out var email))
        {
            return Result<Unit>.Success(Unit.Value);
        }

        var user = await _users.FindByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        var tenant = await _tenants.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        var tenantName = tenant?.Name ?? string.Empty;

        var (raw, hash) = _generator.NewOpaqueToken();
        var nowUtc = _clock.UtcNow;
        var token = PasswordResetToken.Issue(user.Id, HashedToken.From(raw), nowUtc, _lifetimes.PasswordResetTokenLifetime);
        await _tokens.AddAsync(token, cancellationToken).ConfigureAwait(false);

        await _email.SendPasswordResetAsync(email, user.FullName, raw, tenantName, cancellationToken).ConfigureAwait(false);

        return Result<Unit>.Success(Unit.Value);
    }
}
