// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentValidation;
using SalekhPos.Application.Common;

namespace SalekhPos.Application.Identity.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.RawToken).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Must(plain => PasswordPolicy.Validate(plain, personalFields: null).IsSuccess)
            .WithMessage("New password does not satisfy the security policy.");
    }
}
