// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken);

    Task<EmailVerificationToken?> FindActiveByHashAsync(string hashedToken, CancellationToken cancellationToken);
}
