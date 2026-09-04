// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions.Persistence;

/// <summary>
/// Unit of work. Wraps the EF Core SaveChangesAsync so use cases depend
/// on the abstraction instead of the DbContext directly.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
