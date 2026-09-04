// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Domain.Common;

namespace SalekhPos.Application.Common;

/// <summary>
/// Password policy. Enforced at register, change, and reset. Mirrors the
/// Security doc:
///   - length 12..128
///   - at least one lower, one upper, one digit, one symbol
///   - no whitespace
///   - not in the bundled top-1k common-password list
///   - does not contain obvious personal fields (email local part, full name parts)
/// Returns <see cref="Result{T}"/> of the normalised password (we keep the
/// original casing; trimming/normalisation is the caller's job).
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 12;
    public const int MaxLength = 128;

    public static Result<string> Validate(
        string plaintext,
        IReadOnlyCollection<string>? personalFields = null)
    {
        var reasons = new List<string>();

        if (string.IsNullOrEmpty(plaintext))
        {
            reasons.Add("password.required");
            return Result<string>.Failure("password.policy", string.Join("; ", reasons));
        }

        if (plaintext.Length < MinLength)
        {
            reasons.Add($"password.too_short:{MinLength}");
        }

        if (plaintext.Length > MaxLength)
        {
            reasons.Add($"password.too_long:{MaxLength}");
        }

        if (plaintext.Any(char.IsWhiteSpace))
        {
            reasons.Add("password.whitespace");
        }

        var hasLower = false;
        var hasUpper = false;
        var hasDigit = false;
        var hasSymbol = false;
        foreach (var ch in plaintext)
        {
            if (char.IsLower(ch))
            {
                hasLower = true;
            }
            else if (char.IsUpper(ch))
            {
                hasUpper = true;
            }
            else if (char.IsDigit(ch))
            {
                hasDigit = true;
            }
            else if (!char.IsWhiteSpace(ch))
            {
                hasSymbol = true;
            }
        }

        if (!hasLower)
        {
            reasons.Add("password.missing_lower");
        }

        if (!hasUpper)
        {
            reasons.Add("password.missing_upper");
        }

        if (!hasDigit)
        {
            reasons.Add("password.missing_digit");
        }

        if (!hasSymbol)
        {
            reasons.Add("password.missing_symbol");
        }

        if (CommonPasswordList.IsCommon(plaintext))
        {
            reasons.Add("password.common");
        }

        if (personalFields is not null)
        {
            foreach (var field in personalFields)
            {
                if (string.IsNullOrWhiteSpace(field))
                {
                    continue;
                }

                if (field.Length >= 4 &&
                    plaintext.Contains(field, StringComparison.OrdinalIgnoreCase))
                {
                    reasons.Add("password.contains_personal");
                    break;
                }
            }
        }

        if (reasons.Count > 0)
        {
            return Result<string>.Failure("password.policy", string.Join("; ", reasons));
        }

        return Result<string>.Success(plaintext);
    }
}
