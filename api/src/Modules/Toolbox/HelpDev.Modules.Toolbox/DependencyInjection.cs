using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Categories;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Application.History;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Application.Tools;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Toolbox;

public static class DependencyInjection
{
    public static IServiceCollection AddToolboxModule(this IServiceCollection services)
    {
        services.AddScoped<IToolCategoryRepository, ToolCategoryRepository>();
        services.AddScoped<IToolDefinitionRepository, ToolDefinitionRepository>();
        services.AddScoped<IToolFavoriteRepository, ToolFavoriteRepository>();
        services.AddScoped<IToolExecutionRecordRepository, ToolExecutionRecordRepository>();

        services.AddScoped<IToolCatalogQueries, ToolCatalogQueries>();
        services.AddScoped<IToolCategoryQueries, ToolCategoryQueries>();
        services.AddScoped<IToolDefinitionQueries, ToolDefinitionQueries>();
        services.AddScoped<IToolFavoriteQueries, ToolFavoriteQueries>();
        services.AddScoped<IToolExecutionHistoryQueries, ToolExecutionHistoryQueries>();

        services.AddScoped<IToolCategoryService, ToolCategoryService>();
        services.AddScoped<IToolDefinitionService, ToolDefinitionService>();
        services.AddScoped<IToolExecutionService, ToolExecutionService>();
        services.AddScoped<IToolFavoriteService, ToolFavoriteService>();

        services.AddSingleton<IToolExecutor, JsonFormatterToolExecutor>();
        services.AddSingleton<IToolExecutor, JsonValidatorToolExecutor>();
        services.AddSingleton<IToolExecutor, Base64EncodeToolExecutor>();
        services.AddSingleton<IToolExecutor, Base64DecodeToolExecutor>();
        services.AddSingleton<IToolExecutor, UrlEncodeToolExecutor>();
        services.AddSingleton<IToolExecutor, UrlDecodeToolExecutor>();
        services.AddSingleton<IToolExecutor, UuidGeneratorToolExecutor>();
        services.AddSingleton<IToolExecutor, HashGeneratorToolExecutor>();
        services.AddSingleton<IToolExecutor, TimestampConverterToolExecutor>();
        services.AddSingleton<IToolExecutor, TextStatisticsToolExecutor>();
        services.AddSingleton<IToolExecutor, RegexTesterToolExecutor>();
        services.AddSingleton<IToolExecutorRegistry, ToolExecutorRegistry>();

        return services;
    }
}
