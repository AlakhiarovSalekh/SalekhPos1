// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Api.Auth.Dtos;

/// <summary>Request body for /auth/register.</summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string TenantName,
    string TenantSlug);

/// <summary>Response for /auth/register. 202 Accepted — the user must verify their email.</summary>
public sealed record RegisterResponse(Guid UserId, Guid TenantId, string TenantSlug);

/// <summary>Request body for /auth/verify-email.</summary>
public sealed record VerifyEmailRequest(string Token);

/// <summary>Request body for /auth/login.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Response for /auth/login. Either a token pair (success) or an MFA challenge.</summary>
public sealed record LoginResponse(
    string Status,
    TokenResponse? Tokens = null,
    MfaChallenge? Mfa = null);

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record MfaChallenge(Guid UserId, Guid TenantId);

/// <summary>Request body for /auth/refresh.</summary>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>Request body for /auth/forgot-password.</summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Request body for /auth/reset-password.</summary>
public sealed record ResetPasswordRequest(string Token, string NewPassword);

/// <summary>Request body for /auth/change-password (authenticated).</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>Response for /auth/mfa/setup. The client renders the QR code from ProvisioningUri and stores the recovery codes locally.</summary>
public sealed record MfaSetupResponse(string ProvisioningUri, IReadOnlyList<string> RecoveryCodes);

/// <summary>Request body for /auth/mfa/verify.</summary>
public sealed record MfaVerifyRequest(Guid UserId, string Code);
