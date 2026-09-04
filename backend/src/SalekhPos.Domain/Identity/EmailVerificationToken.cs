// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// A single-use email-verification token. The raw token is never stored;
/// only its hash. After consumption the row is marked consumed and a
/// replay attempt returns "already used".
/// </summary>
public sealed class EmailVerificationToken : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public HashedToken HashedToken { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }

    private EmailVerificationToken()
    {
    }

    private EmailVerificationToken(Guid userId, HashedToken hashedToken, DateTime issuedAtUtc, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        HashedToken = hashedToken;
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = issuedAtUtc;
    }

    public static EmailVerificationToken Issue(
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
