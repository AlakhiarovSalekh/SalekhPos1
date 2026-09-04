// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Abstractions.Persistence;
using SalekhPos.Application.Common;
using SalekhPos.Domain.Common;
using SalekhPos.Domain.Identity;
using SalekhPos.Domain.Tenants;

namespace SalekhPos.Application.Identity.EnableMfa;

public sealed class EnableMfaHandler
{
    private readonly IClock _clock;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IMfaFactorRepository _factors;
    private readonly IMfaRecoveryCodeRepository _recoveryCodes;
    private readonly ITotpService _totp;
    private readonly IAesGcmCipher _cipher;
    private readonly IEmailSender _email;

    public EnableMfaHandler(
        IClock clock,
        IUserRepository users,
        ITenantRepository tenants,
        IMfaFactorRepository factors,
        IMfaRecoveryCodeRepository recoveryCodes,
        ITotpService totp,
        IAesGcmCipher cipher,
        IEmailSender email)
    {
        _clock = clock;
        _users = users;
        _tenants = tenants;
        _factors = factors;
        _recoveryCodes = recoveryCodes;
        _totp = totp;
        _cipher = cipher;
        _email = email;
    }

    public async Task<Result<MfaEnrollment>> HandleAsync(EnableMfaCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdWithMfaAsync(command.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("User not found.");

        if (user.Mfa is not null)
        {
            return Result<MfaEnrollment>.Failure("auth.mfa_already_enabled", "MFA is already enabled for this user.");
        }

        var nowUtc = _clock.UtcNow;
        var secret = _totp.GenerateSecret();
        var encrypted = _cipher.Encrypt(secret);

        // Persist the factor immediately so the secret survives even if
        // the user does not complete the verify step. Verification flips
        // the factor from "pending" to "active" via a follow-up step in
        // a later slice. For Phase 2 we mark enabled on creation.
        var factor = MfaFactor.Enable(user.Id, Serialize(encrypted), nowUtc);
        await _factors.AddAsync(factor, cancellationToken).ConfigureAwait(false);
        user.EnableMfa(factor, nowUtc);

        // Recovery codes: 10 one-time codes, hashed, never stored plaintext.
        var plainCodes = _totp.GenerateRecoveryCodes(10);
        var codes = plainCodes
            .Select(plain => MfaRecoveryCode.Issue(user.Id, HashedToken.From(plain), nowUtc))
            .ToList();
        await _recoveryCodes.AddManyAsync(codes, cancellationToken).ConfigureAwait(false);

        var tenant = await _tenants.FindByIdAsync(user.TenantId, cancellationToken).ConfigureAwait(false);
        await _email.SendMfaEnabledAsync(user.Email, user.FullName, tenant?.Name ?? string.Empty, cancellationToken).ConfigureAwait(false);

        var accountLabel = user.Email.Value;
        var provisioningUri = _totp.BuildProvisioningUri(secret, accountLabel, "SalekhPos");

        return Result<MfaEnrollment>.Success(new MfaEnrollment(provisioningUri, plainCodes));
    }

    private static byte[] Serialize(AesGcmResult cipher)
    {
        // Layout: [12 bytes nonce][N bytes ciphertext][16 bytes tag]
        var output = new byte[cipher.Nonce.Length + cipher.Ciphertext.Length + cipher.Tag.Length];
        Buffer.BlockCopy(cipher.Nonce, 0, output, 0, cipher.Nonce.Length);
        Buffer.BlockCopy(cipher.Ciphertext, 0, output, cipher.Nonce.Length, cipher.Ciphertext.Length);
        Buffer.BlockCopy(cipher.Tag, 0, output, cipher.Nonce.Length + cipher.Ciphertext.Length, cipher.Tag.Length);
        return output;
    }
}

public sealed record MfaEnrollment(string ProvisioningUri, IReadOnlyList<string> RecoveryCodes);
