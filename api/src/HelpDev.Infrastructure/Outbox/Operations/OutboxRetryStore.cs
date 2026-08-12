using HelpDev.Infrastructure.Persistence;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Outbox.Operations;

/// <summary>
/// Narrow persistence port for Outbox recovery mutations (no dispatch).
/// </summary>
public interface IOutboxRetryStore
{
    Task<OutboxMessage?> GetTrackedByIdAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> ResetFailedBatchAsync(
        int limit,
        string? typeFilter,
        DateTime nowUtc,
        int maxAttempts,
        CancellationToken cancellationToken = default);
}

public sealed class EfOutboxRetryStore : IOutboxRetryStore
{
    public const string ResetFailedBatchSqlTemplate = OutboxOperationsService.ResetFailedBatchSql;

    private readonly ApplicationDbContext _dbContext;

    public EfOutboxRetryStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OutboxMessage?> GetTrackedByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default) =>
        _dbContext.OutboxMessages.FirstOrDefaultAsync(row => row.Id == messageId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<int> ResetFailedBatchAsync(
        int limit,
        string? typeFilter,
        DateTime nowUtc,
        int maxAttempts,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var resetCount = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            WITH failed AS (
                SELECT "Id"
                FROM outbox_messages
                WHERE processed_at_utc IS NULL
                  AND attempt_count >= {maxAttempts}
                  AND (locked_until_utc IS NULL OR locked_until_utc < {nowUtc})
                  AND ({typeFilter}::text IS NULL OR type = {typeFilter})
                ORDER BY occurred_at_utc, "Id"
                LIMIT {limit}
                FOR UPDATE SKIP LOCKED
            )
            UPDATE outbox_messages AS o
            SET
                attempt_count = 0,
                last_attempt_at_utc = NULL,
                error = NULL,
                locked_until_utc = NULL,
                lock_id = NULL
            FROM failed
            WHERE o."Id" = failed."Id"
            """, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return resetCount;
    }
}
