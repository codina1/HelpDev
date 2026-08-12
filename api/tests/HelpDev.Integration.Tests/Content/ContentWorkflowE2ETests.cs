using HelpDev.Infrastructure.Outbox;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Search.Domain;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Content;

[Collection(PostgreSqlCollection.Name)]
public sealed class ContentWorkflowE2ETests : IntegrationTestClassBase
{
    public ContentWorkflowE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Submit_approve_publish_writes_outbox_and_search()
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
                    Title = "Workflow Article",
                    Slug = "workflow-article",
                    Body = "Body",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        var workflow = Factory.Services.GetRequiredService<IContentWorkflowService>();

        await workflow.SubmitForReviewAsync(
            new ContentManagementActor(authorId, canManageAllContent: false),
            contentId,
            CancellationToken.None);

        await workflow.ApproveAsync(
            new ContentManagementActor(authorId, canManageAllContent: true),
            contentId,
            CancellationToken.None);

        await workflow.PublishAsync(
            new ContentManagementActor(authorId, canManageAllContent: true),
            contentId,
            CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HelpDev.Infrastructure.Persistence.ApplicationDbContext>();
            Assert.Equal(3, await context.ContentWorkflowTransitions.CountAsync(t => t.ContentId == contentId));
            Assert.True(await context.OutboxMessages.AnyAsync(m => m.Type == "content.published.v1"));
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<HelpDev.Infrastructure.Persistence.ApplicationDbContext>();
            var doc = await context.SearchDocuments.SingleOrDefaultAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            Assert.NotNull(doc);
            Assert.True(doc!.IsPublished);
        }
    }

    private async Task<Guid> SeedUserAsync()
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<HelpDev.Infrastructure.Persistence.ApplicationDbContext>();
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "Workflow User",
            FirstName = "Workflow",
            LastName = "User",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}
