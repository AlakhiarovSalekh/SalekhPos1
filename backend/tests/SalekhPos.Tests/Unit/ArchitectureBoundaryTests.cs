// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

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
    public void ApplicationAssemblyShouldReferenceDomain()
    {
        var referenced = AssemblyMarker.Assembly.GetReferencedAssemblies();
        referenced.Should().Contain(a => a.Name == "SalekhPos.Domain");
    }

    [Fact]
    public void ApplicationAssemblyShouldNotReferenceInfrastructure()
    {
        // Constitution rule: Application depends on Domain and abstractions only.
        // It must not reach into Infrastructure.
        var referenced = AssemblyMarker.Assembly.GetReferencedAssemblies();
        referenced.Should().NotContain(a => a.Name == "SalekhPos.Infrastructure");
    }
}
