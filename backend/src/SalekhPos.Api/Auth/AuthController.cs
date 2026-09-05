// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalekhPos.Api.Auth.Dtos;
using SalekhPos.Application.Common;
using SalekhPos.Application.Identity;
using SalekhPos.Application.Identity.ChangePassword;
using SalekhPos.Application.Identity.DisableMfa;
using SalekhPos.Application.Identity.EnableMfa;
using SalekhPos.Application.Identity.ForgotPassword;
using SalekhPos.Application.Identity.Login;
using SalekhPos.Application.Identity.Logout;
using SalekhPos.Application.Identity.Refresh;
using SalekhPos.Application.Identity.Register;
using SalekhPos.Application.Identity.RequestPasswordReset;
using SalekhPos.Application.Identity.ResetPassword;
using SalekhPos.Application.Identity.VerifyEmail;
using SalekhPos.Application.Identity.VerifyMfa;

namespace SalekhPos.Api.Auth;

/// <summary>
/// Phase 2 authentication endpoints. The controller stays thin: it
/// validates request shape, calls the appropriate use-case handler,
/// and translates the result to a small DTO. Business rules and
/// security decisions live in the Application and Domain layers.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        [FromServices] IValidator<RegisterCommand> validator,
        [FromServices] RegisterHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FullName, request.TenantName, request.TenantSlug);
        var validation = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) });
        }

        var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return result.Error!.Field switch
            {
                nameof(RegisterCommand.Email) => Conflict(new { code = result.Error.Code, message = result.Error.Message }),
                nameof(RegisterCommand.TenantSlug) => Conflict(new { code = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { code = result.Error.Code, message = result.Error.Message }),
            };
        }

        var value = result.Value!;
        return Accepted(new RegisterResponse(value.UserId, value.TenantId, value.TenantSlug));
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        [FromServices] VerifyEmailHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new VerifyEmailCommand(request.Token), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken).ConfigureAwait(false);
        // The use case throws InvalidCredentials/AccountLocked/EmailNotVerified; the
        // ProblemDetails middleware in Program.cs maps those to 401/423/403. If we
        // reach here the outcome is a Result.
        if (!result.IsSuccess)
        {
            return BadRequest(new { code = result.Error!.Code, message = result.Error.Message });
        }

        return result.Value switch
        {
            LoginOutcome.Authenticated auth => Ok(new LoginResponse(
                Status: "authenticated",
                Tokens: new TokenResponse(
                    auth.Tokens.AccessToken,
                    auth.Tokens.RefreshTokenRaw,
                    auth.Tokens.AccessTokenExpiresAtUtc,
                    auth.Tokens.RefreshTokenExpiresAtUtc))),
            LoginOutcome.MfaRequired mfa => Ok(new LoginResponse(
                Status: "mfa_required",
                Mfa: new MfaChallenge(mfa.UserId, mfa.TenantId))),
            _ => throw new InvalidOperationException("Unknown login outcome."),
        };
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<ActionResult<TokenResponse>> Refresh(
        [FromBody] RefreshRequest request,
        [FromServices] RefreshHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RefreshCommand(request.RefreshToken), cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return BadRequest(new { code = result.Error!.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new TokenResponse(
            value.AccessToken,
            value.RefreshTokenRaw,
            value.AccessTokenExpiresAtUtc,
            value.RefreshTokenExpiresAtUtc));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromServices] LogoutHandler handler,
        CancellationToken cancellationToken)
    {
        // The user id is derived from the validated JWT; the ProblemDetails
        // middleware returns 401 if no token was supplied. The Authorization
        // header is parsed by the JwtBearer middleware in Program.cs.
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await handler.HandleAsync(new LogoutCommand(userId), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] ForgotPasswordHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ForgotPasswordCommand(request.Email), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] IValidator<ResetPasswordCommand> validator,
        [FromServices] ResetPasswordHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Token, request.NewPassword);
        var validation = await validator.ValidateAsync(command, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return BadRequest(new { errors = validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) });
        }

        await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        [FromServices] ChangePasswordHandler handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await handler.HandleAsync(new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("mfa/setup")]
    [Authorize]
    [ProducesResponseType(typeof(MfaSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MfaSetupResponse>> MfaSetup(
        [FromServices] EnableMfaHandler handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await handler.HandleAsync(new EnableMfaCommand(userId), cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return Conflict(new { code = result.Error!.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new MfaSetupResponse(value.ProvisioningUri, value.RecoveryCodes));
    }

    [HttpPost("mfa/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> MfaVerify(
        [FromBody] MfaVerifyRequest request,
        [FromServices] VerifyMfaHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new VerifyMfaCommand(request.UserId, request.Code), cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return BadRequest(new { code = result.Error!.Code, message = result.Error.Message });
        }

        var value = result.Value!;
        return Ok(new TokenResponse(
            value.AccessToken,
            value.RefreshTokenRaw,
            value.AccessTokenExpiresAtUtc,
            value.RefreshTokenExpiresAtUtc));
    }

    [HttpPost("mfa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MfaDisable(
        [FromServices] DisableMfaHandler handler,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        await handler.HandleAsync(new DisableMfaCommand(userId), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
