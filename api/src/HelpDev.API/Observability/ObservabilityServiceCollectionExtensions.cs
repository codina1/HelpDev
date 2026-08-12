using HelpDev.Infrastructure.Observability;
using HelpDev.Infrastructure.Observability.HealthChecks;
using HelpDev.SharedContracts.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Observability;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddHelpDevObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<ObservabilityOptions>, ObservabilityOptionsValidator>();
        services.AddOptions<ObservabilityOptions>()
            .Bind(configuration.GetSection(ObservabilityOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IApplicationInfo, ApplicationInfo>();
        services.AddSingleton<IApplicationLifetimeInfo, ApplicationLifetimeInfo>();

        ValidateProductionLogging(configuration, environment);

        services.AddHealthChecks()
            .AddCheck<SelfHealthCheck>(
                HealthCheckNames.Self,
                tags: [HealthCheckTags.Live, HealthCheckTags.Critical])
            .AddCheck<ReadinessHealthCheck>(
                "readiness_aggregate",
                tags: [HealthCheckTags.Ready]);

        return services;
    }

    public static WebApplication MapHelpDevHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live),
            ResponseWriter = PublicHealthResponseWriter.WriteMinimalResponse,
        }).AllowAnonymous().DisableRateLimiting();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Ready),
            ResponseWriter = PublicHealthResponseWriter.WriteMinimalResponse,
        }).AllowAnonymous().DisableRateLimiting();

        return app;
    }

    private static void ValidateProductionLogging(IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        var sensitiveDataLogging = configuration.GetSection("Logging:EnableSensitiveDataLogging").Get<bool?>();
        if (sensitiveDataLogging == true)
        {
            throw new InvalidOperationException("EnableSensitiveDataLogging must not be enabled in production.");
        }
    }
}
