using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Deployment;

public static class DeploymentServiceCollectionExtensions
{
    public static IServiceCollection AddHelpDevDeployment(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<ReverseProxyOptions>, ReverseProxyOptionsValidator>();
        services.AddOptions<ReverseProxyOptions>()
            .Bind(configuration.GetSection(ReverseProxyOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<HttpsPolicyOptions>, HttpsPolicyOptionsValidator>();
        services.AddOptions<HttpsPolicyOptions>()
            .Bind(configuration.GetSection(HttpsPolicyOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ReleaseMetadataOptions>, ReleaseMetadataOptionsValidator>();
        services.AddOptions<ReleaseMetadataOptions>()
            .Bind(configuration.GetSection(ReleaseMetadataOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ShutdownOptions>, ShutdownOptionsValidator>();
        services.AddOptions<ShutdownOptions>()
            .Bind(configuration.GetSection(ShutdownOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IApplicationReadinessState, ApplicationReadinessState>();
        services.AddSingleton<IProductionSafetyValidator, ProductionSafetyValidator>();
        services.AddSingleton<IReleaseInfoProvider, ReleaseInfoProvider>();

        var shutdownOptions = configuration.GetSection(ShutdownOptions.SectionName).Get<ShutdownOptions>() ?? new ShutdownOptions();
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(shutdownOptions.TimeoutSeconds);
        });

        return services;
    }
}

public static class DeploymentApplicationBuilderExtensions
{
    /// <summary>
    /// Runs centralized production safety validation. In Production, any error fails startup.
    /// In non-Production environments, errors are logged as warnings unless <paramref name="bypass"/> is set.
    /// </summary>
    public static void ValidateProductionSafety(this WebApplication app, bool bypass = false)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ProductionSafety");
        var validator = app.Services.GetRequiredService<IProductionSafetyValidator>();

        logger.LogInformation("Event={Event}", DeploymentLogEvents.ProductionSafetyValidationStarted);

        var result = validator.Validate();

        foreach (var warning in result.Warnings)
        {
            logger.LogWarning("Event={Event} Detail={Detail}", DeploymentLogEvents.ProductionSafetyWarning, warning);
        }

        if (result.IsValid)
        {
            return;
        }

        if (app.Environment.IsProduction() && !bypass)
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Event={Event} Detail={Detail}", DeploymentLogEvents.ProductionSafetyValidationFailed, error);
            }

            throw new ProductionSafetyValidationException(result.Errors);
        }

        foreach (var error in result.Errors)
        {
            logger.LogWarning("Event={Event} Detail={Detail}", DeploymentLogEvents.ProductionSafetyValidationFailed, error);
        }
    }

    public static WebApplication UseHelpDevProductionHardening(this WebApplication app)
    {
        var https = app.Services.GetRequiredService<IOptions<HttpsPolicyOptions>>().Value;

        if (!app.Environment.IsDevelopment() && https.EnableHsts)
        {
            app.UseHsts();
        }

        if (!app.Environment.IsDevelopment() && https.RedirectToHttps)
        {
            app.UseHttpsRedirection();
        }

        return app;
    }

    public static void RegisterReadinessLifecycle(this WebApplication app)
    {
        var readiness = app.Services.GetRequiredService<IApplicationReadinessState>();
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Lifecycle");

        lifetime.ApplicationStarted.Register(() =>
        {
            readiness.MarkReady();
            logger.LogInformation("Event={Event}", DeploymentLogEvents.ApplicationStarted);
        });

        lifetime.ApplicationStopping.Register(() =>
        {
            readiness.MarkStopping();
            logger.LogInformation("Event={Event}", DeploymentLogEvents.ApplicationStopping);
        });

        lifetime.ApplicationStopped.Register(() =>
            logger.LogInformation("Event={Event}", DeploymentLogEvents.ApplicationStopped));
    }
}

public sealed class ProductionSafetyValidationException : Exception
{
    public ProductionSafetyValidationException(IReadOnlyList<string> errors)
        : base($"Production safety validation failed with {errors.Count} error(s). See logs for safe details.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
