using HelpDev.Modules.Content.Domain.AiWorkflow;
using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.News;
using HelpDev.Modules.Content.Domain.Tools;
using HelpDev.Modules.Content.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Persistence;

/// <summary>
/// Persistence port for Content. Implemented by the shared ApplicationDbContext
/// during incremental migration so the module does not reference legacy Infrastructure.
/// </summary>
public interface IContentDbContext
{
    DbSet<ContentEntity> Contents { get; }

    DbSet<ContentRevision> ContentRevisions { get; }

    DbSet<ContentWorkflowTransition> ContentWorkflowTransitions { get; }

    DbSet<ContentIdea> ContentIdeas { get; }

    DbSet<AiContentWorkflowSession> AiContentWorkflowSessions { get; }

    DbSet<ArticleMetadata> ArticleMetadata { get; }

    DbSet<NewsMetadata> NewsMetadata { get; }

    DbSet<ToolMetadata> ToolMetadata { get; }

    DbSet<ToolFeature> ToolFeatures { get; }

    DbSet<ToolAlternative> ToolAlternatives { get; }

    DbSet<RoadmapMetadata> RoadmapMetadata { get; }

    DbSet<RoadmapStep> RoadmapSteps { get; }

    DbSet<RoadmapTopic> RoadmapTopics { get; }

    DbSet<RoadmapResource> RoadmapResources { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
