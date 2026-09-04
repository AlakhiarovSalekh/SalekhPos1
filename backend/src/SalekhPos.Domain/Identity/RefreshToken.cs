// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// A refresh token entity. The raw token is never stored; only
/// <see cref="HashedToken"/>. Tokens are issued in families
/// (<see cref="FamilyId"/>); on reuse detection the entire family is
/// revoked and the user must re-authenticate.
/// </summary>
public sealed class RefreshToken : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public HashedToken HashedToken { get; private set; }

    public Guid FamilyId { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public string? RevocationReason { get; private set; }

    // EF Core.
    private RefreshToken()
    {
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        HashedToken hashedToken,
        Guid familyId,
        DateTime issuedAtUtc,
        DateTime expiresAtUtc)
    {
        Id = id;
        UserId = userId;
        HashedToken = hashedToken;
        FamilyId = familyId;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = issuedAtUtc;
    }

    public static RefreshToken Issue(
        Guid userId,
        HashedToken hashedToken,
        Guid familyId,
        DateTime issuedAtUtc,
        TimeSpan lifetime)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }

        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            hashedToken,
            familyId == Guid.Empty ? Guid.NewGuid() : familyId,
            issuedAtUtc,
            issuedAtUtc.Add(lifetime));
    }

    public bool IsActive(DateTime nowUtc) =>
        ConsumedAtUtc is null && RevocationReason is null && nowUtc < ExpiresAtUtc;

    /// <summary>
    /// Mark this token as consumed and link it to the replacement. The
    /// caller is expected to issue a fresh <see cref="RefreshToken"/> in
    /// the same family and persist both in one transaction.
    /// </summary>
    public void Consume(Guid replacedByTokenId, DateTime nowUtc)
    {
        if (ConsumedAtUtc is not null)
        {
            throw new InvalidOperationException("Refresh token has already been consumed.");
        }

        ConsumedAtUtc = nowUtc;
        ReplacedByTokenId = replacedByTokenId;
        UpdatedAtUtc = nowUtc;
    }

    public void Revoke(string reason, DateTime nowUtc)
    {
        RevocationReason = reason;
        UpdatedAtUtc = nowUtc;
    }
}
