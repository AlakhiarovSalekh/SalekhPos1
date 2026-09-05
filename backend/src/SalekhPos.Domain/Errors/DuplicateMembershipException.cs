// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when an attempt is made to invite a user to a tenant they
/// already have an active or pending membership in. Enforced in the
/// application layer (the domain's <c>Membership.Invite</c> factory
/// throws it when the in-memory state already contains a membership;
/// the repository in Slice 3a enforces it transactionally).
/// </summary>
public sealed class DuplicateMembershipException : DomainException
{
    public Guid UserId { get; }
    public Guid TenantId { get; }

    public DuplicateMembershipException(Guid userId, Guid tenantId)
        : base($"User {userId} already has a membership in tenant {tenantId}.")
    {
        UserId = userId;
        TenantId = tenantId;
    }
}
