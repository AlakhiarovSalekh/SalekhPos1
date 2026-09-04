// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when a request would exceed a rate limit. Surfaces as
/// ProblemDetails 429 with a <c>Retry-After</c> header.
/// </summary>
public sealed class RateLimitedException : DomainException
{
    public TimeSpan RetryAfter { get; }

    public RateLimitedException(TimeSpan retryAfter)
        : base("Too many requests.")
    {
        RetryAfter = retryAfter;
    }
}
