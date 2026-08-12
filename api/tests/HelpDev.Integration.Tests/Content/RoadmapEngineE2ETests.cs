using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Roadmaps;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
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
public sealed class RoadmapEngineE2ETests : IntegrationTestClassBase
{
    public RoadmapEngineE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Create_roadmap_add_steps_publish_refreshes_search()
    {
        var authorId = await SeedUserAsync("Roadmap Author");
        Guid contentId;

        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var contentService = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await contentService.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Frontend Developer Roadmap",
                    Slug = "frontend-developer-roadmap",
                    Body = "Structured learning path",
                    Type = nameof(ContentType.Roadmap),
                    Status = nameof(ContentStatus.Published),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        await using (var roadmapScope = Factory.Services.CreateAsyncScope())
        {
            var roadmapService = roadmapScope.ServiceProvider.GetRequiredService<IRoadmapService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            var roadmap = await roadmapService.CreateAsync(
                actor,
                contentId,
                new UpdateRoadmapRequest
                {
                    Level = "Beginner",
                    EstimatedDuration = "12 weeks",
                    Goal = "Become a frontend developer",
                    Prerequisites = "Basic computer literacy",
                },
                CancellationToken.None);

            Assert.Equal(contentId, roadmap.ContentId);

            await roadmapService.AddStepAsync(
                actor,
                contentId,
                new CreateRoadmapStepRequest
                {
                    Title = "HTML CSS",
                    Description = "Markup and styling",
                    EstimatedHours = 20,
                    Topics =
                    [
                        new UpsertRoadmapTopicItem { Title = "Semantics", Order = 0 },
                        new UpsertRoadmapTopicItem { Title = "Flexbox", Order = 1 },
                    ],
                    Resources =
                    [
                        new UpsertRoadmapResourceItem
                        {
                            Title = "MDN HTML",
                            Url = "https://developer.mozilla.org/en-US/docs/Web/HTML",
                            ResourceType = "Article",
                            Order = 0,
                        },
                    ],
                },
                CancellationToken.None);

            await roadmapService.AddStepAsync(
                actor,
                contentId,
                new CreateRoadmapStepRequest
                {
                    Title = "JavaScript",
                    EstimatedHours = 40,
                    Topics =
                    [
                        new UpsertRoadmapTopicItem { Title = "Variables", Order = 0 },
                        new UpsertRoadmapTopicItem { Title = "Functions", Order = 1 },
                    ],
                },
                CancellationToken.None);
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var verifyScope = Factory.Services.CreateAsyncScope())
        {
            var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await context.RoadmapMetadata.AnyAsync(r => r.ContentId == contentId));
            var roadmapId = await context.RoadmapMetadata
                .Where(r => r.ContentId == contentId)
                .Select(r => r.Id)
                .SingleAsync();
            Assert.Equal(2, await context.RoadmapSteps.CountAsync(s => s.RoadmapId == roadmapId));

            var searchDocument = await context.SearchDocuments.SingleOrDefaultAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            Assert.NotNull(searchDocument);
            Assert.Equal("frontend-developer-roadmap", searchDocument!.Slug);
            Assert.True(searchDocument.IsPublished);
        }
    }

    private async Task<Guid> SeedUserAsync(string fullName)
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = fullName,
            FirstName = fullName,
            LastName = "Tester",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}
