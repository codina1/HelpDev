using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Administration.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Administration;

public static class DependencyInjection
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services)
    {
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();

        services.AddScoped<IFeatureFlagQueries, FeatureFlagQueries>();
        services.AddScoped<ISystemSettingQueries, SystemSettingQueries>();
        services.AddScoped<IPublicSystemSettingQueries, PublicSystemSettingQueries>();
        services.AddScoped<IAnnouncementQueries, AnnouncementQueries>();

        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IAdministrationDashboardQueries, AdministrationDashboardQueries>();

        return services;
    }
}
