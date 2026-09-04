// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Domain.Identity;

/// <summary>
/// Lifecycle state of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>Created but the email has not been verified yet.</summary>
    PendingVerification = 0,

    /// <summary>Verified and able to authenticate.</summary>
    Active = 1,

    /// <summary>Temporarily locked due to too many failed login attempts.</summary>
    Locked = 2,

    /// <summary>Disabled by an administrator. Cannot authenticate.</summary>
    Disabled = 3,
}
