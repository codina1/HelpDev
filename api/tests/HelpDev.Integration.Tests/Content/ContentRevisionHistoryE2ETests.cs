using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Content;

[Collection(PostgreSqlCollection.Name)]
public sealed class ContentRevisionHistoryE2ETests : IntegrationTestClassBase
{
    public ContentRevisionHistoryE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Update_restore_creates_revisions_outbox_and_search_refresh()
    {
        var authorId = await SeedUserAsync("Revision Author");
        Guid contentId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Rev Title v0",
                    Slug = "rev-history",
                    Body = "Body v0",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Published),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var updateScope = Factory.Services.CreateAsyncScope())
        {
            var service = updateScope.ServiceProvider.GetRequiredService<IContentService>();
            await service.UpdateAsync(
                new ContentManagementActor(authorId, canManageAllContent: false),
                contentId,
                new UpdateContentRequest
                {
                    Title = "Rev Title v1",
                    Slug = "rev-history",
                    Type = nameof(ContentType.Article),
                    Body = "Body v1",
                },
                CancellationToken.None);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var revisions = await context.ContentRevisions
                .AsNoTracking()
                .Where(r => r.ContentId == contentId)
                .OrderBy(r => r.VersionNumber)
                .ToListAsync();
            Assert.Single(revisions);
            Assert.Equal(1, revisions[0].VersionNumber);
            Assert.Equal("Rev Title v1", revisions[0].Snapshot.Title);
        }

        AdminContentDetailDto restored;
        await using (var restoreScope = Factory.Services.CreateAsyncScope())
        {
            var revisionService = restoreScope.ServiceProvider.GetRequiredService<IContentRevisionService>();
            restored = await revisionService.RestoreAsync(
                new ContentManagementActor(authorId, canManageAllContent: false),
                contentId,
                versionNumber: 1,
                new RestoreContentRevisionRequest("Restore test"),
                CancellationToken.None);
        }

        Assert.Equal("Rev Title v1", restored.Title);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var count = await context.ContentRevisions.CountAsync(r => r.ContentId == contentId);
            Assert.Equal(2, count);

            var latest = await context.ContentRevisions
                .AsNoTracking()
                .Where(r => r.ContentId == contentId)
                .OrderByDescending(r => r.VersionNumber)
                .FirstAsync();
            Assert.Equal(2, latest.VersionNumber);
            Assert.Equal("Restore test", latest.ChangeReason);

            Assert.True(await context.OutboxMessages.AnyAsync(m => m.Type == "content.updated.v1"));
        }

        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var searchScope = Factory.Services.CreateAsyncScope())
        {
            var context = searchScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var document = await context.SearchDocuments.SingleAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            Assert.Equal("Rev Title v1", document.Title);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var migrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrations.Count());
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
