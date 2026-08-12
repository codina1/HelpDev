using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Content.Tests;

public sealed class ContentWorkflowServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 22, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Writer_can_submit_own_content()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.Draft);
        var service = CreateService(content, out var uow, out var transitions);

        var result = await service.SubmitForReviewAsync(
            new ContentManagementActor(authorId, canManageAllContent: false),
            content.Id,
            CancellationToken.None);

        Assert.Equal(nameof(ContentStatus.ReviewPending), result.ContentStatus);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single(transitions.Items);
    }

    [Fact]
    public async Task Writer_cannot_approve()
    {
        var authorId = Guid.NewGuid();
        var content = CreateContent(authorId, ContentStatus.ReviewPending);
        var service = CreateService(content, out _, out _);

        var ex = await Assert.ThrowsAsync<ContentException>(() =>
            service.ApproveAsync(new ContentManagementActor(authorId, false), content.Id, CancellationToken.None));

        Assert.Equal(ContentErrorCodes.OperationInvalid, ex.Code);
    }

    [Fact]
    public async Task Admin_publish_from_approved_records_transition()
    {
        var content = CreateContent(Guid.NewGuid(), ContentStatus.Approved);
        var service = CreateService(content, out var uow, out var transitions);

        await service.PublishAsync(new ContentManagementActor(Guid.NewGuid(), true), content.Id, CancellationToken.None);

        Assert.Equal(1, uow.SaveCount);
        Assert.Single(transitions.Items);
        Assert.Equal(ContentStatus.Published, content.Status);
    }

    private static ContentWorkflowService CreateService(
        ContentEntity content,
        out CountingUnitOfWork uow,
        out RecordingTransitionRepository transitions)
    {
        uow = new CountingUnitOfWork();
        transitions = new RecordingTransitionRepository();
        return new ContentWorkflowService(
            new WorkflowFakeContentRepository(content),
            transitions,
            new FakeWorkflowQueries(),
            uow,
            new FixedClock());
    }

    private sealed class WorkflowFakeContentRepository : IContentRepository
    {
        private readonly ContentEntity _content;

        public WorkflowFakeContentRepository(ContentEntity content) => _content = content;

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

    private static ContentEntity CreateContent(Guid authorId, ContentStatus status)
    {
        var content = ContentEntity.Create(
            Guid.NewGuid(),
            "T",
            Slug.Create("workflow-slug"),
            "B",
            ContentType.Article,
            authorId,
            ContentStatus.Draft,
            Now);

        if (status == ContentStatus.ReviewPending)
        {
            content.SubmitForReview(authorId, Now);
        }
        else if (status == ContentStatus.Approved)
        {
            content.SubmitForReview(authorId, Now);
            content.Approve(authorId, Now);
        }

        return content;
    }

    private sealed class FakeWorkflowQueries : IContentWorkflowQueries
    {
        public Task<WorkflowHistoryDto> GetHistoryAsync(
            ContentManagementActor actor,
            Guid contentId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new WorkflowHistoryDto([]));
    }

    private sealed class RecordingTransitionRepository : IContentWorkflowTransitionRepository
    {
        public List<ContentWorkflowTransition> Items { get; } = [];

        public Task AddAsync(ContentWorkflowTransition transition, CancellationToken cancellationToken = default)
        {
            Items.Add(transition);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(
            IReadOnlyList<ContentWorkflowTransition> transitionList,
            CancellationToken cancellationToken = default)
        {
            Items.AddRange(transitionList);
            return Task.CompletedTask;
        }
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
        public DateTime UtcNow => Now;
    }
}
