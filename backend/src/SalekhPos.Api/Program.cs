// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SalekhPos.Api.Authentication;
using SalekhPos.Api.Diagnostics;
using SalekhPos.Api.ProblemDetails;
using SalekhPos.Application;
using SalekhPos.Infrastructure;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// --- Logging (Serilog) ------------------------------------------------------
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
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- Application / Infrastructure composition root --------------------------
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// --- Authentication --------------------------------------------------------
// Custom EdDSA scheme (we sign with Ed25519 via BouncyCastle; the
// built-in JwtBearer handler does not support EdDSA in .NET 8).
builder.Services
    .AddAuthentication(EdDsaAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, EdDsaAuthenticationHandler>(
        EdDsaAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();

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
    options.IncludeExceptionDetails = (_, _) => builder.Environment.IsDevelopment();
    options.ShouldLogUnhandledException = (_, _, _) => true;
    DomainExceptionMapping.Configure(options);
});

// --- Health checks ----------------------------------------------------------
#pragma warning disable CA1861
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("SalekhPos.Api process is up."), tags: new[] { "live" });
#pragma warning restore CA1861

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

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    AllowCachingResponses = false,
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    AllowCachingResponses = false,
}).AllowAnonymous();

app.Run();

/// <summary>
/// Exposed for WebApplicationFactory&lt;Program&gt; integration tests.
/// </summary>
public partial class Program
{
}
