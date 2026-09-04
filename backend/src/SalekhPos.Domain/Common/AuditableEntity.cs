// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Common;

/// <summary>
/// Base class for entities that the system audits. CreatedAtUtc is set on
/// insert; UpdatedAtUtc and the rest are maintained by an EF Core
/// interceptor in the Infrastructure layer.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime CreatedAtUtc { get; protected set; }

    public Guid? CreatedByUserId { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public Guid? UpdatedByUserId { get; protected set; }
}
