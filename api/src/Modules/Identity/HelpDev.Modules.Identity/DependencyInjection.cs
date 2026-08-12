using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Identity.Application.Profiles;
using HelpDev.Modules.Identity.Infrastructure.Auth;
using HelpDev.Modules.Identity.Infrastructure.Persistence;
using HelpDev.Modules.Identity.Infrastructure.Security;
using HelpDev.SharedApplication.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Identity;

/// <summary>
/// DI entry point for the Identity module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateOnStart();
        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));

        services.AddSingleton<IValidateOptions<OtpSettings>, OtpSettingsValidator>();
        services.AddOptions<OtpSettings>()
            .Bind(configuration.GetSection(OtpSettings.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IOtpStore, InMemoryOtpStore>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
