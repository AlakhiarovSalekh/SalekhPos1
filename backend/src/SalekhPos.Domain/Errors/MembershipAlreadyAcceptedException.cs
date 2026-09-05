// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Raised when <c>Membership.Accept</c> is called on a membership that
/// has already been accepted. Indicates either a client bug (double-click
/// on the accept button) or a race condition; the membership is
/// idempotent on the accept action in spirit, but the typed exception
/// is the only way the application layer can tell the caller that the
/// state is already "accepted" and re-issuing tokens / re-routing is
/// unnecessary.
/// </summary>
public sealed class MembershipAlreadyAcceptedException : DomainException
{
    public Guid MembershipId { get; }

    public MembershipAlreadyAcceptedException(Guid membershipId)
        : base($"Membership {membershipId} has already been accepted.")
    {
        MembershipId = membershipId;
    }
}
