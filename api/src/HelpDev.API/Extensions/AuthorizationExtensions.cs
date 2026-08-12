using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.AspNetCore.Authorization;

namespace HelpDev.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddHelpDevAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.Authenticated, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(AuthorizationPolicies.WriterOrAdmin, policy =>
                policy.RequireRole(AppRoles.Writer, AppRoles.Admin));

            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(AppRoles.Admin));
        });

        return services;
    }
}
