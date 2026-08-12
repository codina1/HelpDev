using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests;

public sealed class ContentServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task UpdateAsync_by_owner_updates_fields_and_returns_detail()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, repo, uow, _, revisions) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var result = await service.UpdateAsync(
            actor,
            content.Id,
            new UpdateContentRequest
            {
                Title = "Updated Title",
                Slug = "updated-slug",
                Type = nameof(ContentType.News),
                Body = "Updated body",
                Excerpt = "Updated excerpt",
            },
            CancellationToken.None);

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("updated-slug", result.Slug);
        Assert.Equal(nameof(ContentType.News), result.ContentType);
        Assert.Equal("Updated excerpt", result.Excerpt);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(1, revisions.AppendCount);
    }

    [Fact]
    public async Task UpdateAsync_no_op_does_not_append_revision()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, _, uow, _, revisions) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        await service.UpdateAsync(
            actor,
            content.Id,
            new UpdateContentRequest
            {
                Title = content.Title,
                Slug = content.Slug.Value,
                Type = content.Type.ToString(),
                Body = content.Body,
                Excerpt = content.Excerpt,
                CoverImage = content.CoverImage,
            },
            CancellationToken.None);

        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(0, revisions.AppendCount);
    }

    [Fact]
    public async Task UpdateAsync_by_non_owner_writer_throws_not_found()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateAsync(
            actor,
            content.Id,
            ValidUpdate(),
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task UpdateAsync_by_admin_on_other_authors_content_is_allowed()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var result = await service.UpdateAsync(actor, content.Id, ValidUpdate(), CancellationToken.None);

        Assert.Equal("valid-slug", result.Slug);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_missing_content_throws_not_found()
    {
        var (service, _, _, _, _) = CreateService(existing: null);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateAsync(
            actor,
            Guid.NewGuid(),
            ValidUpdate(),
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task UpdateAsync_with_duplicate_slug_throws_conflict()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, repo, _, _, _) = CreateService(content);
        repo.SlugTakenByOthers = true;
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateAsync(
            actor,
            content.Id,
            ValidUpdate(),
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.SlugDuplicate, ex.Code);
    }

    [Fact]
    public async Task UpdateAsync_on_published_content_queues_update_event()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Published);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        await service.UpdateAsync(actor, content.Id, ValidUpdate(), CancellationToken.None);

        Assert.Contains(content.DomainEvents, e => e is ContentUpdatedDomainEvent);
    }

    [Fact]
    public async Task UpdateAsync_on_draft_content_queues_no_event()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        await service.UpdateAsync(actor, content.Id, ValidUpdate(), CancellationToken.None);

        Assert.DoesNotContain(content.DomainEvents, e => e is ContentUpdatedDomainEvent);
    }

    [Fact]
    public async Task PublishAsync_transitions_approved_to_published_and_queues_event()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Approved);
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        var result = await service.PublishAsync(actor, content.Id, CancellationToken.None);

        Assert.Equal(nameof(ContentStatus.Published), result.ContentStatus);
        Assert.NotNull(result.PublishedAtUtc);
        Assert.Equal(1, uow.SaveCount);
        Assert.Contains(content.DomainEvents, e => e is ContentPublishedDomainEvent);
    }

    [Fact]
    public async Task PublishAsync_by_non_owner_writer_throws_not_found()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.PublishAsync(actor, content.Id, CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_by_owner_updates_and_commits_once()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var result = await service.UpdateSeoMetadataAsync(
            actor,
            content.Id,
            new UpdateSeoMetadataRequest
            {
                SeoTitle = "Great SEO title",
                SeoDescription = "A concise meta description.",
                CanonicalUrl = "https://helpdev.example/articles/great",
                OgImage = "https://cdn.helpdev.example/og.png",
                FocusKeyword = "helpdev",
            },
            CancellationToken.None);

        Assert.Equal("Great SEO title", result.Seo.SeoTitle);
        Assert.Equal("A concise meta description.", result.Seo.SeoDescription);
        Assert.Equal("https://helpdev.example/articles/great", result.Seo.CanonicalUrl);
        Assert.Equal("https://cdn.helpdev.example/og.png", result.Seo.OgImage);
        Assert.Equal("helpdev", result.Seo.FocusKeyword);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_by_non_owner_writer_throws_not_found()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateSeoMetadataAsync(
            actor,
            content.Id,
            new UpdateSeoMetadataRequest { SeoTitle = "x" },
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_missing_content_throws_not_found()
    {
        var (service, _, _, _, _) = CreateService(existing: null);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateSeoMetadataAsync(
            actor,
            Guid.NewGuid(),
            new UpdateSeoMetadataRequest { SeoTitle = "x" },
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_invalid_url_throws_operation_invalid()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() => service.UpdateSeoMetadataAsync(
            actor,
            content.Id,
            new UpdateSeoMetadataRequest { CanonicalUrl = "not-a-url" },
            CancellationToken.None));

        Assert.Equal(ContentErrorCodes.OperationInvalid, ex.Code);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_on_published_content_queues_update_event()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Published);
        content.ClearDomainEvents();
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        await service.UpdateSeoMetadataAsync(
            actor,
            content.Id,
            new UpdateSeoMetadataRequest { SeoTitle = "Published SEO" },
            CancellationToken.None);

        Assert.Contains(content.DomainEvents, e => e is ContentUpdatedDomainEvent);
    }

    [Fact]
    public async Task UpdateSeoMetadataAsync_on_draft_content_queues_no_event()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);

        await service.UpdateSeoMetadataAsync(
            actor,
            content.Id,
            new UpdateSeoMetadataRequest { SeoTitle = "Draft SEO" },
            CancellationToken.None);

        Assert.DoesNotContain(content.DomainEvents, e => e is ContentUpdatedDomainEvent);
    }

    [Fact]
    public async Task GetManagedByIdAsync_by_owner_returns_full_admin_detail()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        content.UpdateSeoMetadata(
            SeoMetadata.Create("SEO Title", "SEO Description", null, null, "kw"),
            Now);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var result = await service.GetManagedByIdAsync(actor, content.Id, CancellationToken.None);

        Assert.Equal(content.Id, result.Id);
        Assert.Equal(content.Title, result.Title);
        Assert.Equal(content.Slug.Value, result.Slug);
        Assert.Equal(content.Body, result.Body);
        Assert.Equal(nameof(ContentStatus.Draft), result.ContentStatus);
        Assert.Equal("SEO Title", result.Seo.SeoTitle);
        Assert.Equal("SEO Description", result.Seo.SeoDescription);
        Assert.Equal("kw", result.Seo.FocusKeyword);
        Assert.Equal(authorId, result.AuthorId);
    }

    [Fact]
    public async Task GetManagedByIdAsync_by_non_owner_writer_throws_not_found()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.GetManagedByIdAsync(actor, content.Id, CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task GetManagedByIdAsync_by_admin_on_other_authors_content_is_allowed()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Published);
        var (service, _, _, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var result = await service.GetManagedByIdAsync(actor, content.Id, CancellationToken.None);

        Assert.Equal(content.Id, result.Id);
        Assert.Equal(nameof(ContentStatus.Published), result.ContentStatus);
    }

    [Fact]
    public async Task GetManagedByIdAsync_missing_content_throws_not_found()
    {
        var (service, _, _, _, _) = CreateService(existing: null);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.GetManagedByIdAsync(actor, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task AnalyzeSeoAsync_writer_own_content_returns_report_without_side_effects()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        content.ClearDomainEvents();
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var report = await service.AnalyzeSeoAsync(actor, content.Id, CancellationToken.None);

        Assert.Equal(content.Id, report.ContentId);
        Assert.NotEmpty(report.Findings);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public async Task AnalyzeSeoAsync_writer_cross_owner_throws_not_found_without_save()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Draft);
        content.ClearDomainEvents();
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.AnalyzeSeoAsync(actor, content.Id, CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public async Task AnalyzeSeoAsync_admin_cross_owner_is_allowed_without_side_effects()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Published);
        content.ClearDomainEvents();
        var (service, _, uow, _, _) = CreateService(content);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var report = await service.AnalyzeSeoAsync(actor, content.Id, CancellationToken.None);

        Assert.Equal(content.Id, report.ContentId);
        Assert.NotEmpty(report.Findings);
        Assert.Equal(0, uow.SaveCount);
        Assert.Empty(content.DomainEvents);
    }

    [Fact]
    public async Task AnalyzeSeoAsync_missing_content_throws_not_found()
    {
        var (service, _, uow, _, _) = CreateService(existing: null);
        var actor = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.AnalyzeSeoAsync(actor, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
        Assert.Equal(0, uow.SaveCount);
    }

    private static UpdateContentRequest ValidUpdate() =>
        new()
        {
            Title = "Valid Title",
            Slug = "valid-slug",
            Type = nameof(ContentType.Article),
            Body = "Valid body",
        };

    private static ContentEntity CreateContent(Guid authorId, ContentStatus status) =>
        CreateContentWithStatus(authorId, status);

    private static ContentEntity CreateContentWithStatus(Guid authorId, ContentStatus status)
    {
        var content = ContentEntity.Create(
            Guid.NewGuid(),
            "Original Title",
            Slug.Create("original-slug"),
            "Original body",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            Now.AddDays(-1));

        if (status is ContentStatus.ReviewPending or ContentStatus.Approved or ContentStatus.Published or ContentStatus.Archived)
        {
            content.SubmitForReview(authorId, Now);
        }

        if (status is ContentStatus.Approved or ContentStatus.Published or ContentStatus.Archived)
        {
            content.Approve(authorId, Now);
        }

        if (status is ContentStatus.Published or ContentStatus.Archived)
        {
            content.Publish(authorId, Now);
        }

        if (status == ContentStatus.Archived)
        {
            content.Archive(authorId, Now);
        }

        return content;
    }

    private static (ContentService Service, FakeContentRepository Repo, FakeUnitOfWork Uow, FakeAdminContentQueries Queries, FakeContentRevisionService Revisions) CreateService(
        ContentEntity? existing)
    {
        var repo = new FakeContentRepository(existing);
        var uow = new FakeUnitOfWork();
        var queries = new FakeAdminContentQueries(existing);
        var revisions = new FakeContentRevisionService();
        var transitions = new WorkflowTransitionRecorder();
        var workflow = new ContentWorkflowService(
            repo,
            transitions,
            new FakeWorkflowQueries(),
            uow,
            new FixedClock(Now));
        var service = new ContentService(
            repo,
            queries,
            new ContentSeoAnalyzer(),
            revisions,
            workflow,
            uow,
            new FixedClock(Now),
            new NoOpAnalyticsIngestor(),
            NullLogger<ContentService>.Instance);
        return (service, repo, uow, queries, revisions);
    }

    private sealed class FakeAdminContentQueries : IAdminContentQueries
    {
        private readonly ContentEntity? _existing;

        public FakeAdminContentQueries(ContentEntity? existing) => _existing = existing;

        public Task<PagedResult<AdminContentListItemDto>> ListAsync(
            ContentSearchFilter filter,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<AdminContentListItemDto>([], 1, 20, 0));

        public Task<AdminContentDetailDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (_existing is null || _existing.Id != id)
            {
                return Task.FromResult<AdminContentDetailDto?>(null);
            }

            return Task.FromResult<AdminContentDetailDto?>(Map(_existing));
        }

        public Task<AdminContentDetailDto?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default)
        {
            if (_existing is null || _existing.Slug.Value != slug)
            {
                return Task.FromResult<AdminContentDetailDto?>(null);
            }

            return Task.FromResult<AdminContentDetailDto?>(Map(_existing));
        }

        private static AdminContentDetailDto Map(ContentEntity content) =>
            new(
                content.Id,
                content.Title,
                content.Slug.Value,
                content.Body,
                content.Excerpt,
                content.CoverImage,
                content.Type.ToString(),
                content.Status.ToString(),
                content.AuthorId,
                content.Views,
                content.Saves,
                content.CreatedAt,
                content.UpdatedAt,
                content.PublishedAtUtc,
                new SeoMetadataDto(
                    content.SeoMetadata.SeoTitle,
                    content.SeoMetadata.SeoDescription,
                    content.SeoMetadata.CanonicalUrl,
                    content.SeoMetadata.OgImage,
                    content.SeoMetadata.FocusKeyword));
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly ContentEntity? _existing;

        public FakeContentRepository(ContentEntity? existing) => _existing = existing;

        public bool SlugTakenByOthers { get; set; }

        public Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentEntity>>([]);

        public Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentEntity?>(null);

        public Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_existing is not null && _existing.Id == id ? _existing : null);

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(SlugTakenByOthers);

        public Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default) =>
            Task.FromResult(content);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public FixedClock(DateTime now) => UtcNow = now;

        public DateTime UtcNow { get; }
    }

    private sealed class NoOpAnalyticsIngestor : IAnalyticsEventIngestor
    {
        public Task IngestAsync(AnalyticsEventEnvelope analyticsEvent, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeContentRevisionService : IContentRevisionService
    {
        public int AppendCount { get; private set; }

        public Task AppendRevisionAsync(
            ContentEntity content,
            Guid createdByUserId,
            string? changeReason,
            CancellationToken cancellationToken = default)
        {
            AppendCount++;
            return Task.CompletedTask;
        }

        public Task<AdminContentDetailDto> RestoreAsync(
            ContentManagementActor actor,
            Guid contentId,
            int versionNumber,
            RestoreContentRevisionRequest? request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeWorkflowQueries : IContentWorkflowQueries
    {
        public Task<WorkflowHistoryDto> GetHistoryAsync(
            ContentManagementActor actor,
            Guid contentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new WorkflowHistoryDto([]));
    }

    private sealed class WorkflowTransitionRecorder : IContentWorkflowTransitionRepository
    {
        public Task AddAsync(ContentWorkflowTransition transition, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AddRangeAsync(
            IReadOnlyList<ContentWorkflowTransition> transitions,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}

public sealed class ContentSearchFilterTests
{
    [Fact]
    public void Create_applies_defaults_when_missing()
    {
        var filter = ContentSearchFilter.Create();

        Assert.Equal(ContentSearchFilter.DefaultPage, filter.Page);
        Assert.Equal(ContentSearchFilter.DefaultPageSize, filter.PageSize);
        Assert.Null(filter.Search);
        Assert.Null(filter.Status);
        Assert.Null(filter.Type);
        Assert.Null(filter.AuthorId);
    }

    [Fact]
    public void Create_clamps_page_and_page_size()
    {
        var lower = ContentSearchFilter.Create(page: 0, pageSize: 0);
        Assert.Equal(1, lower.Page);
        Assert.Equal(ContentSearchFilter.DefaultPageSize, lower.PageSize);

        var upper = ContentSearchFilter.Create(page: 3, pageSize: 5000);
        Assert.Equal(3, upper.Page);
        Assert.Equal(ContentSearchFilter.MaxPageSize, upper.PageSize);
    }

    [Fact]
    public void Create_trims_blank_filters_to_null()
    {
        var filter = ContentSearchFilter.Create(search: "   ", status: "", type: "  ");

        Assert.Null(filter.Search);
        Assert.Null(filter.Status);
        Assert.Null(filter.Type);
    }

    [Fact]
    public void Create_preserves_author_scope()
    {
        var authorId = Guid.NewGuid();
        var filter = ContentSearchFilter.Create(authorId: authorId);

        Assert.Equal(authorId, filter.AuthorId);
    }
}
