using HelpDev.Infrastructure.Observability;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Events;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Tests;

public sealed class OutboxProcessorTests
{
    [Fact]
    public async Task Pending_message_is_dispatched_and_marked_processed()
    {
        var store = new FakeOutboxMessageStore();
        var domainEvent = new CoursePublishedDomainEvent(Guid.NewGuid(), "slug");
        store.Pending.Add(CreateMessage(domainEvent));
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(store, dispatcher);

        await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Single(dispatcher.Events);
        Assert.NotNull(store.Pending[0].ProcessedAtUtc);
        Assert.Null(store.Pending[0].Error);
    }

    [Fact]
    public async Task Zero_handlers_still_marks_processed()
    {
        var store = new FakeOutboxMessageStore();
        store.Pending.Add(CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "slug")));
        var dispatcher = new NoOpDispatcher();
        var processor = CreateProcessor(store, dispatcher);

        await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.NotNull(store.Pending[0].ProcessedAtUtc);
    }

    [Fact]
    public async Task Handler_exception_increments_attempts_and_keeps_unprocessed()
    {
        var store = new FakeOutboxMessageStore();
        store.Pending.Add(CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "slug")));
        var processor = CreateProcessor(store, new FailingDispatcher());

        await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Null(store.Pending[0].ProcessedAtUtc);
        Assert.Equal(1, store.Pending[0].AttemptCount);
        Assert.Contains("InvalidOperationException", store.Pending[0].Error, StringComparison.Ordinal);
        Assert.DoesNotContain("payload", store.Pending[0].Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Max_attempt_processed_and_locked_messages_are_skipped_by_store()
    {
        var store = new FakeOutboxMessageStore
        {
            MaxAttempts = 2,
        };
        var maxed = CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "a"));
        maxed.AttemptCount = 2;
        var processed = CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "b"));
        processed.ProcessedAtUtc = DateTime.UtcNow;
        var locked = CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "c"));
        locked.LockedUntilUtc = DateTime.UtcNow.AddMinutes(5);
        store.Pending.Add(maxed);
        store.Pending.Add(processed);
        store.Pending.Add(locked);
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(store, dispatcher);

        await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Empty(dispatcher.Events);
    }

    [Fact]
    public async Task Expired_lock_is_reclaimable_and_oldest_first_within_batch()
    {
        var store = new FakeOutboxMessageStore { BatchSize = 1 };
        var older = CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "older"));
        older.OccurredAtUtc = DateTime.UtcNow.AddMinutes(-10);
        older.LockedUntilUtc = DateTime.UtcNow.AddMinutes(-1);
        var newer = CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "newer"));
        newer.OccurredAtUtc = DateTime.UtcNow.AddMinutes(-1);
        store.Pending.Add(newer);
        store.Pending.Add(older);
        var dispatcher = new RecordingDispatcher();
        var processor = CreateProcessor(store, dispatcher);

        await processor.ProcessBatchAsync(CancellationToken.None);

        Assert.Equal(older.Id, Assert.Single(store.LastClaimed).Id);
        Assert.Equal(older.Id, Assert.Single(dispatcher.Events).EventId);
    }

    [Fact]
    public async Task Cancellation_is_honored_before_dispatch_loop_continues()
    {
        var store = new FakeOutboxMessageStore();
        store.Pending.Add(CreateMessage(new CoursePublishedDomainEvent(Guid.NewGuid(), "slug")));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var processor = CreateProcessor(store, new RecordingDispatcher());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            processor.ProcessBatchAsync(cts.Token));
    }

    [Fact]
    public void TruncateError_bounds_length_and_omits_payload_requirement()
    {
        var longError = new string('x', OutboxMessageStore.ErrorSummaryMaxLength + 50);
        var truncated = OutboxMessageStore.TruncateError(longError);
        Assert.Equal(OutboxMessageStore.ErrorSummaryMaxLength, truncated.Length);
    }

    [Fact]
    public void OutboxProcessor_constructor_takes_scope_factory_not_DbContext()
    {
        var ctor = typeof(OutboxProcessor).GetConstructors().Single();
        Assert.DoesNotContain(
            ctor.GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
        Assert.Contains(
            ctor.GetParameters(),
            parameter => parameter.ParameterType == typeof(IServiceScopeFactory));
    }

    private static OutboxProcessor CreateProcessor(
        FakeOutboxMessageStore store,
        IDomainEventDispatcher dispatcher)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOutboxMessageStore>(store);
        services.AddSingleton(dispatcher);
        services.AddSingleton<IOutboxEventSerializer>(CreateSerializer());
        services.AddSingleton<IDateTimeProvider>(new FixedClock(DateTime.UtcNow));
        services.AddSingleton(Options.Create(new OutboxOptions
        {
            BatchSize = store.BatchSize,
            MaxAttempts = store.MaxAttempts,
            LockDurationSeconds = 30,
            PollIntervalSeconds = 5,
        }));
        var provider = services.BuildServiceProvider();

        return new OutboxProcessor(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OutboxOptions()),
            new OutboxProcessorHeartbeat(),
            NullLogger<OutboxProcessor>.Instance);
    }

    private static OutboxMessage CreateMessage(CoursePublishedDomainEvent domainEvent)
    {
        var serializer = CreateSerializer();
        var serialized = serializer.Serialize(domainEvent);
        return new OutboxMessage
        {
            Id = serialized.Id,
            OccurredAtUtc = serialized.OccurredAtUtc,
            Type = serialized.Type,
            Payload = serialized.Payload,
            AttemptCount = 0,
        };
    }

    private static IOutboxEventSerializer CreateSerializer()
    {
        var registry = new OutboxEventTypeRegistry();
        registry.Register<CoursePublishedDomainEvent>("learning.course-published.v1");
        registry.Seal();
        return new SystemTextJsonOutboxEventSerializer(registry);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public FixedClock(DateTime utcNow) => UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        public DateTime UtcNow { get; }
    }

    private sealed class RecordingDispatcher : IDomainEventDispatcher
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            Events.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FailingDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("handler failed");
    }

    private sealed class FakeOutboxMessageStore : IOutboxMessageStore
    {
        public List<OutboxMessage> Pending { get; } = [];

        public List<OutboxMessage> LastClaimed { get; } = [];

        public int BatchSize { get; set; } = 20;

        public int MaxAttempts { get; set; } = 10;

        public Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
            string lockId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = DateTime.UtcNow;
            var claimed = Pending
                .Where(message =>
                    message.ProcessedAtUtc is null
                    && message.AttemptCount < MaxAttempts
                    && (message.LockedUntilUtc is null || message.LockedUntilUtc < now))
                .OrderBy(message => message.OccurredAtUtc)
                .Take(BatchSize)
                .ToList();

            foreach (var message in claimed)
            {
                message.LockId = lockId;
                message.LockedUntilUtc = now.AddSeconds(30);
            }

            LastClaimed.Clear();
            LastClaimed.AddRange(claimed);
            return Task.FromResult<IReadOnlyList<OutboxMessage>>(claimed);
        }

        public Task MarkProcessedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            message.ProcessedAtUtc = DateTime.UtcNow;
            message.Error = null;
            message.LockId = null;
            message.LockedUntilUtc = null;
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(OutboxMessage message, string error, CancellationToken cancellationToken = default)
        {
            message.AttemptCount += 1;
            message.LastAttemptAtUtc = DateTime.UtcNow;
            message.Error = OutboxMessageStore.TruncateError(error);
            message.LockId = null;
            message.LockedUntilUtc = null;
            return Task.CompletedTask;
        }
    }
}
