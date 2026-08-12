using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Application.Storage;
using HelpDev.Modules.Media.Application.Validation;
using HelpDev.Modules.Media.Infrastructure.Inspection;
using HelpDev.Modules.Media.Infrastructure.Persistence;
using HelpDev.Modules.Media.Infrastructure.Queries;
using HelpDev.Modules.Media.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Media;

public static class DependencyInjection
{
    public static IServiceCollection AddMediaModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<MediaOptions>, MediaOptionsValidator>();
        services.AddOptions<MediaOptions>()
            .Bind(configuration.GetSection(MediaOptions.SectionName))
            .ValidateOnStart();

        services.PostConfigure<MediaOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.LocalStorageRoot))
            {
                options.LocalStorageRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "HelpDev",
                    "media-uploads");
            }
            else
            {
                options.LocalStorageRoot = Path.GetFullPath(options.LocalStorageRoot);
            }

            if (string.IsNullOrWhiteSpace(options.PublicBasePath))
            {
                options.PublicBasePath = "/media";
            }
            else if (!options.PublicBasePath.StartsWith("/", StringComparison.Ordinal))
            {
                options.PublicBasePath = "/" + options.PublicBasePath.Trim('/');
            }
        });

        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IMediaAssetQueries, MediaAssetQueries>();
        services.AddScoped<IMediaAssetService, MediaAssetService>();
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddSingleton<IImageFileInspector, ImageFileInspector>();

        return services;
    }
}
