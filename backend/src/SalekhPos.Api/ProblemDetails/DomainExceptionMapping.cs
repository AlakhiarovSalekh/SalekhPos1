// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using SalekhPos.Domain.Errors;

namespace SalekhPos.Api.ProblemDetails;

/// <summary>
/// Maps Phase 2 domain exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public static class DomainExceptionMapping
{
    public static void Configure(Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options)
    {
        options.Map<InvalidCredentialsException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed",
            Detail = "Invalid credentials.",
            Type = "https://salekhpos.com/errors/auth.invalid_credentials",
        });

        options.Map<AccountLockedException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status423Locked,
            Title = "Account locked",
            Detail = "Account is temporarily locked.",
            Type = "https://salekhpos.com/errors/auth.account_locked",
            Extensions = { ["lockedUntilUtc"] = ex.LockedUntilUtc },
        });

        options.Map<EmailNotVerifiedException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Email not verified",
            Detail = "Email address has not been verified.",
            Type = "https://salekhpos.com/errors/auth.email_not_verified",
        });

        options.Map<PasswordPolicyException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Password does not satisfy the security policy",
            Detail = ex.Message,
            Type = "https://salekhpos.com/errors/auth.password_policy",
            Extensions = { ["reasons"] = ex.Reasons },
        });

        options.Map<RateLimitedException>(ex =>
        {
            var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "You have made too many requests. Please try again later.",
                Type = "https://salekhpos.com/errors/auth.rate_limited",
            };
            pd.Extensions["retryAfterSeconds"] = (int)ex.RetryAfter.TotalSeconds;
            return pd;
        });

        options.Map<InvalidTokenException>(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = ex.IsReplay ? StatusCodes.Status410Gone : StatusCodes.Status400BadRequest,
            Title = ex.IsReplay ? "Token replay detected" : "Token is invalid",
            Detail = ex.Message,
            Type = ex.IsReplay
                ? "https://salekhpos.com/errors/auth.token_replay"
                : "https://salekhpos.com/errors/auth.token_invalid",
        });
    }
}
