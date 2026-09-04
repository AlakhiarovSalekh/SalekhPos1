// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> FindByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(EmailAddress email, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a user together with their active MFA factor (if any). Use
    /// the eager-load variant on every login / MFA-verify code path so
    /// we never issue a token without first checking MFA enrolment.
    /// </summary>
    Task<User?> FindByIdWithMfaAsync(Guid id, CancellationToken cancellationToken);
}
