// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// The current authenticated user, derived from the validated JWT
/// claims. The Infrastructure layer implements this over
/// <c>IHttpContextAccessor</c>.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid? TenantId { get; }

    string? Role { get; }

    int? TokenVersion { get; }

    bool IsAuthenticated { get; }
}
