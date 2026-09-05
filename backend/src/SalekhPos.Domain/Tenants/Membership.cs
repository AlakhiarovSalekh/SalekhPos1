// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Domain.Tenants;

/// <summary>
/// A user's membership in a tenant. A membership has a single role and
/// is optionally scoped to a subset of the tenant's stores (when
/// <see cref="StoreIds"/> is empty, the membership grants access to
/// every store the tenant owns; when non-empty, only the listed ones).
///
/// Lifecycle: <c>Invite</c> creates a pending membership; <c>Accept</c>
/// transitions it to active and stamps <see cref="AcceptedAtUtc"/>.
/// The membership can be <c>Suspend</c>ed or <c>Reactivate</c>d at any
/// time by an Owner.
///
/// Cross-aggregate invariants (enforced in the application / infrastructure
/// layers, not in the domain):
/// <list type="bullet">
///   <item>A user has at most one active or pending membership per tenant.
///         (The <c>Invite</c> factory throws <see cref="DuplicateMembershipException"/>
///         when the in-memory state already has one; the repository
///         enforces it transactionally in Phase 3 Slice 3a.)</item>
///   <item>Every store id in <see cref="StoreIds"/> belongs to the
///         membership's tenant. (Enforced in the <c>GrantStoreAccess</c>
///         use case.)</item>
/// </list>
///
/// The role type currently lives under <c>Identity.UserRole</c> for
/// backwards compatibility with the Phase 2 JWT signer. A future slice
/// will move it under <c>Tenants.Role</c> and replace the field type
/// here. The property name and the public contract stay the same.
/// </summary>
public sealed class Membership : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid TenantId { get; private set; }

    public UserRole Role { get; private set; }

    /// <summary>
    /// The set of store ids the membership grants access to. An empty
    /// list means "all stores in the tenant". A non-empty list is a
    /// strict subset. Memberships for a PlatformSuperAdmin are always
    /// empty (the super admin is not scoped to any tenant's stores).
    /// </summary>
    private readonly List<Guid> _storeIds = new();
    public IReadOnlyCollection<Guid> StoreIds => _storeIds.AsReadOnly();

    public Guid? InvitedByUserId { get; private set; }

    public DateTime InvitedAtUtc { get; private set; }

    /// <summary>Null while the membership is pending; set by <see cref="Accept"/>.</summary>
    public DateTime? AcceptedAtUtc { get; private set; }

    public TenantStatus Status { get; private set; }

    private Membership()
    {
    }

    private Membership(
        Guid userId,
        Guid tenantId,
        UserRole role,
        Guid? invitedByUserId,
        DateTime nowUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Role = role;
        InvitedByUserId = invitedByUserId;
        InvitedAtUtc = nowUtc;
        AcceptedAtUtc = null;
        Status = TenantStatus.Active;
        CreatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Creates a new membership. The application layer is responsible for
    /// checking that the user does not already have a membership in the
    /// tenant (the repository enforces it transactionally; the factory
    /// here is the typed exception carrier the application layer throws
    /// when it has already loaded the conflicting membership into memory).
    /// </summary>
    public static Membership Invite(
        Guid userId,
        Guid tenantId,
        UserRole role,
        Guid? invitedByUserId,
        DateTime nowUtc)
    {
        return new Membership(userId, tenantId, role, invitedByUserId, nowUtc);
    }

    public void Accept(DateTime nowUtc)
    {
        if (AcceptedAtUtc.HasValue)
        {
            throw new MembershipAlreadyAcceptedException(Id);
        }
        AcceptedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void AssignRole(UserRole newRole, DateTime nowUtc)
    {
        Role = newRole;
        UpdatedAtUtc = nowUtc;
    }

    public void GrantStoreAccess(Guid storeId, DateTime nowUtc)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException("Store id is required.", nameof(storeId));
        }
        if (!_storeIds.Contains(storeId))
        {
            _storeIds.Add(storeId);
        }
        UpdatedAtUtc = nowUtc;
    }

    public void RevokeStoreAccess(Guid storeId, DateTime nowUtc)
    {
        if (_storeIds.Remove(storeId))
        {
            UpdatedAtUtc = nowUtc;
        }
    }

    public void GrantAllStoreAccess(DateTime nowUtc)
    {
        _storeIds.Clear();
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Returns true when the membership grants access to the given
    /// store. An empty <see cref="StoreIds"/> means "all stores".
    /// </summary>
    public bool IsScopedToStore(Guid storeId)
    {
        return _storeIds.Count == 0 || _storeIds.Contains(storeId);
    }

    public void Suspend(DateTime nowUtc)
    {
        Status = TenantStatus.Suspended;
        UpdatedAtUtc = nowUtc;
    }

    public void Reactivate(DateTime nowUtc)
    {
        Status = TenantStatus.Active;
        UpdatedAtUtc = nowUtc;
    }
}
