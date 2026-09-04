// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace SalekhPos.Api.Diagnostics;

/// <summary>
/// Middleware that ensures every request has a stable correlation/request ID,
/// surfaced as <c>X-Request-Id</c> (response) and as a Serilog log-context
/// property. The ID propagates into background work and is included in audit
/// records.
/// </summary>
public sealed class RequestIdMiddleware
{
    private const string HeaderName = "X-Request-Id";
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Headers.TryGetValue(HeaderName, out StringValues incoming) ||
            string.IsNullOrWhiteSpace(incoming))
        {
            incoming = Guid.NewGuid().ToString("N");
        }

        var requestId = incoming.ToString();
        context.Request.Headers[HeaderName] = requestId;
        context.Response.Headers[HeaderName] = requestId;
        context.TraceIdentifier = requestId;

        using (LogContext.PushProperty("RequestId", requestId))
        {
            await _next(context).ConfigureAwait(false);
        }
    }
}
