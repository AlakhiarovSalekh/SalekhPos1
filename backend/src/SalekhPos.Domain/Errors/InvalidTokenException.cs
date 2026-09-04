// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Domain.Errors;

/// <summary>
/// Thrown when an opaque token is unknown, expired, or already used.
/// Surfaces as ProblemDetails 410 (Gone) for replay and 400 (Bad
/// Request) for malformed tokens.
/// </summary>
public sealed class InvalidTokenException : DomainException
{
    public enum TokenKind
    {
        EmailVerification,
        PasswordReset,
        RefreshToken,
        MfaCode,
        MfaRecoveryCode,
    }

    public TokenKind Kind { get; }

    public bool IsReplay { get; }

    public InvalidTokenException(TokenKind kind, bool isReplay, string message)
        : base(message)
    {
        Kind = kind;
        IsReplay = isReplay;
    }
}
