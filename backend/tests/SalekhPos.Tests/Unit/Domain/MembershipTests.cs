// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentAssertions;
using SalekhPos.Domain.Errors;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;
using Xunit;

namespace SalekhPos.Tests.Unit.Domain;

public sealed class MembershipTests
{
    private static readonly DateTime Now = new(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid InviterId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static Membership New()
    {
        return Membership.Invite(UserId, TenantId, UserRole.TenantManager, InviterId, Now);
    }

    [Fact]
    public void InviteCreatesPendingMembership()
    {
        var m = New();
        m.Id.Should().NotBe(Guid.Empty);
        m.UserId.Should().Be(UserId);
        m.TenantId.Should().Be(TenantId);
        m.Role.Should().Be(UserRole.TenantManager);
        m.InvitedByUserId.Should().Be(InviterId);
        m.InvitedAtUtc.Should().Be(Now);
        m.AcceptedAtUtc.Should().BeNull();
        m.StoreIds.Should().BeEmpty();
        m.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void AcceptStampsAcceptedAtUtc()
    {
        var m = New();
        var later = Now.AddMinutes(5);
        m.Accept(later);
        m.AcceptedAtUtc.Should().Be(later);
    }

    [Fact]
    public void AcceptTwiceThrowsAlreadyAccepted()
    {
        var m = New();
        m.Accept(Now.AddMinutes(1));
        Action act = () => m.Accept(Now.AddMinutes(2));
        act.Should().Throw<MembershipAlreadyAcceptedException>();
    }

    [Fact]
    public void AssignRoleUpdatesRole()
    {
        var m = New();
        m.AssignRole(UserRole.TenantOwner, Now.AddMinutes(1));
        m.Role.Should().Be(UserRole.TenantOwner);
    }

    [Fact]
    public void GrantAndRevokeStoreAccess()
    {
        var m = New();
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();

        m.GrantStoreAccess(storeA, Now.AddMinutes(1));
        m.GrantStoreAccess(storeB, Now.AddMinutes(2));
        m.StoreIds.Should().BeEquivalentTo(new[] { storeA, storeB });

        m.RevokeStoreAccess(storeA, Now.AddMinutes(3));
        m.StoreIds.Should().BeEquivalentTo(new[] { storeB });
    }

    [Fact]
    public void GrantStoreAccessIsIdempotent()
    {
        var m = New();
        var store = Guid.NewGuid();
        m.GrantStoreAccess(store, Now);
        m.GrantStoreAccess(store, Now);
        m.StoreIds.Should().HaveCount(1);
    }

    [Fact]
    public void RevokeStoreAccessOnMissingStoreIsNoop()
    {
        var m = New();
        m.RevokeStoreAccess(Guid.NewGuid(), Now);
        m.StoreIds.Should().BeEmpty();
    }

    [Fact]
    public void GrantAllStoreAccessClearsList()
    {
        var m = New();
        m.GrantStoreAccess(Guid.NewGuid(), Now);
        m.GrantAllStoreAccess(Now);
        m.StoreIds.Should().BeEmpty();
    }

    [Fact]
    public void IsScopedToStoreWhenEmptyReturnsTrue()
    {
        var m = New();
        m.IsScopedToStore(Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void IsScopedToStoreRespectsList()
    {
        var m = New();
        var storeA = Guid.NewGuid();
        var storeB = Guid.NewGuid();
        m.GrantStoreAccess(storeA, Now);
        m.IsScopedToStore(storeA).Should().BeTrue();
        m.IsScopedToStore(storeB).Should().BeFalse();
    }

    [Fact]
    public void SuspendAndReactivate()
    {
        var m = New();
        m.Suspend(Now.AddMinutes(1));
        m.Status.Should().Be(TenantStatus.Suspended);
        m.Reactivate(Now.AddMinutes(2));
        m.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void GrantStoreAccessRejectsEmptyStoreId()
    {
        var m = New();
        Action act = () => m.GrantStoreAccess(Guid.Empty, Now);
        act.Should().Throw<ArgumentException>();
    }
}
