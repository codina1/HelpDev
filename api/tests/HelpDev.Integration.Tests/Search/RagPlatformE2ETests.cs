using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Search;

/// <summary>
/// Sprint 44 — RAG pipeline on real PostgreSQL: chunk → embed → vector → retrieve → answer.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Rag")]
public sealed class RagPlatformE2ETests : IntegrationTestClassBase
{
    public RagPlatformE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Published_knowledge_produces_non_empty_rag_context_without_secret_leakage()
    {
        var authorId = await SeedUserAsync();
        Guid contentId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await content.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "HelpDev pgvector RAG Guide",
                    Slug = $"rag-guide-{Guid.NewGuid():N}"[..40],
                    Body = "HelpDev stores embeddings in PostgreSQL with pgvector. RAG answers use only HelpDev knowledge snippets.",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                });
            contentId = created.Id;
        }

        var workflow = Factory.Services.GetRequiredService<IContentWorkflowService>();
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);
        await workflow.SubmitForReviewAsync(actor, contentId);
        await workflow.ApproveAsync(actor, contentId);
        await workflow.PublishAsync(actor, contentId);

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.SearchChunks.AnyAsync(c => c.SourceId == contentId));
            Assert.True(await db.SearchVectors.AnyAsync());

            var contextBuilder = scope.ServiceProvider.GetRequiredService<IRagContextBuilder>();
            var context = await contextBuilder.BuildAsync("How does HelpDev use pgvector?");
            Assert.NotEmpty(context.Chunks);
            Assert.All(context.Chunks, chunk =>
            {
                Assert.False(string.IsNullOrWhiteSpace(chunk.Snippet));
                Assert.DoesNotContain("sk-", chunk.Snippet, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("ApiKey", chunk.Snippet, StringComparison.OrdinalIgnoreCase);
                Assert.False(chunk.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && chunk.Url.Contains("openai", StringComparison.OrdinalIgnoreCase));
            });

            // Vectors must not be exposed through RAG DTO surface.
            Assert.DoesNotContain(
                typeof(RagAnswerDto).GetProperties().Select(p => p.Name),
                n => n.Contains("Embedding", StringComparison.OrdinalIgnoreCase)
                    || n.Contains("Vector", StringComparison.OrdinalIgnoreCase));

            var rag = scope.ServiceProvider.GetRequiredService<IRagAnswerService>();
            var answer = await rag.AskAsync("How does HelpDev use pgvector?");
            Assert.False(string.IsNullOrWhiteSpace(answer.Answer));
            Assert.DoesNotContain("sk-", answer.Answer, StringComparison.OrdinalIgnoreCase);

            Assert.True(await db.AiUsageRecords.AnyAsync(r => r.TaskType == AiOperationNames.RagAnswer));
        }

        SensitiveLogAssertionHelper.AssertSentinelsAbsent(
            CapturedLogs,
            "sk-",
            "ApiKey",
            "Bearer ");
    }

    private async Task<Guid> SeedUserAsync()
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "RAG User",
            FirstName = "RAG",
            LastName = "User",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        return userId;
    }
}
