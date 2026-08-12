using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Search.Domain;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Integration.Tests.Content;

[Collection(PostgreSqlCollection.Name)]
public sealed class AdminContentManagementE2ETests : IntegrationTestClassBase
{
    public AdminContentManagementE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Admin_list_applies_filters_ordering_and_pagination()
    {
        var authorId = await SeedUserAsync("List Author");
        var otherAuthorId = await SeedUserAsync("Other Author");

        var baseTime = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        // Deterministic UpdatedAt ordering via increasing created timestamps.
        await SeedContentAsync(authorId, "Alpha Guide", "alpha-guide", ContentType.Article, ContentStatus.Draft, baseTime);
        await SeedContentAsync(authorId, "Beta News", "beta-news", ContentType.News, ContentStatus.Published, baseTime.AddDays(1));
        await SeedContentAsync(authorId, "Gamma Article", "gamma-article", ContentType.Article, ContentStatus.Published, baseTime.AddDays(2));
        await SeedContentAsync(otherAuthorId, "Other Content", "other-content", ContentType.Article, ContentStatus.Published, baseTime.AddDays(3));

        await using var scope = Factory.Services.CreateAsyncScope();
        var queries = scope.ServiceProvider.GetRequiredService<IAdminContentQueries>();

        // Ownership scope: only this author's items, ordered by UpdatedAt descending.
        var scoped = await queries.ListAsync(
            ContentSearchFilter.Create(authorId: authorId),
            CancellationToken.None);

        Assert.Equal(3, scoped.TotalCount);
        Assert.Equal(new[] { "Gamma Article", "Beta News", "Alpha Guide" }, scoped.Items.Select(i => i.Title).ToArray());

        // Status filter.
        var published = await queries.ListAsync(
            ContentSearchFilter.Create(status: nameof(ContentStatus.Published), authorId: authorId),
            CancellationToken.None);
        Assert.Equal(2, published.TotalCount);
        Assert.All(published.Items, i => Assert.Equal(nameof(ContentStatus.Published), i.ContentStatus));

        // Type filter.
        var articles = await queries.ListAsync(
            ContentSearchFilter.Create(type: nameof(ContentType.Article), authorId: authorId),
            CancellationToken.None);
        Assert.Equal(2, articles.TotalCount);
        Assert.All(articles.Items, i => Assert.Equal(nameof(ContentType.Article), i.ContentType));

        // Case-insensitive title search.
        var search = await queries.ListAsync(
            ContentSearchFilter.Create(search: "beta", authorId: authorId),
            CancellationToken.None);
        Assert.Equal(1, search.TotalCount);
        Assert.Equal("Beta News", search.Items[0].Title);

        // Pagination.
        var page1 = await queries.ListAsync(
            ContentSearchFilter.Create(page: 1, pageSize: 2, authorId: authorId),
            CancellationToken.None);
        var page2 = await queries.ListAsync(
            ContentSearchFilter.Create(page: 2, pageSize: 2, authorId: authorId),
            CancellationToken.None);
        Assert.Equal(3, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.Single(page2.Items);
    }

    [PostgreSqlFact]
    public async Task Update_published_content_writes_outbox_and_refreshes_search()
    {
        var authorId = await SeedUserAsync("Update Author");
        Guid contentId;

        // Create + publish through the application service so the outbox is populated.
        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var service = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Original Published Title",
                    Slug = "original-published",
                    Body = "Original body",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Published),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        // Edit the published content -> content.updated.v1 -> search refresh.
        await using (var updateScope = Factory.Services.CreateAsyncScope())
        {
            var service = updateScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: true);
            await service.UpdateAsync(
                actor,
                contentId,
                new UpdateContentRequest
                {
                    Title = "Edited Published Title",
                    Slug = "edited-published",
                    Type = nameof(ContentType.Article),
                    Body = "Edited body",
                },
                CancellationToken.None);
        }

        await using (var verifyScope = Factory.Services.CreateAsyncScope())
        {
            var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedMessage = await context.OutboxMessages
                .Where(m => m.Type == "content.updated.v1")
                .OrderByDescending(m => m.OccurredAtUtc)
                .FirstOrDefaultAsync();
            Assert.NotNull(updatedMessage);
        }

        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var searchScope = Factory.Services.CreateAsyncScope())
        {
            var context = searchScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var searchDocument = await context.SearchDocuments.SingleOrDefaultAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);

            Assert.NotNull(searchDocument);
            Assert.Equal("edited-published", searchDocument!.Slug);
            Assert.Equal("Edited Published Title", searchDocument.Title);
            Assert.True(searchDocument.IsPublished);
        }
    }

    [PostgreSqlFact]
    public async Task Update_seo_of_published_content_writes_outbox_and_refreshes_search()
    {
        var authorId = await SeedUserAsync("Seo Author");
        Guid contentId;

        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var service = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Seo Published Title",
                    Slug = "seo-published",
                    Body = "Seo body",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Published),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);

        DateTime updatedAtAfterPublish;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var document = await context.SearchDocuments.SingleAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            updatedAtAfterPublish = document.SourceUpdatedAtUtc;
        }

        // Small delay so the SEO update timestamp is strictly greater than publish time.
        await Task.Delay(10);

        AdminContentDetailDto seoResult;
        await using (var seoScope = Factory.Services.CreateAsyncScope())
        {
            var service = seoScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            seoResult = await service.UpdateSeoMetadataAsync(
                actor,
                contentId,
                new UpdateSeoMetadataRequest
                {
                    SeoTitle = "Optimized SEO Title",
                    SeoDescription = "Optimized meta description for search engines.",
                    CanonicalUrl = "https://helpdev.example/articles/seo-published",
                    OgImage = "https://cdn.helpdev.example/seo-published.png",
                    FocusKeyword = "helpdev seo",
                },
                CancellationToken.None);
        }

        Assert.Equal("Optimized SEO Title", seoResult.Seo.SeoTitle);
        Assert.Equal("https://helpdev.example/articles/seo-published", seoResult.Seo.CanonicalUrl);

        // Reusing the existing content.updated.v1 event -> outbox -> search refresh.
        await using (var verifyScope = Factory.Services.CreateAsyncScope())
        {
            var context = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedMessage = await context.OutboxMessages
                .Where(m => m.Type == "content.updated.v1")
                .OrderByDescending(m => m.OccurredAtUtc)
                .FirstOrDefaultAsync();
            Assert.NotNull(updatedMessage);
        }

        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var searchScope = Factory.Services.CreateAsyncScope())
        {
            var context = searchScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var document = await context.SearchDocuments.SingleAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);

            Assert.True(document.IsPublished);
            Assert.True(
                document.SourceUpdatedAtUtc > updatedAtAfterPublish,
                "SEO update on published content should refresh the search projection timestamp.");
        }

        // SEO fields must never leak into the public search projection.
        await using (var leakScope = Factory.Services.CreateAsyncScope())
        {
            var context = leakScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var document = await context.SearchDocuments.SingleAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);

            Assert.DoesNotContain("Optimized SEO Title", document.Title, StringComparison.Ordinal);
            Assert.DoesNotContain("helpdev seo", document.Summary, StringComparison.Ordinal);
        }
    }

    [PostgreSqlFact]
    public async Task Admin_get_by_id_returns_full_detail_including_seo_and_publish_timestamp()
    {
        var authorId = await SeedUserAsync("Detail Author");
        Guid contentId;

        // Create as Draft via the application service.
        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var service = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Draft Detail Title",
                    Slug = "draft-detail",
                    Body = "Draft detail body",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        // Attach SEO metadata while still a draft.
        await using (var seoScope = Factory.Services.CreateAsyncScope())
        {
            var service = seoScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            await service.UpdateSeoMetadataAsync(
                actor,
                contentId,
                new UpdateSeoMetadataRequest
                {
                    SeoTitle = "Draft SEO Title",
                    SeoDescription = "Draft SEO description",
                    CanonicalUrl = "https://helpdev.example/draft-detail",
                    OgImage = "https://cdn.helpdev.example/draft.png",
                    FocusKeyword = "draft keyword",
                },
                CancellationToken.None);
        }

        // Admin GET by id (via the owned read model) must return full detail + SEO for a draft.
        AdminContentDetailDto draftDetail;
        await using (var readScope = Factory.Services.CreateAsyncScope())
        {
            var service = readScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            draftDetail = await service.GetManagedByIdAsync(actor, contentId, CancellationToken.None);
        }

        Assert.Equal(contentId, draftDetail.Id);
        Assert.Equal("Draft Detail Title", draftDetail.Title);
        Assert.Equal("draft-detail", draftDetail.Slug);
        Assert.Equal("Draft detail body", draftDetail.Body);
        Assert.Equal(nameof(ContentStatus.Draft), draftDetail.ContentStatus);
        Assert.Null(draftDetail.PublishedAtUtc);
        Assert.Equal("Draft SEO Title", draftDetail.Seo.SeoTitle);
        Assert.Equal("Draft SEO description", draftDetail.Seo.SeoDescription);
        Assert.Equal("https://helpdev.example/draft-detail", draftDetail.Seo.CanonicalUrl);
        Assert.Equal("https://cdn.helpdev.example/draft.png", draftDetail.Seo.OgImage);
        Assert.Equal("draft keyword", draftDetail.Seo.FocusKeyword);

        // Query port directly (AsNoTracking projection) also returns the same SEO fields.
        await using (var queryScope = Factory.Services.CreateAsyncScope())
        {
            var queries = queryScope.ServiceProvider.GetRequiredService<IAdminContentQueries>();
            var projected = await queries.GetByIdAsync(contentId, CancellationToken.None);
            Assert.NotNull(projected);
            Assert.Equal("Draft SEO Title", projected!.Seo.SeoTitle);

            var bySlug = await queries.GetBySlugAsync("draft-detail", CancellationToken.None);
            Assert.NotNull(bySlug);
            Assert.Equal(contentId, bySlug!.Id);
        }

        // Publish → Admin GET must expose PublishedAtUtc.
        await using (var publishScope = Factory.Services.CreateAsyncScope())
        {
            var service = publishScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            await service.PublishAsync(actor, contentId, CancellationToken.None);
        }

        AdminContentDetailDto publishedDetail;
        await using (var readPublishedScope = Factory.Services.CreateAsyncScope())
        {
            var service = readPublishedScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: true);
            publishedDetail = await service.GetManagedByIdAsync(actor, contentId, CancellationToken.None);
        }

        Assert.Equal(nameof(ContentStatus.Published), publishedDetail.ContentStatus);
        Assert.NotNull(publishedDetail.PublishedAtUtc);
        Assert.Equal("Draft SEO Title", publishedDetail.Seo.SeoTitle);

        // Cross-owner writer must not observe existence.
        await using (var forbiddenScope = Factory.Services.CreateAsyncScope())
        {
            var service = forbiddenScope.ServiceProvider.GetRequiredService<IContentService>();
            var stranger = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);
            var ex = await Assert.ThrowsAsync<ContentException>(() =>
                service.GetManagedByIdAsync(stranger, contentId, CancellationToken.None));
            Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
        }
    }

    [PostgreSqlFact]
    public async Task Seo_analysis_is_side_effect_free_and_ownership_aware()
    {
        var authorId = await SeedUserAsync("Seo Analyzer Author");
        Guid contentId;
        DateTime updatedAtBefore;
        int outboxCountBefore;

        await using (var createScope = Factory.Services.CreateAsyncScope())
        {
            var service = createScope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await service.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "ASP.NET Core Basics Analyzer",
                    Slug = "aspnet-core-basics-analyzer",
                    Body = """
                        ASP.NET Core Basics opens this article for developers.

                        ## ASP.NET Core Basics Overview

                        More guidance on ASP.NET Core Basics with [docs](/docs) and [ext](https://example.com).

                        ```csharp
                        Console.WriteLine(1);
                        ```

                        ```
                        unlabelled
                        ```
                        """,
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                },
                CancellationToken.None);
            contentId = created.Id;
        }

        await using (var seoScope = Factory.Services.CreateAsyncScope())
        {
            var service = seoScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            await service.UpdateSeoMetadataAsync(
                actor,
                contentId,
                new UpdateSeoMetadataRequest
                {
                    SeoTitle = "ASP.NET Core Basics Guide for Developers",
                    SeoDescription = "ASP.NET Core Basics explained for developers with practical examples and guidance.",
                    CanonicalUrl = "https://helpdev.example/aspnet-core-basics-analyzer",
                    OgImage = "https://cdn.helpdev.example/og.png",
                    FocusKeyword = "ASP.NET Core Basics",
                },
                CancellationToken.None);
        }

        await using (var updateCoverScope = Factory.Services.CreateAsyncScope())
        {
            var service = updateCoverScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            await service.UpdateAsync(
                actor,
                contentId,
                new UpdateContentRequest
                {
                    Title = "ASP.NET Core Basics Analyzer",
                    Slug = "aspnet-core-basics-analyzer",
                    Type = nameof(ContentType.Article),
                    Body = """
                        ASP.NET Core Basics opens this article for developers.

                        ## ASP.NET Core Basics Overview

                        More guidance on ASP.NET Core Basics with [docs](/docs) and [ext](https://example.com).

                        ```csharp
                        Console.WriteLine(1);
                        ```

                        ```
                        unlabelled
                        ```
                        """,
                    Excerpt = "ASP.NET Core Basics excerpt covering fundamentals for SEO analysis.",
                    CoverImage = "https://cdn.helpdev.example/cover.png",
                },
                CancellationToken.None);
        }

        await using (var beforeScope = Factory.Services.CreateAsyncScope())
        {
            var context = beforeScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var row = await context.Contents.AsNoTracking().SingleAsync(c => c.Id == contentId);
            updatedAtBefore = row.UpdatedAt;
            outboxCountBefore = await context.OutboxMessages.CountAsync();
        }

        SeoAuditReportDto draftReport;
        await using (var analyzeScope = Factory.Services.CreateAsyncScope())
        {
            var service = analyzeScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            draftReport = await service.AnalyzeSeoAsync(actor, contentId, CancellationToken.None);
        }

        Assert.Equal(contentId, draftReport.ContentId);
        Assert.Contains(draftReport.Findings, f => f.RuleId == "seo.title.missing" || f.RuleId.StartsWith("seo.title.", StringComparison.Ordinal));
        Assert.Contains(draftReport.Findings, f => f.RuleId.StartsWith("seo.", StringComparison.Ordinal));
        Assert.DoesNotContain(
            typeof(SeoAuditReportDto).GetProperties().Select(p => p.Name),
            n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));

        await using (var afterScope = Factory.Services.CreateAsyncScope())
        {
            var context = afterScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var row = await context.Contents.AsNoTracking().SingleAsync(c => c.Id == contentId);
            Assert.Equal(updatedAtBefore, row.UpdatedAt);
            Assert.Equal(outboxCountBefore, await context.OutboxMessages.CountAsync());
            var migrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrations.Count());
        }

        await using (var publishScope = Factory.Services.CreateAsyncScope())
        {
            var service = publishScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: false);
            await service.PublishAsync(actor, contentId, CancellationToken.None);
        }

        await using (var analyzePublishedScope = Factory.Services.CreateAsyncScope())
        {
            var service = analyzePublishedScope.ServiceProvider.GetRequiredService<IContentService>();
            var actor = new ContentManagementActor(authorId, canManageAllContent: true);
            var publishedReport = await service.AnalyzeSeoAsync(actor, contentId, CancellationToken.None);
            Assert.NotEmpty(publishedReport.Findings);
        }

        await using (var forbiddenScope = Factory.Services.CreateAsyncScope())
        {
            var service = forbiddenScope.ServiceProvider.GetRequiredService<IContentService>();
            var stranger = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);
            var ex = await Assert.ThrowsAsync<ContentException>(() =>
                service.AnalyzeSeoAsync(stranger, contentId, CancellationToken.None));
            Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
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

    private async Task SeedContentAsync(
        Guid authorId,
        string title,
        string slug,
        ContentType type,
        ContentStatus status,
        DateTime createdAtUtc)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var content = ContentEntity.Create(
            Guid.NewGuid(),
            title,
            Slug.Create(slug),
            $"{title} body",
            type,
            authorId,
            status,
            createdAtUtc);

        context.Contents.Add(content);
        await context.SaveChangesAsync();
    }
}
