using Microsoft.Extensions.DependencyInjection;
using HelpDev.Application.Admin;
using HelpDev.Application.Test;

namespace HelpDev.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<ITestService, TestService>();
        return services;
    }
}
