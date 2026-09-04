// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions.Persistence;

public interface IMfaRecoveryCodeRepository
{
    Task AddManyAsync(IEnumerable<MfaRecoveryCode> codes, CancellationToken cancellationToken);

    Task<IReadOnlyList<MfaRecoveryCode>> ListActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
