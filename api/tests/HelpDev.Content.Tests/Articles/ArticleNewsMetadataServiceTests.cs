using HelpDev.Modules.Content.Application.Articles;
using HelpDev.Modules.Content.Application.Articles.Dtos;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.News;
using HelpDev.Modules.Content.Application.News.Dtos;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.News;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests.Articles;

public sealed class ArticleNewsMetadataServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Article_create_by_owner_persists_metadata()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentType.Article);
        var (service, metaRepo, uow) = CreateArticleService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var dto = await service.CreateAsync(
            actor,
            content.Id,
            new UpdateArticleMetadataRequest
            {
                DifficultyLevel = "Intermediate",
                ReadingTimeMinutes = 9,
                IsFeatured = true,
                AllowComments = true,
                TableOfContentsEnabled = true,
            });

        Assert.Equal(content.Id, dto.ContentId);
        Assert.Equal("Intermediate", dto.DifficultyLevel);
        Assert.Equal(9, dto.ReadingTimeMinutes);
        Assert.True(dto.IsFeatured);
        Assert.Equal(1, metaRepo.Items.Count);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task Article_update_masks_cross_owner_as_not_found()
    {
        var content = CreateContent(Guid.NewGuid(), ContentType.Article);
        var (service, _, _) = CreateArticleService(content);
        var stranger = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.UpdateAsync(
                stranger,
                content.Id,
                new UpdateArticleMetadataRequest { ReadingTimeMinutes = 3, DifficultyLevel = "Beginner" }));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task News_create_requires_news_content_type()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentType.Article);
        var (service, _, _) = CreateNewsService(content);
        var actor = new ContentManagementActor(authorId, canManageAllContent: false);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.CreateAsync(
                actor,
                content.Id,
                new UpdateNewsMetadataRequest
                {
                    SourceName = "Wire",
                    Priority = "Normal",
                    NewsDateUtc = Now,
                }));

        Assert.Equal(ContentErrorCodes.Validation, ex.Code);
    }

    [Fact]
    public async Task News_admin_can_update_any_news_item()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentType.News);
        var existing = NewsMetadata.Create(
            Guid.NewGuid(),
            content.Id,
            "Wire",
            "https://helpdev.example/s",
            Now,
            NewsPriority.Normal,
            null,
            Now);
        var (service, _, uow) = CreateNewsService(content, existing);
        var admin = new ContentManagementActor(Guid.NewGuid(), canManageAllContent: true);

        var dto = await service.UpdateAsync(
            admin,
            content.Id,
            new UpdateNewsMetadataRequest
            {
                SourceName = "Agency",
                SourceUrl = "https://helpdev.example/a",
                Priority = "Breaking",
                NewsDateUtc = Now,
            });

        Assert.Equal("Agency", dto.SourceName);
        Assert.Equal("Breaking", dto.Priority);
        Assert.Equal(1, uow.SaveCount);
    }

    private static (ArticleMetadataService, FakeArticleRepo, FakeUnitOfWork) CreateArticleService(
        ContentEntity content)
    {
        var contentRepo = new FakeContentRepo(content);
        var metaRepo = new FakeArticleRepo();
        var uow = new FakeUnitOfWork();
        var service = new ArticleMetadataService(contentRepo, metaRepo, uow, new FixedClock(Now));
        return (service, metaRepo, uow);
    }

    private static (NewsMetadataService, FakeNewsRepo, FakeUnitOfWork) CreateNewsService(
        ContentEntity content,
        NewsMetadata? existing = null)
    {
        var contentRepo = new FakeContentRepo(content);
        var metaRepo = new FakeNewsRepo(existing);
        var uow = new FakeUnitOfWork();
        var service = new NewsMetadataService(contentRepo, metaRepo, uow, new FixedClock(Now));
        return (service, metaRepo, uow);
    }

    private static ContentEntity CreateContent(Guid authorId, ContentType type) =>
        ContentEntity.Create(
            Guid.NewGuid(),
            "Title",
            Slug.Create("title-" + Guid.NewGuid().ToString("N")[..8]),
            "Body",
            type,
            authorId,
            ContentStatus.Draft,
            Now);

    private sealed class FakeContentRepo(ContentEntity existing) : IContentRepository
    {
        public Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentEntity>>([]);

        public Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentEntity?>(null);

        public Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(existing.Id == id ? existing : null);

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default) =>
            Task.FromResult(content);
    }

    private sealed class FakeArticleRepo : IArticleMetadataRepository
    {
        public List<ArticleMetadata> Items { get; } = [];

        public Task<ArticleMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(x => x.ContentId == contentId));

        public Task AddAsync(ArticleMetadata metadata, CancellationToken cancellationToken = default)
        {
            Items.Add(metadata);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNewsRepo(NewsMetadata? existing) : INewsMetadataRepository
    {
        private NewsMetadata? _existing = existing;

        public Task<NewsMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_existing is not null && _existing.ContentId == contentId ? _existing : null);

        public Task AddAsync(NewsMetadata metadata, CancellationToken cancellationToken = default)
        {
            _existing = metadata;
            return Task.CompletedTask;
        }
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

    private sealed class FixedClock(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = now;
    }
}
