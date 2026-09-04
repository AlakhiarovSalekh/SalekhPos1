// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

// CA1848 prefers LoggerMessage source-generated delegates. The
// LoggingEmailSender is a development-only path with very low call
// volume; the source-gen pattern adds noise here for no real win. We
// suppress the rule for the file.
#pragma warning disable CA1848

using Microsoft.Extensions.Logging;
using SalekhPos.Application.Abstractions;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Infrastructure.Security;

/// <summary>
/// Default IEmailSender for development: writes a structured log line
/// per email at Information level. The Production SMTP/SES adapter
/// lands in Phase 16 (Integrations). Never use this in production
/// (it leaks the raw token in logs).
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(EmailAddress recipient, string fullName, string rawToken, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Verification to {Recipient} for {FullName} at {Tenant}. Token: {Token}",
            recipient.Value, fullName, tenantName, rawToken);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(EmailAddress recipient, string fullName, string rawToken, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] Password reset to {Recipient} for {FullName} at {Tenant}. Token: {Token}",
            recipient.Value, fullName, tenantName, rawToken);
        return Task.CompletedTask;
    }

    public Task SendMfaEnabledAsync(EmailAddress recipient, string fullName, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] MFA enabled for {Recipient} ({FullName}) at {Tenant}.",
            recipient.Value, fullName, tenantName);
        return Task.CompletedTask;
    }

    public Task SendMfaDisabledAsync(EmailAddress recipient, string fullName, string tenantName, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[DEV EMAIL] MFA disabled for {Recipient} ({FullName}) at {Tenant}.",
            recipient.Value, fullName, tenantName);
        return Task.CompletedTask;
    }
}
