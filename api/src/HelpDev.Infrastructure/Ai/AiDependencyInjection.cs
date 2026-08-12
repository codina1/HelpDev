using HelpDev.SharedContracts.Ai;
using HelpDev.Modules.Content.Application.ContentAi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

public static class AiDependencyInjection
{
    public static IServiceCollection AddAiInfrastructure(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<AiProviderOptions>, AiProviderOptionsValidator>();
        services.AddOptions<AiProviderOptions>()
            .Bind(configuration.GetSection(AiProviderOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpClient(nameof(HttpAiTextGenerator), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<IContentAiFeatureGate, ContentAiFeatureGate>();
        services.AddSingleton<FakeAiFailureInjector>();
        services.AddSingleton<IAiOperationMetrics, AiOperationMetrics>();
        services.AddSingleton<AiRetryPolicy>(sp =>
            new AiRetryPolicy(logger: sp.GetService<ILogger<AiRetryPolicy>>()));
        services.AddSingleton<IAiHealthProbe, AiHealthProbe>();

        services.AddScoped<FakeAiTextGenerator>();
        services.AddScoped<HttpAiTextGenerator>(sp =>
        {
            var http = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(HttpAiTextGenerator));
            return ActivatorUtilities.CreateInstance<HttpAiTextGenerator>(sp, http);
        });

        services.AddScoped<IAiTextGenerator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AiProviderOptions>>().Value;
            var provider = (options.ProviderName ?? "Fake").Trim();
            IAiTextGenerator inner = string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<HttpAiTextGenerator>()
                : sp.GetRequiredService<FakeAiTextGenerator>();

            return new ResilientAiTextGenerator(
                inner,
                sp.GetRequiredService<AiRetryPolicy>(),
                sp.GetRequiredService<IAiOperationMetrics>(),
                sp.GetRequiredService<ILogger<ResilientAiTextGenerator>>());
        });

        services.AddSingleton<IValidateOptions<EmbeddingOptions>, EmbeddingOptionsValidator>();
        services.AddOptions<EmbeddingOptions>()
            .Bind(configuration.GetSection(EmbeddingOptions.SectionName))
            .ValidateOnStart();

        services.AddHttpClient(nameof(HttpEmbeddingGenerator), client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<IEmbeddingGenerator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EmbeddingOptions>>().Value;
            var provider = (options.ProviderName ?? "Fake").Trim();
            if (string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
            {
                var http = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(nameof(HttpEmbeddingGenerator));
                return ActivatorUtilities.CreateInstance<HttpEmbeddingGenerator>(sp, http);
            }

            return ActivatorUtilities.CreateInstance<FakeEmbeddingGenerator>(sp);
        });

        return services;
    }
}
