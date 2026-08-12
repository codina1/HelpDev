using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Events;

namespace HelpDev.Infrastructure.Tests;

public sealed class OutboxCaptureTests
{
    [Fact]
    public void No_events_creates_no_outbox_records()
    {
        var messages = OutboxCapture.CreateMessages(
            Array.Empty<DomainEventSnapshot>(),
            CreateSerializer());

        Assert.Empty(messages);
    }

    [Fact]
    public void One_event_creates_one_record_with_stable_type()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new CoursePublishedDomainEvent(Guid.NewGuid(), "slug");
        aggregate.Raise(domainEvent);
        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);

        var messages = OutboxCapture.CreateMessages(snapshots, CreateSerializer());

        var message = Assert.Single(messages);
        Assert.Equal(domainEvent.EventId, message.Id);
        Assert.Equal("learning.course-published.v1", message.Type);
        Assert.False(string.IsNullOrWhiteSpace(message.Payload));
        Assert.Equal(0, message.AttemptCount);
        Assert.True(aggregate.HasDomainEvents);
    }

    [Fact]
    public void Multiple_events_preserve_order_without_duplicates()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var first = new CoursePublishedDomainEvent(Guid.NewGuid(), "a");
        var second = new CoursePublishedDomainEvent(Guid.NewGuid(), "b");
        aggregate.Raise(first);
        aggregate.Raise(second);
        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);

        var messages = OutboxCapture.CreateMessages(snapshots, CreateSerializer());

        Assert.Equal(2, messages.Count);
        Assert.Equal(first.EventId, messages[0].Id);
        Assert.Equal(second.EventId, messages[1].Id);
        Assert.Equal(2, messages.Select(message => message.Id).Distinct().Count());
    }

    [Fact]
    public void ClearCaptured_after_successful_persist_path_clears_aggregates()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new CoursePublishedDomainEvent(Guid.NewGuid(), "slug"));
        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);
        _ = OutboxCapture.CreateMessages(snapshots, CreateSerializer());

        DomainEventCommitPipeline.ClearCaptured(snapshots);

        Assert.False(aggregate.HasDomainEvents);
    }

    [Fact]
    public void Failed_persist_path_leaves_aggregate_events_when_clear_is_skipped()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new CoursePublishedDomainEvent(Guid.NewGuid(), "slug"));
        var snapshots = DomainEventCommitPipeline.Capture([aggregate]);
        _ = OutboxCapture.CreateMessages(snapshots, CreateSerializer());

        // Simulate failed SaveChanges: do not call ClearCaptured.
        Assert.True(aggregate.HasDomainEvents);
    }

    private static IOutboxEventSerializer CreateSerializer()
    {
        var registry = new OutboxEventTypeRegistry();
        registry.Register<CoursePublishedDomainEvent>("learning.course-published.v1");
        registry.Seal();
        return new SystemTextJsonOutboxEventSerializer(registry);
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
            : base(id)
        {
        }

        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }
}
