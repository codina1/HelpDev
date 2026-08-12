using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests;

public sealed class ContentRevisionServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Restore_creates_new_revision_and_single_commit()
    {
        var authorId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var content = ContentEntity.Create(
            contentId,
            "Current",
            Slug.Create("current"),
            "Current body",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            Now);
        ContentWorkflowTestHelper.PromoteToPublished(content, authorId, Now);

        var snapshot = ContentRevisionSnapshot.FromContent(content);
        var revision = ContentRevision.Create(
            Guid.NewGuid(),
            contentId,
            3,
            snapshot,
            null,
            authorId,
            Now);

        var repo = new FakeContentRepository(content);
        var revisionRepo = new FakeContentRevisionRepository(revision);
        var uow = new CountingUnitOfWork();
        var service = new ContentRevisionService(repo, revisionRepo, uow, new FixedClock(Now.AddHours(1)));

        var actor = new ContentManagementActor(authorId, canManageAllContent: false);
        var result = await service.RestoreAsync(actor, contentId, 3, new RestoreContentRevisionRequest("Restore v3"), CancellationToken.None);

        Assert.Equal("Current", result.Title);
        Assert.Equal(1, uow.SaveCount);
        Assert.Equal(2, revisionRepo.Revisions.Count);
        Assert.Equal(4, revisionRepo.Revisions[^1].VersionNumber);
        Assert.Equal("Restore v3", revisionRepo.Revisions[^1].ChangeReason);
    }

    [Fact]
    public async Task Restore_cross_owner_writer_gets_not_found()
    {
        var content = ContentEntity.Create(
            Guid.NewGuid(),
            "T",
            Slug.Create("valid-slug"),
            "B",
            ContentType.Article,
            Guid.NewGuid(),
            ContentStatus.Draft,
            Now);

        var revisionRepo = new FakeContentRevisionRepository(
            ContentRevision.Create(
                Guid.NewGuid(),
                content.Id,
                1,
                ContentRevisionSnapshot.FromContent(content),
                null,
                content.AuthorId,
                Now));

        var service = new ContentRevisionService(
            new FakeContentRepository(content),
            revisionRepo,
            new CountingUnitOfWork(),
            new FixedClock(Now));

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.RestoreAsync(
                new ContentManagementActor(Guid.NewGuid(), false),
                content.Id,
                1,
                null,
                CancellationToken.None));

        Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
    }

    private sealed class FakeContentRevisionRepository : IContentRevisionRepository
    {
        private readonly ContentRevision? _existing;

        public FakeContentRevisionRepository(ContentRevision existing)
        {
            _existing = existing;
            Revisions.Add(existing);
        }

        public List<ContentRevision> Revisions { get; } = [];

        public Task AddAsync(ContentRevision revision, CancellationToken cancellationToken = default)
        {
            Revisions.Add(revision);
            return Task.CompletedTask;
        }

        public Task<ContentRevision?> GetByContentIdAndVersionAsync(
            Guid contentId,
            int versionNumber,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                _existing is not null
                && _existing.ContentId == contentId
                && _existing.VersionNumber == versionNumber
                    ? _existing
                    : Revisions.FirstOrDefault(r => r.ContentId == contentId && r.VersionNumber == versionNumber));

        public Task<int> GetMaxVersionNumberAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            var max = Revisions.Where(r => r.ContentId == contentId).Select(r => r.VersionNumber).DefaultIfEmpty(0).Max();
            return Task.FromResult(max);
        }
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        private readonly ContentEntity _content;

        public FakeContentRepository(ContentEntity content) => _content = content;

        public Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_content.Id == id ? _content : null);

        public Task<IReadOnlyList<ContentEntity>> GetPublishedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ContentEntity>>([]);

        public Task<ContentEntity?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult<ContentEntity?>(null);

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> SlugExistsForOtherAsync(string slug, Guid excludingContentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<ContentEntity> AddAsync(ContentEntity content, CancellationToken cancellationToken = default) =>
            Task.FromResult(content);
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
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
        public FixedClock(DateTime utc) => UtcNow = utc;

        public DateTime UtcNow { get; }
    }
}
