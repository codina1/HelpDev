using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.Extensions.Options;

namespace HelpDev.Identity.Tests;

public sealed class JwtSettingsValidatorTests
{
    private readonly JwtSettingsValidator _validator = new();

    [Fact]
    public void Validate_succeeds_for_valid_settings()
    {
        var settings = CreateValidSettings();

        var result = _validator.Validate(null, settings);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short-secret")]
    public void Validate_fails_when_secret_is_missing_or_too_short(string? secret)
    {
        var settings = CreateValidSettings();
        settings.Secret = secret!;

        var result = _validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.Contains("JWT secret must be at least 32 characters.", result.Failures!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_fails_when_issuer_is_missing(string? issuer)
    {
        var settings = CreateValidSettings();
        settings.Issuer = issuer!;

        var result = _validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.Contains("JWT issuer is required.", result.Failures!);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_fails_when_audience_is_missing(string? audience)
    {
        var settings = CreateValidSettings();
        settings.Audience = audience!;

        var result = _validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.Contains("JWT audience is required.", result.Failures!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_fails_when_expiration_is_not_positive(int expirationMinutes)
    {
        var settings = CreateValidSettings();
        settings.ExpirationMinutes = expirationMinutes;

        var result = _validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.Contains("JWT expiration must be greater than zero.", result.Failures!);
    }

    private static JwtSettings CreateValidSettings() =>
        new()
        {
            Secret = "01234567890123456789012345678901",
            Issuer = "HelpDev",
            Audience = "HelpDev.Client",
            ExpirationMinutes = 60,
        };
}
