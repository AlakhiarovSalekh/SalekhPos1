// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Identity;

/// <summary>
/// The User aggregate root. One user belongs to exactly one tenant
/// (or to the platform for <see cref="UserRole.PlatformSuperAdmin"/>).
/// Owns its password hash, token version, lockout state, and MFA
/// enrolment.
/// </summary>
public sealed class User : AuditableEntity
{
    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public EmailAddress Email { get; private set; }

    public string FullName { get; private set; } = string.Empty;

    public PasswordHash PasswordHash { get; private set; }

    public UserStatus Status { get; private set; }

    public UserRole Role { get; private set; }

    /// <summary>
    /// Monotonically increasing version. Every issued access token carries
    /// the version at issue time; the JwtBearer validator bumps on
    /// logout-all, password change, role change, and account disable.
    /// </summary>
    public int TokenVersion { get; private set; }

    public DateTime? EmailVerifiedAtUtc { get; private set; }

    public DateTime? LockedUntilUtc { get; private set; }

    public int FailedLoginCount { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public DateTime? LastFailedLoginAtUtc { get; private set; }

    /// <summary>
    /// MFA enrolment. Lazy-loaded by the repository. When non-null, login
    /// must be followed by an MFA verification step before tokens are
    /// issued.
    /// </summary>
    public MfaFactor? Mfa { get; private set; }

    // EF Core.
    private User()
    {
    }

    private User(
        Guid id,
        Guid tenantId,
        EmailAddress email,
        string fullName,
        PasswordHash passwordHash,
        UserRole role,
        DateTime nowUtc)
    {
        Id = id;
        TenantId = tenantId;
        Email = email;
        FullName = fullName;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.PendingVerification;
        TokenVersion = 1;
        CreatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Factory used by the registration use case. Creates a new user in
    /// the <see cref="UserStatus.PendingVerification"/> state.
    /// </summary>
    public static User Create(
        Guid tenantId,
        EmailAddress email,
        string fullName,
        PasswordHash passwordHash,
        UserRole role,
        DateTime nowUtc)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }

        return new User(Guid.NewGuid(), tenantId, email, fullName.Trim(), passwordHash, role, nowUtc);
    }

    public void MarkEmailVerified(DateTime nowUtc)
    {
        if (Status == UserStatus.Active && EmailVerifiedAtUtc.HasValue)
        {
            return;
        }

        EmailVerifiedAtUtc = nowUtc;
        Status = UserStatus.Active;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordSuccessfulLogin(DateTime nowUtc)
    {
        FailedLoginCount = 0;
        LockedUntilUtc = null;
        LastLoginAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void RecordFailedLogin(DateTime nowUtc, int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginCount++;
        LastFailedLoginAtUtc = nowUtc;
        if (FailedLoginCount >= maxAttempts)
        {
            Status = UserStatus.Locked;
            LockedUntilUtc = nowUtc.Add(lockoutDuration);
            FailedLoginCount = 0; // reset so the next unlock starts clean
        }
        UpdatedAtUtc = nowUtc;
    }

    public bool IsLockedAt(DateTime nowUtc) =>
        Status == UserStatus.Locked && LockedUntilUtc.HasValue && nowUtc < LockedUntilUtc.Value;

    public void Unlock(DateTime nowUtc)
    {
        if (Status == UserStatus.Locked)
        {
            Status = EmailVerifiedAtUtc.HasValue ? UserStatus.Active : UserStatus.PendingVerification;
        }
        LockedUntilUtc = null;
        FailedLoginCount = 0;
        UpdatedAtUtc = nowUtc;
    }

    public void Disable(DateTime nowUtc)
    {
        Status = UserStatus.Disabled;
        TokenVersion++;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangePassword(PasswordHash newHash, DateTime nowUtc)
    {
        PasswordHash = newHash;
        TokenVersion++;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangeRole(UserRole newRole, DateTime nowUtc)
    {
        if (newRole == Role)
        {
            return;
        }

        Role = newRole;
        TokenVersion++;
        UpdatedAtUtc = nowUtc;
    }

    public void EnableMfa(MfaFactor factor, DateTime nowUtc)
    {
        Mfa = factor ?? throw new ArgumentNullException(nameof(factor));
        UpdatedAtUtc = nowUtc;
    }

    public void DisableMfa(DateTime nowUtc)
    {
        Mfa = null;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Bump the token version (logout-all, role/permission change, etc.).</summary>
    public void RevokeAllTokens(DateTime nowUtc)
    {
        TokenVersion++;
        UpdatedAtUtc = nowUtc;
    }
}
