using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Categories;
using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Application.History;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.PromptLab;

public static class DependencyInjection
{
    public static IServiceCollection AddPromptLabModule(this IServiceCollection services)
    {
        services.AddScoped<IPromptCategoryRepository, PromptCategoryRepository>();
        services.AddScoped<IPromptDefinitionRepository, PromptDefinitionRepository>();
        services.AddScoped<IPromptRepository, PromptRepository>();
        services.AddScoped<IAiModelRepository, AiModelRepository>();
        services.AddScoped<IPromptFavoriteRepository, PromptFavoriteRepository>();
        services.AddScoped<IPromptRenderRecordRepository, PromptRenderRecordRepository>();

        services.AddScoped<IPromptCatalogQueries, PromptCatalogQueries>();
        services.AddScoped<IPromptPublicQueries, PromptPublicQueries>();
        services.AddScoped<IPromptWriterQueries, PromptWriterQueries>();
        services.AddScoped<IPromptAdminReviewQueries, PromptAdminReviewQueries>();
        services.AddScoped<IPromptCategoryQueries, PromptCategoryQueries>();
        services.AddScoped<IPromptDefinitionQueries, PromptDefinitionQueries>();
        services.AddScoped<IPromptFavoriteQueries, PromptFavoriteQueries>();
        services.AddScoped<IPromptRenderHistoryQueries, PromptRenderHistoryQueries>();

        services.AddScoped<IPromptCategoryService, PromptCategoryService>();
        services.AddScoped<IPromptDefinitionService, PromptDefinitionService>();
        services.AddScoped<IPromptWriterService, PromptWriterService>();
        services.AddScoped<IPromptAdminReviewService, PromptAdminReviewService>();
        services.AddScoped<IPromptRenderService, PromptRenderService>();
        services.AddScoped<IPromptFavoriteService, PromptFavoriteService>();

        services.AddSingleton<IPromptTemplateParser, PromptTemplateParser>();
        services.AddSingleton<IPromptRenderer, PromptRenderer>();

        return services;
    }
}
