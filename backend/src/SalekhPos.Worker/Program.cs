// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application;
using SalekhPos.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Application", "SalekhPos.Worker"));

// Phase 1: composition root. Concrete background services
// (outbox dispatcher, sync reconciliation, report generator, fiscal/pollers)
// are added in their respective phases.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
