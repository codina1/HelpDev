using HelpDev.Infrastructure.Ai;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedContracts.Ai;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Ai;

[Collection(PostgreSqlCollection.Name)]
public sealed class AiPlatformReliabilityE2ETests : IntegrationTestClassBase
{
    public AiPlatformReliabilityE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Workflow_research_outline_draft_apply_creates_revision()
    {
        var authorId = await SeedUserAsync();
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        await using var scope = Factory.Services.CreateAsyncScope();
        var workflow = scope.ServiceProvider.GetRequiredService<IAiContentWorkflowService>();

        var session = await workflow.CreateAsync(
            actor,
            new CreateAiContentWorkflowRequest("AI Reliability Guide", "Production AI", "Article"));

        var research = await workflow.ResearchAsync(actor, session.Id);
        Assert.False(string.IsNullOrWhiteSpace(research.Summary));

        var outline = await workflow.GenerateOutlineAsync(
            actor,
            session.Id,
            new GenerateOutlineRequest(research.Summary));
        Assert.False(string.IsNullOrWhiteSpace(outline.RawText));

        var draft = await workflow.GenerateDraftAsync(
            actor,
            session.Id,
            new GenerateDraftRequest(outline.Title, outline.RawText));
        Assert.False(string.IsNullOrWhiteSpace(draft.BodyMarkdown));

        var applied = await workflow.ApplyDraftAsync(
            actor,
            session.Id,
            new ApplyDraftRequest(draft.Title, draft.BodyMarkdown, "ai-reliability-guide", "Article"));

        Assert.NotEqual(Guid.Empty, applied.ContentId);
        Assert.True(applied.RevisionVersion >= 1);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True(await db.AiUsageRecords.AnyAsync(r => r.TaskType == AiOperationNames.WorkflowResearch && r.Success));
        Assert.True(await db.ContentRevisions.AnyAsync(r => r.ContentId == applied.ContentId));
    }

    [PostgreSqlFact]
    public async Task Publish_outbox_chunk_embed_enables_rag_context()
    {
        var authorId = await SeedUserAsync();
        Guid contentId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "PostgreSQL Vector Search",
                    Slug = "postgresql-vector-search-ai",
                    Body = "HelpDev indexes published knowledge with pgvector embeddings for RAG answers.",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        var workflow = Factory.Services.GetRequiredService<IContentWorkflowService>();
        var admin = new ContentManagementActor(authorId, canManageAllContent: true);
        await workflow.SubmitForReviewAsync(admin, contentId, CancellationToken.None);
        await workflow.ApproveAsync(admin, contentId, CancellationToken.None);
        await workflow.PublishAsync(admin, contentId, CancellationToken.None);

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.SearchDocuments.AnyAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId));
            Assert.True(await db.SearchChunks.AnyAsync(c => c.SourceId == contentId));
            Assert.True(await db.SearchVectors.AnyAsync());

            var rag = scope.ServiceProvider.GetRequiredService<IRagAnswerService>();
            var answer = await rag.AskAsync("How does HelpDev use pgvector?");
            Assert.False(string.IsNullOrWhiteSpace(answer.Answer));
            Assert.True(await db.AiUsageRecords.AnyAsync(r => r.TaskType == AiOperationNames.RagAnswer));
        }
    }

    [PostgreSqlFact]
    public async Task Provider_failure_records_usage_and_keeps_workflow_consistent()
    {
        var authorId = await SeedUserAsync();
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);
        var injector = Factory.Services.GetRequiredService<FakeAiFailureInjector>();
        injector.Clear();
        // Exhaust retries (policy max 3) with transient failures.
        injector.Arm(AiErrorCodes.ProviderUnavailable, failureCount: 5);

        try
        {
            await using var scope = Factory.Services.CreateAsyncScope();
            var workflow = scope.ServiceProvider.GetRequiredService<IAiContentWorkflowService>();
            var session = await workflow.CreateAsync(
                actor,
                new CreateAiContentWorkflowRequest("Failure Topic", "desc", "Article"));

            var ex = await Assert.ThrowsAsync<ContentAiException>(
                () => workflow.ResearchAsync(actor, session.Id));

            Assert.Equal(ContentAiErrorCodes.ProviderFailed, ex.Code);

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var usage = await db.AiUsageRecords
                .Where(r => r.TaskType == AiOperationNames.WorkflowResearch && !r.Success)
                .OrderByDescending(r => r.CreatedAtUtc)
                .FirstOrDefaultAsync();

            Assert.NotNull(usage);
            Assert.Equal(AiErrorCodes.ProviderUnavailable, usage!.ErrorCode);

            var persisted = await workflow.GetByIdAsync(actor, session.Id);
            Assert.Equal(session.Id, persisted.Id);
            Assert.Null(persisted.LinkedContentId);
        }
        finally
        {
            injector.Clear();
        }
    }

    private async Task<Guid> SeedUserAsync()
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "AI E2E User",
            FirstName = "AI",
            LastName = "E2E",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}
