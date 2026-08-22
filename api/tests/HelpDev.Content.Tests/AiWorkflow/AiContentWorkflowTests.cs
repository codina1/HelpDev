using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Domain.AiWorkflow;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests.AiWorkflow;

public sealed class ContentIdeaLifecycleTests
{
    [Fact]
    public void Idea_starts_as_draft_and_transitions_only_explicitly()
    {
        var idea = ContentIdea.Create(
            Guid.NewGuid(),
            "RAG in HelpDev",
            "Explain semantic search",
            "Article",
            Guid.NewGuid(),
            new DateTime(2026, 7, 23, 8, 0, 0, DateTimeKind.Utc));

        Assert.Equal(ContentIdeaStatus.Draft, idea.Status);
        idea.MarkResearching(DateTime.UtcNow);
        Assert.Equal(ContentIdeaStatus.Researching, idea.Status);
        idea.MarkWriting(DateTime.UtcNow);
        idea.MarkReview(DateTime.UtcNow);
        idea.MarkCompleted(DateTime.UtcNow);
        Assert.Equal(ContentIdeaStatus.Completed, idea.Status);
        Assert.Throws<DomainException>(() => idea.MarkWriting(DateTime.UtcNow));
    }
}

public sealed class AiWorkflowSessionTests
{
    [Fact]
    public void Session_advances_steps_and_links_content_once()
    {
        var session = AiContentWorkflowSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);

        Assert.Equal(AiContentWorkflowStep.Research, session.CurrentStep);
        session.AdvanceTo(AiContentWorkflowStep.Outline, DateTime.UtcNow);
        Assert.Equal(AiContentWorkflowStep.Outline, session.CurrentStep);

        var contentId = Guid.NewGuid();
        session.LinkContent(contentId, DateTime.UtcNow);
        Assert.Equal(contentId, session.LinkedContentId);
        Assert.Equal(AiContentWorkflowStep.Review, session.CurrentStep);
        Assert.Throws<DomainException>(() => session.LinkContent(Guid.NewGuid(), DateTime.UtcNow));
    }
}

public sealed class AiResearchServiceTests
{
    [Fact]
    public async Task Research_uses_knowledge_context_and_does_not_expose_scores()
    {
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);
        var idea = ContentIdea.Create(Guid.NewGuid(), "Topic", "Desc", "Article", actor.UserId, DateTime.UtcNow);
        var session = AiContentWorkflowSession.Create(Guid.NewGuid(), idea.Id, actor.UserId, DateTime.UtcNow);

        var service = new AiContentWorkflowService(
            new InMemoryIdeaRepository(idea),
            new InMemorySessionRepository(session),
            new StubKnowledge(new WorkflowKnowledgeContext(
                "Topic",
                [new WorkflowKnowledgeSource("Doc", "/content/x", "content", "HelpDev uses pgvector.", 0.9)])),
            new StubAi("Research brief without scores."),
            new AlwaysOnGate(),
            new ContentSeoAnalyzer(),
            new UnusedContentService(),
            new UnusedContentRepository(),
            new UnusedRevisionService(),
            new StubUsage(),
            new StubAudit(),
            new StubUnitOfWork(),
            new FixedClock(DateTime.UtcNow),
            NullLogger<AiContentWorkflowService>.Instance);

        var result = await service.ResearchAsync(actor, session.Id);

        Assert.Equal("Research brief without scores.", result.Summary);
        Assert.Single(result.Sources);
        Assert.DoesNotContain(typeof(AiResearchResultDto).GetProperties(), p => p.Name.Contains("Confidence", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(AiResearchResultDto).GetProperties(), p => p.Name.Contains("Hallucination", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FixedClock(DateTime utc) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utc;
    }

    private sealed class AlwaysOnGate : IContentAiFeatureGate
    {
        public bool IsEnabled => true;
        public string DefaultModel => "fake";
        public bool IsTaskAllowed(ContentAiTaskType taskType) => true;
    }

    private sealed class StubKnowledge(WorkflowKnowledgeContext context) : IWorkflowKnowledgeRetriever
    {
        public Task<WorkflowKnowledgeContext> RetrieveAsync(string topic, int take = 8, CancellationToken cancellationToken = default) =>
            Task.FromResult(context);
    }

    private sealed class StubAi(string text) : IAiTextGenerator
    {
        public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AiTextResponse(text, "fake", "Fake", new AiTokenUsage(1, 2)));

        public Task<AiGenerationResult> GenerateSafeAsync(
            AiTextRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(AiGenerationResult.Ok(text, 1, "fake", "Fake", new AiTokenUsage(1, 2)));
    }

    private sealed class StubUsage : IAiUsageRecorder
    {
        public Task RecordAsync(AiUsageRecordInput input, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubAudit : IAuditRecorder
    {
        public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class InMemoryIdeaRepository(ContentIdea idea) : IContentIdeaRepository
    {
        public Task<ContentIdea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentIdea?>(idea.Id == id ? idea : null);

        public Task AddAsync(ContentIdea value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class InMemorySessionRepository(AiContentWorkflowSession session) : IAiContentWorkflowSessionRepository
    {
        public Task<AiContentWorkflowSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<AiContentWorkflowSession?>(session.Id == id ? session : null);

        public Task AddAsync(AiContentWorkflowSession value, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<AiContentWorkflowSession>> ListByCreatorAsync(Guid? createdByUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AiContentWorkflowSession>>([session]);
    }

    private sealed class UnusedContentService : IContentService
    {
        public Task<IReadOnlyList<ContentListItemDto>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentListItemDto>>([]);

        public Task<ContentDetailDto> GetPublishedBySlugAsync(string slug, Guid? viewerUserId = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ContentDetailDto> CreateAsync(Guid authorId, CreateContentRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> UpdateAsync(ContentManagementActor actor, Guid id, UpdateContentRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> PublishAsync(ContentManagementActor actor, Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> UpdateSeoMetadataAsync(ContentManagementActor actor, Guid id, UpdateSeoMetadataRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<AdminContentDetailDto> GetManagedByIdAsync(ContentManagementActor actor, Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<SeoAuditReportDto> AnalyzeSeoAsync(ContentManagementActor actor, Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public PreviewArticleDto Preview(PreviewArticleRequest request) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedContentRepository : IContentRepository
    {
        public Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentEntity>>([]);

        public Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentEntity?>(null);

        public Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentEntity?>(null);

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default) =>
            Task.FromResult(content);
    }

    private sealed class UnusedRevisionService : IContentRevisionService
    {
        public Task<AdminContentDetailDto> RestoreAsync(ContentManagementActor actor, Guid contentId, int versionNumber, RestoreContentRevisionRequest? request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AppendRevisionAsync(ContentEntity content, Guid createdByUserId, string? changeReason, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
