// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Identity.Register;

/// <summary>
/// Self-service registration. Creates a tenant and the first user
/// (TenantOwner). The user starts in <c>PendingVerification</c> state and
/// must verify the email before login succeeds.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string TenantName,
    string TenantSlug);
