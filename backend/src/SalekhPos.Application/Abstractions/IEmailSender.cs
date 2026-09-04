// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Abstractions;

/// <summary>
/// Sends transactional email. The Phase 2 default is a logging sender;
/// the production SMTP/SES provider lands in Phase 16 (Integrations).
/// </summary>
public interface IEmailSender
{
    Task SendEmailVerificationAsync(EmailAddress recipient, string fullName, string rawToken, string tenantName, CancellationToken cancellationToken);

    Task SendPasswordResetAsync(EmailAddress recipient, string fullName, string rawToken, string tenantName, CancellationToken cancellationToken);

    Task SendMfaEnabledAsync(EmailAddress recipient, string fullName, string tenantName, CancellationToken cancellationToken);

    Task SendMfaDisabledAsync(EmailAddress recipient, string fullName, string tenantName, CancellationToken cancellationToken);
}
