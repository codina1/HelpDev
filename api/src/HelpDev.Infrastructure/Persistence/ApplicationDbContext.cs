using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Domain.Announcements;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using HelpDev.Modules.Administration.Domain.Settings;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.AiWorkflow;
using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.News;
using HelpDev.Modules.Content.Domain.Tools;
using HelpDev.Modules.Content.Domain.Roadmaps;
using HelpDev.Modules.Identity.Application.Persistence;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.Modules.Learning.Domain.Personalization;
using HelpDev.Modules.Media.Application.Persistence;
using HelpDev.Modules.Media.Domain.Assets;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.AiUsage;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.Infrastructure.Outbox;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Events;
using Microsoft.EntityFrameworkCore;
using AdministrationModuleMarker = HelpDev.Modules.Administration.ModuleMarker;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;
using ContentModuleMarker = HelpDev.Modules.Content.ModuleMarker;
using IdentityModuleMarker = HelpDev.Modules.Identity.ModuleMarker;
using LearningModuleMarker = HelpDev.Modules.Learning.ModuleMarker;
using MediaModuleMarker = HelpDev.Modules.Media.ModuleMarker;
using SearchModuleMarker = HelpDev.Modules.Search.ModuleMarker;
using ToolboxModuleMarker = HelpDev.Modules.Toolbox.ModuleMarker;
using PromptLabModuleMarker = HelpDev.Modules.PromptLab.ModuleMarker;
using AnalyticsModuleMarker = HelpDev.Modules.Analytics.ModuleMarker;
using AuditingModuleMarker = HelpDev.Modules.Auditing.ModuleMarker;

namespace HelpDev.Infrastructure.Persistence;

public class ApplicationDbContext :
    DbContext,
    IIdentityDbContext,
    IContentDbContext,
    ILearningDbContext,
    ISearchDbContext,
    IAdministrationDbContext,
    IToolboxDbContext,
    IPromptLabDbContext,
    IAnalyticsDbContext,
    IAuditDbContext,
    IMediaDbContext,
    IUnitOfWork
{
    private readonly IOutboxEventSerializer _outboxEventSerializer;

    /// <summary>
    /// Design-time / options-only construction.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : this(options, NullOutboxEventSerializer.Instance)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOutboxEventSerializer outboxEventSerializer)
        : base(options)
    {
        _outboxEventSerializer = outboxEventSerializer
            ?? throw new ArgumentNullException(nameof(outboxEventSerializer));
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<ContentEntity> Contents => Set<ContentEntity>();

    public DbSet<ContentRevision> ContentRevisions => Set<ContentRevision>();

    public DbSet<ContentWorkflowTransition> ContentWorkflowTransitions => Set<ContentWorkflowTransition>();

    public DbSet<ContentIdea> ContentIdeas => Set<ContentIdea>();

    public DbSet<AiContentWorkflowSession> AiContentWorkflowSessions => Set<AiContentWorkflowSession>();

    public DbSet<ArticleMetadata> ArticleMetadata => Set<ArticleMetadata>();

    public DbSet<NewsMetadata> NewsMetadata => Set<NewsMetadata>();

    public DbSet<ToolMetadata> ToolMetadata => Set<ToolMetadata>();

    public DbSet<ToolFeature> ToolFeatures => Set<ToolFeature>();

    public DbSet<ToolAlternative> ToolAlternatives => Set<ToolAlternative>();

    public DbSet<RoadmapMetadata> RoadmapMetadata => Set<RoadmapMetadata>();

    public DbSet<RoadmapStep> RoadmapSteps => Set<RoadmapStep>();

    public DbSet<RoadmapTopic> RoadmapTopics => Set<RoadmapTopic>();

    public DbSet<RoadmapResource> RoadmapResources => Set<RoadmapResource>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<LearningProfile> LearningProfiles => Set<LearningProfile>();

    public DbSet<LearningPreference> LearningPreferences => Set<LearningPreference>();

    public DbSet<LearningRoadmap> LearningRoadmaps => Set<LearningRoadmap>();

    public DbSet<LearningRoadmapStep> LearningRoadmapSteps => Set<LearningRoadmapStep>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<SearchDocument> SearchDocuments => Set<SearchDocument>();

    public DbSet<SearchChunk> SearchChunks => Set<SearchChunk>();

    public DbSet<SearchVector> SearchVectors => Set<SearchVector>();

    public DbSet<SearchSemanticIndexState> SearchSemanticIndexStates => Set<SearchSemanticIndexState>();

    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<Announcement> Announcements => Set<Announcement>();

    public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();

    public DbSet<ToolDefinition> ToolDefinitions => Set<ToolDefinition>();

    public DbSet<ToolFavorite> ToolFavorites => Set<ToolFavorite>();

    public DbSet<ToolExecutionRecord> ToolExecutionRecords => Set<ToolExecutionRecord>();

    public DbSet<PromptCategory> PromptCategories => Set<PromptCategory>();

    public DbSet<PromptDefinition> PromptDefinitions => Set<PromptDefinition>();

    public DbSet<PromptFavorite> PromptFavorites => Set<PromptFavorite>();

    public DbSet<PromptRenderRecord> PromptRenderRecords => Set<PromptRenderRecord>();

    public DbSet<AnalyticsEventReceipt> AnalyticsEventReceipts => Set<AnalyticsEventReceipt>();

    public DbSet<DailyMetric> DailyMetrics => Set<DailyMetric>();

    public DbSet<DailyActiveUser> DailyActiveUsers => Set<DailyActiveUser>();

    public DbSet<AnalyticsSubjectSnapshot> AnalyticsSubjectSnapshots => Set<AnalyticsSubjectSnapshot>();

    public DbSet<AiUsageRecord> AiUsageRecords => Set<AiUsageRecord>();

    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearningModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ToolboxModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PromptLabModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditingModuleMarker).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaModuleMarker).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Persists tracked business changes and Outbox rows atomically, then clears Aggregate events.
    /// Does not dispatch Domain Events synchronously; the Outbox processor dispatches later.
    /// </summary>
    /// <remarks>
    /// Pipeline: snapshot events → stage OutboxMessage rows → base.SaveChangesAsync → clear.
    /// The returned affected-row count includes newly inserted Outbox rows.
    /// Synchronous <see cref="DbContext.SaveChanges()"/> overloads are not overridden and do not write Outbox rows.
    /// </remarks>
    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        var snapshots = DomainEventCommitPipeline.Capture(CollectAggregatesWithDomainEvents());
        foreach (var message in OutboxCapture.CreateMessages(snapshots, _outboxEventSerializer))
        {
            OutboxMessages.Add(message);
        }

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
            .ConfigureAwait(false);

        DomainEventCommitPipeline.ClearCaptured(snapshots);
        return result;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    private IEnumerable<IHasDomainEvents> CollectAggregatesWithDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IHasDomainEvents aggregate && aggregate.DomainEvents.Count > 0)
            {
                yield return aggregate;
            }
        }
    }
}
