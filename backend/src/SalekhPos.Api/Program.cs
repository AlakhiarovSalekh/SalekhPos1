// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Hellang.Middleware.ProblemDetails;
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
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.ShouldLogUnhandledException = (ctx, _, _) => true;
});

// --- Health checks ----------------------------------------------------------
// /health/live: process is up.
// /health/ready: ready to receive normal traffic (deps later).
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("SalekhPos.Api process is up."), tags: new[] { "live" });

// --- OpenAPI ----------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

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
app.MapOpenApi();

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
