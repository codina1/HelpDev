using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.Extensions.Options;

namespace HelpDev.Identity.Tests;

public sealed class OtpSettingsValidatorTests
{
    private readonly OtpSettingsValidator _validator = new();

    [Fact]
    public void Validate_succeeds_for_positive_max_failed_attempts()
    {
        var settings = new OtpSettings { MaxFailedAttempts = 5 };

        var result = _validator.Validate(null, settings);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_fails_when_max_failed_attempts_is_not_positive(int maxFailedAttempts)
    {
        var settings = new OtpSettings { MaxFailedAttempts = maxFailedAttempts };

        var result = _validator.Validate(null, settings);

        Assert.True(result.Failed);
        Assert.Contains("OTP max failed attempts must be greater than zero.", result.Failures!);
    }

    [Fact]
    public void Missing_configuration_uses_default_max_failed_attempts_of_five()
    {
        var settings = new OtpSettings();

        Assert.Equal(5, settings.MaxFailedAttempts);
        Assert.True(_validator.Validate(null, settings).Succeeded);
    }
}
