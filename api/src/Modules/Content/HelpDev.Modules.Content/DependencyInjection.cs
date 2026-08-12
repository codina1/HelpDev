using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.News;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.SeoAnalysis.Dashboard;
using HelpDev.Modules.Content.Application.StructuredData;
using HelpDev.Modules.Content.Application.Roadmaps;
using HelpDev.Modules.Content.Application.Roadmaps.Ai;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Ai;
using HelpDev.Modules.Content.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Content;

/// <summary>
/// DI entry point for the Content module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddContentModule(this IServiceCollection services)
    {
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddSingleton<IContentSeoAnalyzer, ContentSeoAnalyzer>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IAdminContentQueries, AdminContentQueries>();
        services.AddScoped<IContentRevisionRepository, ContentRevisionRepository>();
        services.AddScoped<IContentRevisionQueries, ContentRevisionQueries>();
        services.AddScoped<IContentRevisionService, ContentRevisionService>();
        services.AddScoped<IContentWorkflowTransitionRepository, ContentWorkflowTransitionRepository>();
        services.AddScoped<IContentWorkflowQueries, ContentWorkflowQueries>();
        services.AddScoped<IContentWorkflowService, ContentWorkflowService>();
        services.AddScoped<ISeoDashboardQueries, SeoDashboardQueries>();
        services.AddSingleton<IStructuredDataGenerator, StructuredDataGenerator>();
        services.AddScoped<IContentAiAssistantService, ContentAiAssistantService>();
        services.AddScoped<IContentIdeaRepository, ContentIdeaRepository>();
        services.AddScoped<IAiContentWorkflowSessionRepository, AiContentWorkflowSessionRepository>();
        services.AddScoped<AiContentWorkflowService>();
        services.AddScoped<IAiContentWorkflowService>(sp => sp.GetRequiredService<AiContentWorkflowService>());
        services.AddScoped<IAiResearchService>(sp => sp.GetRequiredService<AiContentWorkflowService>());
        services.AddScoped<IArticleMetadataRepository, ArticleMetadataRepository>();
        services.AddScoped<IArticleMetadataService, ArticleMetadataService>();
        services.AddScoped<INewsMetadataRepository, NewsMetadataRepository>();
        services.AddScoped<INewsMetadataService, NewsMetadataService>();
        services.AddScoped<IToolRepository, ToolRepository>();
        services.AddScoped<IToolService, ToolService>();
        services.AddScoped<IToolQueries, ToolQueries>();
        services.AddScoped<IToolAiAssistantService, ToolAiAssistantService>();
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        services.AddScoped<IRoadmapService, RoadmapService>();
        services.AddScoped<IRoadmapQueries, RoadmapQueries>();
        services.AddScoped<IRoadmapAiAssistantService, RoadmapAiAssistantService>();
        return services;
    }
}
