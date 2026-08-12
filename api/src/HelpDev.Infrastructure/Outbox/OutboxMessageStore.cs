using HelpDev.Infrastructure.Persistence;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Outbox;

public interface IOutboxMessageStore
{
    Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
        string lockId,
        CancellationToken cancellationToken = default);

    Task MarkProcessedAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Claims pending outbox rows using PostgreSQL FOR UPDATE SKIP LOCKED for multi-instance safety.
/// Processing outcome updates use ExecuteUpdate to avoid re-entering ApplicationDbContext.SaveChangesAsync.
/// </summary>
public sealed class OutboxMessageStore : IOutboxMessageStore
{
    public const int ErrorSummaryMaxLength = OutboxMessageConfiguration.ErrorMaxLength;

    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly OutboxOptions _options;

    public OutboxMessageStore(
        ApplicationDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<OutboxOptions> options)
    {
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<OutboxMessage>> ClaimPendingAsync(
        string lockId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockId);

        var now = _clock.UtcNow;
        var lockedUntil = now.AddSeconds(_options.LockDurationSeconds);
        var batchSize = _options.BatchSize;
        var maxAttempts = _options.MaxAttempts;

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            WITH pending AS (
                SELECT "Id"
                FROM outbox_messages
                WHERE processed_at_utc IS NULL
                  AND attempt_count < {maxAttempts}
                  AND (locked_until_utc IS NULL OR locked_until_utc < {now})
                ORDER BY occurred_at_utc
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
            )
            UPDATE outbox_messages AS o
            SET
                locked_until_utc = {lockedUntil},
                lock_id = {lockId},
                last_attempt_at_utc = {now}
            FROM pending
            WHERE o."Id" = pending."Id"
            """, cancellationToken).ConfigureAwait(false);

        var claimed = await _dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.LockId == lockId && message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return claimed;
    }

    public async Task MarkProcessedAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var now = _clock.UtcNow;
        await _dbContext.OutboxMessages
            .Where(row => row.Id == message.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(row => row.ProcessedAtUtc, now)
                    .SetProperty(row => row.Error, (string?)null)
                    .SetProperty(row => row.LockId, (string?)null)
                    .SetProperty(row => row.LockedUntilUtc, (DateTime?)null),
                cancellationToken)
            .ConfigureAwait(false);

        message.ProcessedAtUtc = now;
        message.Error = null;
        message.LockId = null;
        message.LockedUntilUtc = null;
    }

    public async Task MarkFailedAsync(
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var now = _clock.UtcNow;
        var summary = TruncateError(error);
        var attempts = message.AttemptCount + 1;

        await _dbContext.OutboxMessages
            .Where(row => row.Id == message.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(row => row.AttemptCount, attempts)
                    .SetProperty(row => row.LastAttemptAtUtc, now)
                    .SetProperty(row => row.Error, summary)
                    .SetProperty(row => row.LockId, (string?)null)
                    .SetProperty(row => row.LockedUntilUtc, (DateTime?)null),
                cancellationToken)
            .ConfigureAwait(false);

        message.AttemptCount = attempts;
        message.LastAttemptAtUtc = now;
        message.Error = summary;
        message.LockId = null;
        message.LockedUntilUtc = null;
    }

    public static string TruncateError(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown outbox processing error.";
        }

        var trimmed = error.Trim();
        return trimmed.Length <= ErrorSummaryMaxLength
            ? trimmed
            : trimmed[..ErrorSummaryMaxLength];
    }
}
