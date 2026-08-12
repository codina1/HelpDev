using HelpDev.Modules.Search.Application.Chunking;
using HelpDev.Modules.Search.Application.Handlers;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Application.Knowledge;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Application.Queries;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Application.Search;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Infrastructure.Persistence;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Modules.Search;

public static class DependencyInjection
{
    public static IServiceCollection AddSearchModule(this IServiceCollection services)
    {
        services.AddScoped<ISearchDocumentRepository, SearchDocumentRepository>();
        services.AddScoped<ISearchChunkRepository, SearchChunkRepository>();
        services.AddScoped<ISearchVectorRepository, SearchVectorRepository>();
        services.AddScoped<ISearchSemanticIndexStateRepository, SearchSemanticIndexStateRepository>();
        services.AddScoped<ISearchQueries, SearchQueries>();
        services.AddScoped<ISemanticSearchQueries, SemanticSearchQueries>();
        services.AddScoped<IKnowledgeDashboardQueries, KnowledgeDashboardQueries>();
        services.AddScoped<IRagContextBuilder, RagContextBuilder>();
        services.AddScoped<IRagAnswerService, RagAnswerService>();
        services.AddSingleton<MarkdownKnowledgeChunker>();
        services.AddSingleton<IKnowledgeChunker>(sp => sp.GetRequiredService<MarkdownKnowledgeChunker>());
        services.AddSingleton<IContentChunker>(sp => sp.GetRequiredService<MarkdownKnowledgeChunker>());
        services.AddScoped<ISemanticIndexingService, SemanticIndexingService>();
        services.AddScoped<ISearchProjectionService, SearchProjectionService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ISearchReindexService, SearchReindexService>();
        services.AddScoped<CourseSearchProjectionApplier>();

        services.AddScoped<IDomainEventHandler<ContentPublishedDomainEvent>, ContentPublishedSearchHandler>();
        services.AddScoped<IDomainEventHandler<ContentUpdatedDomainEvent>, ContentUpdatedSearchHandler>();
        services.AddScoped<IDomainEventHandler<ContentPublishedDomainEvent>, ContentPublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<ContentUpdatedDomainEvent>, ContentUpdatedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<CoursePublishedDomainEvent>, CoursePublishedSearchHandler>();
        services.AddScoped<IDomainEventHandler<CourseUpdatedDomainEvent>, CourseUpdatedSearchHandler>();
        services.AddScoped<IDomainEventHandler<CoursePublishedDomainEvent>, CoursePublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<CourseUpdatedDomainEvent>, CourseUpdatedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<LessonPublishedDomainEvent>, LessonPublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<ToolPublishedDomainEvent>, ToolPublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<ToolUnpublishedDomainEvent>, ToolUnpublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<PromptPublishedDomainEvent>, PromptPublishedSemanticIndexingHandler>();
        services.AddScoped<IDomainEventHandler<PromptUnpublishedDomainEvent>, PromptUnpublishedSemanticIndexingHandler>();

        return services;
    }
}
