// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken);

    Task<PasswordResetToken?> FindActiveByHashAsync(string hashedToken, CancellationToken cancellationToken);
}
