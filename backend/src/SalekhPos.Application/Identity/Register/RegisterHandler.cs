// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Application.Identity.Register;

public sealed class RegisterHandler
{
    private readonly IClock _clock;
    private readonly ITenantRepository _tenants;
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokens;
    private readonly IEmailVerificationTokenRepository _emailTokens;
    private readonly IEmailSender _email;
    private readonly IPasswordHasher _hasher;
    private readonly TokenLifetimeOptions _lifetimes;

    public RegisterHandler(
        IClock clock,
        ITenantRepository tenants,
        IUserRepository users,
        ITokenGenerator tokens,
        IEmailVerificationTokenRepository emailTokens,
        IEmailSender email,
        IPasswordHasher hasher,
        Microsoft.Extensions.Options.IOptions<TokenLifetimeOptions> lifetimes)
    {
        _clock = clock;
        _tenants = tenants;
        _users = users;
        _tokens = tokens;
        _emailTokens = emailTokens;
        _email = email;
        _hasher = hasher;
        _lifetimes = lifetimes.Value;
    }

    public async Task<Result<RegisterResult>> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = EmailAddress.Create(command.Email);

        // Reject duplicate email across the whole platform (one email =
        // one account, period). This applies to tenants as well as
        // future platform admin onboarding.
        if (await _users.EmailExistsAsync(email, cancellationToken).ConfigureAwait(false))
        {
            return Result<RegisterResult>.Failure("auth.email_taken", "An account with this email already exists.", nameof(command.Email));
        }

        // Reject duplicate tenant slug.
        if (await _tenants.FindBySlugAsync(command.TenantSlug, cancellationToken).ConfigureAwait(false) is not null)
        {
            return Result<RegisterResult>.Failure("auth.tenant_slug_taken", "A business with this URL slug already exists.", nameof(command.TenantSlug));
        }

        var nowUtc = _clock.UtcNow;
        var tenant = Tenant.Create(command.TenantName, command.TenantSlug, nowUtc);
        await _tenants.AddAsync(tenant, cancellationToken).ConfigureAwait(false);

        var password = PasswordHash.FromEncoded(_hasher.Hash(command.Password));
        var user = User.Create(
            tenantId: tenant.Id,
            email: email,
            fullName: command.FullName,
            passwordHash: password,
            role: UserRole.TenantOwner,
            nowUtc: nowUtc);
        await _users.AddAsync(user, cancellationToken).ConfigureAwait(false);

        var (rawToken, hash) = _tokens.NewOpaqueToken();
        var verification = EmailVerificationToken.Issue(
            user.Id,
            HashedToken.From(rawToken),
            nowUtc,
            _lifetimes.EmailVerificationTokenLifetime);
        await _emailTokens.AddAsync(verification, cancellationToken).ConfigureAwait(false);

        await _email.SendEmailVerificationAsync(
            email,
            user.FullName,
            rawToken,
            tenant.Name,
            cancellationToken).ConfigureAwait(false);

        return Result<RegisterResult>.Success(new RegisterResult(user.Id, tenant.Id, tenant.Slug));
    }
}

public sealed record RegisterResult(Guid UserId, Guid TenantId, string TenantSlug);
