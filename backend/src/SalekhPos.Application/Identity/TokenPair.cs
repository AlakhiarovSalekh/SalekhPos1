// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Identity;

/// <summary>
/// The result of a successful authentication: a freshly-issued access
/// token and the raw refresh token (the caller embeds the raw refresh
/// token in the response; only its hash is stored).
/// </summary>
public sealed record TokenPair(
    string AccessToken,
    string RefreshTokenRaw,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
