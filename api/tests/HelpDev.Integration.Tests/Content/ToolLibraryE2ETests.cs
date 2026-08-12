using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Dtos;
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
public sealed class ToolLibraryE2ETests : IntegrationTestClassBase
{
    public ToolLibraryE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Create_content_attach_tool_publish_refreshes_search()
    {
        var authorId = await SeedUserAsync("Tool Author");
        Guid contentId;

        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var contentService = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await contentService.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Cursor AI",
                    Slug = "cursor-ai-tool",
                    Body = "AI coding tool",
                    Type = nameof(ContentType.Tool),
                    Status = nameof(ContentStatus.Published),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        await using (var toolScope = Factory.Services.CreateAsyncScope())
        {
            var toolService = toolScope.ServiceProvider.GetRequiredService<IToolService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            var tool = await toolService.CreateAsync(
                actor,
                contentId,
                new UpdateToolRequest
                {
                    ToolName = "Cursor",
                    OfficialWebsiteUrl = "https://cursor.com",
                    GithubUrl = null,
                    CompanyName = "Anysphere",
                    PricingModel = "Freemium",
                    ToolCategory = "IDE",
                    Platforms = ["Windows", "MacOS", "Web"],
                    LicenseType = "Commercial",
                },
                CancellationToken.None);

            Assert.Equal(contentId, tool.ContentId);
            Assert.Equal("Cursor", tool.ToolName);
        }

        await using (var featureScope = Factory.Services.CreateAsyncScope())
        {
            var toolService = featureScope.ServiceProvider.GetRequiredService<IToolService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            var feature = await toolService.AddFeatureAsync(
                actor,
                contentId,
                new CreateToolFeatureRequest { Title = "AI Agent", Description = "Agentic editing", Order = 0 },
                CancellationToken.None);
            Assert.Equal("AI Agent", feature.Title);
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var verifyScope = Factory.Services.CreateAsyncScope())
        {
            var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await context.ToolMetadata.AnyAsync(t => t.ContentId == contentId));
            var toolId = await context.ToolMetadata
                .Where(t => t.ContentId == contentId)
                .Select(t => t.Id)
                .SingleAsync();
            Assert.Equal(1, await context.ToolFeatures.CountAsync(f => f.ToolId == toolId));

            var searchDocument = await context.SearchDocuments.SingleOrDefaultAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            Assert.NotNull(searchDocument);
            Assert.Equal("cursor-ai-tool", searchDocument!.Slug);
            Assert.Equal("Cursor AI", searchDocument.Title);
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
