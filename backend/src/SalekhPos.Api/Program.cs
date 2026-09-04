// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SalekhPos.Api.Diagnostics;
using SalekhPos.Application;
using SalekhPos.Infrastructure;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (Serilog) ------------------------------------------------------
// Structured logging with correlation IDs. Secrets are never logged.
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "SalekhPos.Api"));

// --- Forwarded headers / proxy ---------------------------------------------
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust the loopback by default; configure known proxies in production via env.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- Application / Infrastructure composition root --------------------------
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// --- MVC / Problem Details --------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails(options =>
{
    // Hellang 6.5.1: both IncludeExceptionDetails and ShouldLogUnhandledException
    // are predicates (Func<HttpContext, Exception, bool>), not booleans. We
    // expose stack traces only in Development and always log unhandled
    // exceptions so they show up in the structured logs.
    options.IncludeExceptionDetails = (_, _) => builder.Environment.IsDevelopment();
    options.ShouldLogUnhandledException = (_, _, _) => true;
});

// --- Health checks ----------------------------------------------------------
// /health/live: process is up.
// /health/ready: ready to receive normal traffic (deps later).
// Note: CA1861 prefers a static readonly array; the live tags array is a
// single allocation at startup, so we inline here and suppress the rule.
#pragma warning disable CA1861
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("SalekhPos.Api process is up."), tags: new[] { "live" });
#pragma warning restore CA1861

// --- OpenAPI ----------------------------------------------------------------
// .NET 8 does not ship built-in AddOpenApi/MapOpenApi; we use
// AddEndpointsApiExplorer so a future Swashbuckle or NSwag registration
// picks up the endpoint metadata. The actual UI/JSON endpoint is added
// in the OpenAPI phase.
builder.Services.AddEndpointsApiExplorer();

// --- Build & pipeline -------------------------------------------------------
var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    };
});

app.UseForwardedHeaders();
app.UseMiddleware<RequestIdMiddleware>();
app.UseProblemDetails();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// MapOpenApi is .NET 9+; the OpenAPI UI endpoint is added in the
// OpenAPI phase (Swashbuckle/NSwag).

// --- Health endpoints -------------------------------------------------------
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true, // readiness reports all checks; PostgreSQL/Redis checks added when they are introduced.
    AllowCachingResponses = false
}).AllowAnonymous();

app.Run();

/// <summary>
/// Exposed for WebApplicationFactory&lt;Program&gt; integration tests.
/// </summary>
public partial class Program
{
}
