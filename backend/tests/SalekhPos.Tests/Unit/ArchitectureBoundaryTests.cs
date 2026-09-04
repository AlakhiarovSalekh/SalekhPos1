// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using SalekhPos.Application;
using Xunit;

namespace SalekhPos.Tests.Unit;

/// <summary>
/// Foundation-level tests that verify the architecture invariants defined
/// in the constitution. These are intentionally minimal in the scaffold;
/// they grow with the project (tenant isolation, BOLA, idempotency, etc.).
/// </summary>
public sealed class ArchitectureBoundaryTests
{
    [Fact]
    public void ApplicationAssemblyShouldExposeMarker()
    {
        AssemblyMarker.Assembly.GetName().Name.Should().Be("SalekhPos.Application");
    }

    [Fact]
    public void ApplicationDependenciesShouldIncludeDomain()
    {
        // Constitution rule: Application -> Domain. The ProjectReference
        // is recorded in the assembly's .deps.json sidecar, which is the
        // authoritative runtime reference graph for .NET assemblies.
        var dependencies = ReadRuntimeDependencies(AssemblyMarker.Assembly);
        dependencies.Should().Contain(name => name == "SalekhPos.Domain");
    }

    [Fact]
    public void ApplicationDependenciesShouldNotIncludeInfrastructure()
    {
        // Constitution rule: Application depends on Domain and abstractions
        // only. It must not reach into Infrastructure.
        var dependencies = ReadRuntimeDependencies(AssemblyMarker.Assembly);
        dependencies.Should().NotContain(name => name == "SalekhPos.Infrastructure");
    }

    [Fact]
    public void ApplicationDependenciesShouldNotIncludeApi()
    {
        // Api -> Application is the dependency direction. Application
        // must not reference Api (no upward reach into the host project).
        var dependencies = ReadRuntimeDependencies(AssemblyMarker.Assembly);
        dependencies.Should().NotContain(name => name == "SalekhPos.Api");
    }

    private static IReadOnlyCollection<string> ReadRuntimeDependencies(Assembly assembly)
    {
        // Read SalekhPos.Application.deps.json. The 'targets' object lists
        // the application's project and package dependencies as recorded
        // at build time. This is more reliable than inspecting
        // Assembly.GetReferencedAssemblies(), which only returns
        // already-loaded refs in the test process.
        var location = assembly.Location;
        var depsPath = location.Replace(".dll", ".deps.json");
        if (!File.Exists(depsPath))
        {
            return Array.Empty<string>();
        }

        using var stream = File.OpenRead(depsPath);
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;

        var names = new List<string>();
        if (!root.TryGetProperty("targets", out var targets))
        {
            return names;
        }

        // Find the first target moniker (e.g. ".NETCoreApp,Version=v8.0")
        // and the first project entry under it (SalekhPos.Application/1.0.0).
        foreach (var target in targets.EnumerateObject())
        {
            if (!target.Value.TryGetProperty("SalekhPos.Application/1.0.0", out var project))
            {
                continue;
            }

            if (!project.TryGetProperty("dependencies", out var deps))
            {
                return names;
            }

            foreach (var dep in deps.EnumerateObject())
            {
                names.Add(dep.Name);
            }
            break;
        }

        return names;
    }
}
