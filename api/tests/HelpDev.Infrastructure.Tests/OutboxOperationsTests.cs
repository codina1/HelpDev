using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Time;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Tests;

public sealed class OutboxMessageResetTests
{
    [Fact]
    public void ResetForRetry_clears_processing_fields_and_keeps_identity()
    {
        var message = CreateMessage();
        message.AttemptCount = 10;
        message.LastAttemptAtUtc = DateTime.UtcNow.AddMinutes(-1);
        message.Error = "boom";
        message.LockedUntilUtc = DateTime.UtcNow.AddMinutes(-5);
        message.LockId = "expired-lock";
        var originalId = message.Id;
        var originalType = message.Type;
        var originalPayload = message.Payload;
        var originalOccurred = message.OccurredAtUtc;

        message.ResetForRetry(DateTime.UtcNow);

        Assert.Equal(0, message.AttemptCount);
        Assert.Null(message.LastAttemptAtUtc);
        Assert.Null(message.Error);
        Assert.Null(message.LockedUntilUtc);
        Assert.Null(message.LockId);
        Assert.Null(message.ProcessedAtUtc);
        Assert.Equal(originalId, message.Id);
        Assert.Equal(originalType, message.Type);
        Assert.Equal(originalPayload, message.Payload);
        Assert.Equal(originalOccurred, message.OccurredAtUtc);
    }

    [Fact]
    public void ResetForRetry_rejects_processed_message()
    {
        var message = CreateMessage();
        message.ProcessedAtUtc = DateTime.UtcNow;

        Assert.Throws<InvalidOperationException>(() => message.ResetForRetry(DateTime.UtcNow));
    }

    [Fact]
    public void ResetForRetry_rejects_active_lock()
    {
        var now = DateTime.UtcNow;
        var message = CreateMessage();
        message.LockedUntilUtc = now.AddMinutes(2);

        Assert.Throws<InvalidOperationException>(() => message.ResetForRetry(now));
    }

    [Fact]
    public void ResetForRetry_allows_expired_lock()
    {
        var now = DateTime.UtcNow;
        var message = CreateMessage();
        message.LockedUntilUtc = now.AddMinutes(-1);
        message.LockId = "old";

        message.ResetForRetry(now);

        Assert.Null(message.LockedUntilUtc);
        Assert.Null(message.LockId);
    }

    private static OutboxMessage CreateMessage() =>
        new()
        {
            Id = Guid.NewGuid(),
            Type = "learning.course-published.v1",
            Payload = "{\"ok\":true}",
            OccurredAtUtc = DateTime.UtcNow.AddHours(-1),
        };
}

public sealed class OutboxMessageStatusTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Derive_classifies_pending_processing_failed_and_processed()
    {
        Assert.Equal(
            OutboxMessageStatuses.Processed,
            OutboxMessageStatuses.Derive(Now, 0, null, Now, maxAttempts: 10));
        Assert.Equal(
            OutboxMessageStatuses.Processing,
            OutboxMessageStatuses.Derive(null, 1, Now.AddMinutes(1), Now, 10));
        Assert.Equal(
            OutboxMessageStatuses.Failed,
            OutboxMessageStatuses.Derive(null, 10, null, Now, 10));
        Assert.Equal(
            OutboxMessageStatuses.Pending,
            OutboxMessageStatuses.Derive(null, 2, Now.AddMinutes(-1), Now, 10));
    }

    [Fact]
    public void Failed_with_active_lock_is_processing()
    {
        Assert.Equal(
            OutboxMessageStatuses.Processing,
            OutboxMessageStatuses.Derive(null, 99, Now.AddSeconds(30), Now, 10));
    }
}

public sealed class OutboxOperationsServiceTests
{
    private readonly FakeOutboxRetryStore _store = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 18, 0, 0, DateTimeKind.Utc));
    private readonly OutboxOptions _options = new() { MaxAttempts = 5 };

    private OutboxOperationsService CreateSut(
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            _store,
            _clock,
            Options.Create(_options),
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<OutboxOperationsService>.Instance);

    [Fact]
    public async Task Retry_failed_message_resets_processing_fields_and_commits_once()
    {
        var message = SeedFailed();
        var payload = message.Payload;

        var detail = await CreateSut().RetryAsync(message.Id, Guid.NewGuid());

        Assert.Equal(0, message.AttemptCount);
        Assert.Null(message.Error);
        Assert.Null(message.LockId);
        Assert.Null(message.LockedUntilUtc);
        Assert.Null(message.ProcessedAtUtc);
        Assert.Equal(payload, message.Payload);
        Assert.Equal(OutboxMessageStatuses.Pending, detail.Status);
        Assert.Equal(1, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_pending_unprocessed_message_is_allowed()
    {
        var message = SeedPending();
        message.AttemptCount = 2;
        message.Error = "transient";

        await CreateSut().RetryAsync(message.Id);

        Assert.Equal(0, message.AttemptCount);
        Assert.Null(message.Error);
        Assert.Equal(1, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_processed_message_is_rejected_without_commit()
    {
        var message = SeedPending();
        message.ProcessedAtUtc = _clock.UtcNow;

        var ex = await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            CreateSut().RetryAsync(message.Id));

        Assert.Equal(OutboxOperationsErrorCodes.MessageAlreadyProcessed, ex.Code);
        Assert.Equal(0, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_actively_locked_message_is_rejected()
    {
        var message = SeedPending();
        message.LockedUntilUtc = _clock.UtcNow.AddMinutes(1);
        message.LockId = "active";

        var ex = await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            CreateSut().RetryAsync(message.Id));

        Assert.Equal(OutboxOperationsErrorCodes.MessageCurrentlyProcessing, ex.Code);
        Assert.Equal(0, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_expired_lock_can_be_reset()
    {
        var message = SeedFailed();
        message.LockedUntilUtc = _clock.UtcNow.AddMinutes(-2);
        message.LockId = "expired";

        await CreateSut().RetryAsync(message.Id);

        Assert.Null(message.LockedUntilUtc);
        Assert.Null(message.LockId);
        Assert.Equal(1, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_missing_message_returns_not_found()
    {
        var ex = await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            CreateSut().RetryAsync(Guid.NewGuid()));

        Assert.Equal(OutboxOperationsErrorCodes.MessageNotFound, ex.Code);
        Assert.Equal(0, _store.SaveChangesCount);
    }

    [Fact]
    public async Task Retry_forwards_CancellationToken_and_never_dispatches()
    {
        var message = SeedFailed();
        using var cts = new CancellationTokenSource();

        await CreateSut().RetryAsync(message.Id, cancellationToken: cts.Token);

        Assert.Equal(cts.Token, _store.LastCancellationToken);
        Assert.DoesNotContain(
            typeof(OutboxOperationsService).GetConstructors().Single().GetParameters(),
            parameter => parameter.ParameterType.Name.Contains("Dispatcher", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RetryFailed_validates_limit_and_uses_options_MaxAttempts()
    {
        var ex = await Assert.ThrowsAsync<OutboxOperationsException>(() =>
            CreateSut().RetryFailedAsync(new RetryFailedOutboxRequest(0, null)));
        Assert.Equal(OutboxOperationsErrorCodes.RetryLimitInvalid, ex.Code);

        _store.ResetCountToReturn = 3;
        var result = await CreateSut().RetryFailedAsync(
            new RetryFailedOutboxRequest(25, "content.published.v1"));

        Assert.Equal(25, result.RequestedLimit);
        Assert.Equal(3, result.ResetCount);
        Assert.Equal("content.published.v1", result.Type);
        Assert.Equal(25, _store.LastLimit);
        Assert.Equal(5, _store.LastMaxAttempts);
        Assert.Equal("content.published.v1", _store.LastTypeFilter);
    }

    [Fact]
    public void Batch_reset_sql_uses_skip_locked_and_failed_predicates()
    {
        Assert.Contains("FOR UPDATE SKIP LOCKED", OutboxOperationsService.ResetFailedBatchSql, StringComparison.Ordinal);
        Assert.Contains("attempt_count >=", OutboxOperationsService.ResetFailedBatchSql, StringComparison.Ordinal);
        Assert.Contains("processed_at_utc IS NULL", OutboxOperationsService.ResetFailedBatchSql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY occurred_at_utc", OutboxOperationsService.ResetFailedBatchSql, StringComparison.Ordinal);
    }

    private OutboxMessage SeedFailed()
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "content.updated.v1",
            Payload = "{\"secret\":\"do-not-leak\"}",
            OccurredAtUtc = _clock.UtcNow.AddHours(-2),
            AttemptCount = _options.MaxAttempts,
            Error = "handler failed",
            LastAttemptAtUtc = _clock.UtcNow.AddMinutes(-10),
        };
        _store.Messages[message.Id] = message;
        return message;
    }

    private OutboxMessage SeedPending()
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "learning.course-updated.v1",
            Payload = "{}",
            OccurredAtUtc = _clock.UtcNow.AddHours(-1),
            AttemptCount = 0,
        };
        _store.Messages[message.Id] = message;
        return message;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public FakeDateTimeProvider(DateTime utcNow) => UtcNow = utcNow;

        public DateTime UtcNow { get; }
    }

    private sealed class FakeOutboxRetryStore : IOutboxRetryStore
    {
        public Dictionary<Guid, OutboxMessage> Messages { get; } = new();

        public int SaveChangesCount { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public int ResetCountToReturn { get; set; }

        public int LastLimit { get; private set; }

        public int LastMaxAttempts { get; private set; }

        public string? LastTypeFilter { get; private set; }

        public Task<OutboxMessage?> GetTrackedByIdAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            Messages.TryGetValue(messageId, out var message);
            return Task.FromResult(message);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            SaveChangesCount++;
            return Task.CompletedTask;
        }

        public Task<int> ResetFailedBatchAsync(
            int limit,
            string? typeFilter,
            DateTime nowUtc,
            int maxAttempts,
            CancellationToken cancellationToken = default)
        {
            LastCancellationToken = cancellationToken;
            LastLimit = limit;
            LastTypeFilter = typeFilter;
            LastMaxAttempts = maxAttempts;
            return Task.FromResult(ResetCountToReturn);
        }
    }
}
