// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SalekhPos.Api.Controllers;

/// <summary>
/// System-level endpoints (version, health, status). The only controller
/// present in the Phase 0/1 scaffold. Business endpoints are added in
/// later phases.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/v1/system")]
[Produces("application/json")]
public sealed class SystemController : ControllerBase
{
    /// <summary>
    /// Returns the API version, environment, and the current server time
    /// in UTC. Useful for sanity checks and tooling.
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(SystemInfoResponse), StatusCodes.Status200OK)]
    public ActionResult<SystemInfoResponse> GetInfo()
    {
        var response = new SystemInfoResponse(
            Version: "0.0.0",
            Environment: HttpContext.RequestServices
                .GetService<IHostEnvironment>()?.EnvironmentName ?? "Unknown",
            ServerTimeUtc: DateTime.UtcNow);

        return Ok(response);
    }
}

/// <summary>
/// Information about the running API instance.
/// </summary>
public sealed record SystemInfoResponse(string Version, string Environment, DateTime ServerTimeUtc);
