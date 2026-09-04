// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using FluentValidation;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Identity;

namespace SalekhPos.Application.Identity.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(value => EmailAddress.TryCreate(value, out _))
            .WithMessage("Email format is invalid.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.TenantName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.TenantSlug)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[a-z0-9][a-z0-9-]{1,62}[a-z0-9]$")
            .WithMessage("Tenant slug must be 2-64 chars: lowercase letters, digits, dashes.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must((cmd, plain) => PasswordPolicy.Validate(
                plain,
                personalFields: new[] { cmd.Email, cmd.FullName, cmd.TenantName, cmd.TenantSlug }).IsSuccess)
            .WithMessage("Password does not satisfy the security policy.");
    }
}
