using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Identity.Application.Auth;

public sealed class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret) || options.Secret.Length < 32)
        {
            return ValidateOptionsResult.Fail("JWT secret must be at least 32 characters.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("JWT issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("JWT audience is required.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("JWT expiration must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
