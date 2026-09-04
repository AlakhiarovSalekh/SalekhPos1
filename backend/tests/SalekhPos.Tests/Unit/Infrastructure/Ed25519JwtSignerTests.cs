// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

#pragma warning disable CA1861

using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SalekhPos.Application.Abstractions;
using SalekhPos.Application.Identity;
using SalekhPos.Domain.Errors;
using SalekhPos.Infrastructure.Security;
using Xunit;

namespace SalekhPos.Tests.Unit.Infrastructure;

public sealed class Ed25519JwtSignerTests : IDisposable
{
    private readonly string _keyPath;

    public Ed25519JwtSignerTests()
    {
        // Each test instance gets its own 32-byte Ed25519 seed in a
        // temp file. The signer reads from disk at construction time.
        var seed = new byte[32];
        RandomNumberGenerator.Fill(seed);
        _keyPath = Path.Combine(Path.GetTempPath(), $"salekhpos-test-key-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(_keyPath, seed);
    }

    public void Dispose()
    {
        try { File.Delete(_keyPath); } catch { /* best-effort cleanup */ }
    }

    [Fact]
    public void SignedTokenValidatesWithMatchingClaims()
    {
        var options = Options.Create(new JwtIssuerOptions
        {
            Issuer = "salekhpos.api.test",
            Audiences = new[] { "salekhpos.web.test", "salekhpos.desktop.test" },
        });
        var keyOptions = Options.Create(new JwtSigningKeyOptions { PrivateKeyPath = _keyPath });
        using var signer = new Ed25519JwtSigner(options, keyOptions);

        var now = DateTime.UtcNow;
        var claims = new JwtAccessTokenClaims(
            Subject: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Role: "TenantOwner",
            TokenVersion: 1,
            Audiences: new[] { "salekhpos.web.test", "salekhpos.desktop.test" },
            Jti: Guid.NewGuid(),
            IssuedAtUtc: now,
            ExpiresAtUtc: now.AddMinutes(15));

        var token = signer.Sign(claims);
        token.Should().NotBeNullOrEmpty();

        var validated = signer.Validate(token);
        validated.Subject.Should().Be(claims.Subject);
        validated.TenantId.Should().Be(claims.TenantId);
        validated.Role.Should().Be(claims.Role);
        validated.TokenVersion.Should().Be(claims.TokenVersion);
        validated.Jti.Should().Be(claims.Jti);
        validated.ExpiresAtUtc.Should().BeCloseTo(claims.ExpiresAtUtc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TamperedTokenFailsValidation()
    {
        var options = Options.Create(new JwtIssuerOptions
        {
            Issuer = "salekhpos.api.test",
            Audiences = new[] { "salekhpos.web.test" },
        });
        var keyOptions = Options.Create(new JwtSigningKeyOptions { PrivateKeyPath = _keyPath });
        using var signer = new Ed25519JwtSigner(options, keyOptions);

        var now = DateTime.UtcNow;
        var claims = new JwtAccessTokenClaims(
            Subject: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Role: "TenantOwner",
            TokenVersion: 1,
            Audiences: new[] { "salekhpos.web.test" },
            Jti: Guid.NewGuid(),
            IssuedAtUtc: now,
            ExpiresAtUtc: now.AddMinutes(15));

        var token = signer.Sign(claims);

        // Flip one character of the signature.
        var parts = token.Split('.');
        var sig = parts[2].ToCharArray();
        sig[0] = sig[0] == 'A' ? 'B' : 'A';
        var tampered = $"{parts[0]}.{parts[1]}.{new string(sig)}";

        var act = () => signer.Validate(tampered);
        act.Should().Throw<InvalidTokenException>();
    }

    [Fact]
    public void TokenWithWrongAudienceFailsValidation()
    {
        var options = Options.Create(new JwtIssuerOptions
        {
            Issuer = "salekhpos.api.test",
            Audiences = new[] { "salekhpos.web.test" },
        });
        var keyOptions = Options.Create(new JwtSigningKeyOptions { PrivateKeyPath = _keyPath });
        using var signer = new Ed25519JwtSigner(options, keyOptions);

        var now = DateTime.UtcNow;
        var claims = new JwtAccessTokenClaims(
            Subject: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Role: "TenantOwner",
            TokenVersion: 1,
            Audiences: new[] { "some-other-audience" }, // not in options
            Jti: Guid.NewGuid(),
            IssuedAtUtc: now,
            ExpiresAtUtc: now.AddMinutes(15));

        var token = signer.Sign(claims);
        var act = () => signer.Validate(token);
        act.Should().Throw<InvalidTokenException>();
    }

    [Fact]
    public void ExpiredTokenFailsValidation()
    {
        var options = Options.Create(new JwtIssuerOptions
        {
            Issuer = "salekhpos.api.test",
            Audiences = new[] { "salekhpos.web.test" },
        });
        var keyOptions = Options.Create(new JwtSigningKeyOptions { PrivateKeyPath = _keyPath });
        using var signer = new Ed25519JwtSigner(options, keyOptions);

        var past = DateTime.UtcNow.AddHours(-1);
        var claims = new JwtAccessTokenClaims(
            Subject: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            Role: "TenantOwner",
            TokenVersion: 1,
            Audiences: new[] { "salekhpos.web.test" },
            Jti: Guid.NewGuid(),
            IssuedAtUtc: past,
            ExpiresAtUtc: past.AddMinutes(5));

        var token = signer.Sign(claims);
        var act = () => signer.Validate(token);
        act.Should().Throw<InvalidTokenException>();
    }
}
