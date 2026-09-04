// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface IMfaFactorRepository
{
    Task AddAsync(MfaFactor factor, CancellationToken cancellationToken);

    Task<MfaFactor?> FindActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
