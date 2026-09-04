// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// A single-use password-reset token. Shorter TTL than the email
/// verification token (default: 30 minutes). Replay returns "already
/// used"; expiry returns "expired".
/// </summary>
public sealed class PasswordResetToken : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public HashedToken HashedToken { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }

    private PasswordResetToken()
    {
    }

    private PasswordResetToken(Guid userId, HashedToken hashedToken, DateTime issuedAtUtc, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        HashedToken = hashedToken;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = issuedAtUtc;
    }

    public static PasswordResetToken Issue(
        Guid userId,
        HashedToken hashedToken,
        DateTime issuedAtUtc,
        TimeSpan lifetime) =>
        new(userId, hashedToken, issuedAtUtc, issuedAtUtc.Add(lifetime));

    public bool IsActive(DateTime nowUtc) =>
        ConsumedAtUtc is null && nowUtc < ExpiresAtUtc;

    public void Consume(DateTime nowUtc)
    {
        ConsumedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
