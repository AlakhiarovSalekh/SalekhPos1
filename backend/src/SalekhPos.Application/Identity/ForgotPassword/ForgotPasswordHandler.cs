// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Application.Identity.RequestPasswordReset;
using SalekhPos.Domain.Common;

namespace SalekhPos.Application.Identity.ForgotPassword;

/// <summary>
/// Alias of <see cref="RequestPasswordResetHandler"/> kept for clarity at
/// the API surface. Both names mean the same operation: ask the server
/// to send a password-reset email if the email exists.
/// </summary>
public sealed class ForgotPasswordHandler
{
    private readonly RequestPasswordResetHandler _inner;

    public ForgotPasswordHandler(RequestPasswordResetHandler inner)
    {
        _inner = inner;
    }

    public Task<Result<Unit>> HandleAsync(ForgotPasswordCommand command, CancellationToken cancellationToken) =>
        _inner.HandleAsync(new RequestPasswordResetCommand(command.Email), cancellationToken);
}
