using HelpDev.Analytics.Application.Tests.Fakes;
using HelpDev.Modules.Analytics.Application;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Analytics.Application.Tests;

public sealed class AnalyticsEventProcessorTests
{
    private static readonly DateTime Now = new(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);

    private static AnalyticsEventEnvelope LoginEvent(Guid? actorUserId = null) =>
        new(
            EventId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.IdentityUserLoginSucceeded,
            OccurredAtUtc: Now,
            ActorUserId: actorUserId ?? Guid.NewGuid(),
            SubjectId: null,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

    private static AnalyticsEventEnvelope SimpleEvent(Guid? actorUserId = null) =>
        new(
            EventId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.LearningLessonCompleted,
            OccurredAtUtc: Now,
            ActorUserId: actorUserId,
            SubjectId: null,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

    [Fact]
    public async Task ProcessAsync_commits_on_first_event()
    {
        var (processor, _, _, _, _, unitOfWork, _) = ProcessorFactory.Create();
        var envelope = LoginEvent();

        var result = await processor.ProcessAsync(envelope);

        Assert.False(result.WasDuplicate);
        Assert.True(result.Committed);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task ProcessAsync_is_idempotent_no_second_commit_on_duplicate()
    {
        var (processor, receiptRepo, _, _, _, unitOfWork, _) = ProcessorFactory.Create();
        var envelope = LoginEvent();

        receiptRepo.SeedExisting(envelope.EventId);

        var result = await processor.ProcessAsync(envelope);

        Assert.True(result.WasDuplicate);
        Assert.False(result.Committed);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task ProcessAsync_second_call_with_same_id_is_duplicate()
    {
        var (processor, _, _, _, _, unitOfWork, _) = ProcessorFactory.Create();
        var envelope = LoginEvent();

        await processor.ProcessAsync(envelope);
        var result = await processor.ProcessAsync(envelope);

        Assert.True(result.WasDuplicate);
        Assert.False(result.Committed);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task ProcessAsync_adds_receipt_to_repository()
    {
        var (processor, receiptRepo, _, _, _, _, _) = ProcessorFactory.Create();
        var envelope = LoginEvent();

        await processor.ProcessAsync(envelope);

        Assert.Single(receiptRepo.Receipts);
        Assert.Equal(envelope.EventId, receiptRepo.Receipts[0].EventId);
    }

    [Fact]
    public async Task ProcessAsync_creates_metric()
    {
        var (processor, _, metricRepo, _, _, _, _) = ProcessorFactory.Create();

        await processor.ProcessAsync(SimpleEvent());

        Assert.NotEmpty(metricRepo.Metrics);
    }

    [Fact]
    public async Task ProcessAsync_marks_active_user_for_authenticated_login()
    {
        var (processor, _, _, activeUserRepo, _, _, _) = ProcessorFactory.Create();
        var userId = Guid.NewGuid();
        var envelope = LoginEvent(actorUserId: userId);

        await processor.ProcessAsync(envelope);

        Assert.Single(activeUserRepo.Added);
        Assert.Equal(userId, activeUserRepo.Added[0].UserId);
    }

    [Fact]
    public async Task ProcessAsync_does_not_mark_active_user_for_anonymous_event()
    {
        var (processor, _, _, activeUserRepo, _, _, _) = ProcessorFactory.Create();
        var envelope = SimpleEvent();

        await processor.ProcessAsync(envelope);

        Assert.Empty(activeUserRepo.Added);
    }

    [Fact]
    public async Task ProcessAsync_does_not_add_duplicate_active_user_marker()
    {
        var (processor, _, _, activeUserRepo, _, _, _) = ProcessorFactory.Create();
        var userId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(Now);
        activeUserRepo.SeedExisting(date, userId);

        var envelope = LoginEvent(actorUserId: userId);
        await processor.ProcessAsync(envelope);

        Assert.Empty(activeUserRepo.Added);
    }

    [Fact]
    public async Task ProcessAsync_throws_for_invalid_event_type()
    {
        var (processor, _, _, _, _, _, _) = ProcessorFactory.Create();
        var envelope = new AnalyticsEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: "not.valid",
            OccurredAtUtc: Now,
            ActorUserId: null,
            SubjectId: null,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

        await Assert.ThrowsAsync<AnalyticsException>(() => processor.ProcessAsync(envelope));
    }

    [Fact]
    public async Task ProcessAsync_increments_existing_metric()
    {
        var (processor, _, metricRepo, _, _, _, _) = ProcessorFactory.Create();
        var envelope = SimpleEvent();

        await processor.ProcessAsync(envelope);
        var countAfterFirst = metricRepo.Metrics.First().Count;

        var secondEnvelope = SimpleEvent();
        await processor.ProcessAsync(secondEnvelope);

        Assert.Equal(1, countAfterFirst);
        Assert.Equal(2, metricRepo.Metrics.First().Count);
        Assert.Single(metricRepo.Metrics);
    }

    [Fact]
    public async Task ProcessAsync_creates_snapshot_for_course_event_with_subject()
    {
        var (processor, _, _, _, snapshotRepo, _, _) = ProcessorFactory.Create();
        var subjectId = Guid.NewGuid();
        var envelope = new AnalyticsEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.LearningCourseCreated,
            OccurredAtUtc: Now,
            ActorUserId: Guid.NewGuid(),
            SubjectId: subjectId,
            SubjectType: "Course",
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: "My Course",
            SubjectSlug: "my-course",
            SchemaVersion: 1);

        await processor.ProcessAsync(envelope);

        Assert.Single(snapshotRepo.Snapshots);
        Assert.Equal(subjectId, snapshotRepo.Snapshots[0].SubjectId);
    }
}
