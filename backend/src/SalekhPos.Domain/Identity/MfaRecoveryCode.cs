// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// A one-time recovery code. The raw code is never stored; only its
/// SHA-256 hash. Codes are 10 alphanumeric characters and may only be
/// used once. Regenerating the set revokes the previous codes.
/// </summary>
public sealed class MfaRecoveryCode : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public HashedToken HashedCode { get; private set; }

    public DateTime IssuedAtUtc { get; private set; }

    public DateTime? ConsumedAtUtc { get; private set; }

    private MfaRecoveryCode()
    {
    }

    private MfaRecoveryCode(Guid userId, HashedToken hashedCode, DateTime issuedAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        HashedCode = hashedCode;
        IssuedAtUtc = issuedAtUtc;
        CreatedAtUtc = issuedAtUtc;
    }

    public static MfaRecoveryCode Issue(Guid userId, HashedToken hashedCode, DateTime nowUtc) =>
        new(userId, hashedCode, nowUtc);

    public void Consume(DateTime nowUtc)
    {
        ConsumedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }
}
