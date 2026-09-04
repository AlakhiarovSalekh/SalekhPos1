// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SalekhPos.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for `dotnet ef` migrations. Reads the connection
/// string from the environment variable <c>SALEKHPOS_CONNECTION</c> so
/// `dotnet ef migrations add ...` works without booting the API.
/// </summary>
public sealed class SalekhPosDbContextFactory : IDesignTimeDbContextFactory<SalekhPosDbContext>
{
    public SalekhPosDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("SALEKHPOS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=salekhpos;Username=salekhpos;Password=salekhpos";

        var options = new DbContextOptionsBuilder<SalekhPosDbContext>()
            .UseNpgsql(connection, npg => npg.MigrationsAssembly(typeof(SalekhPosDbContextFactory).Assembly.GetName().Name))
            .Options;

        return new SalekhPosDbContext(options);
    }
}
