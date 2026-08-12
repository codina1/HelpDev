using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Identity.Application.Auth;

public sealed class OtpSettingsValidator : IValidateOptions<OtpSettings>
{
    public ValidateOptionsResult Validate(string? name, OtpSettings options)
    {
        if (options.MaxFailedAttempts <= 0)
        {
            return ValidateOptionsResult.Fail("OTP max failed attempts must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
