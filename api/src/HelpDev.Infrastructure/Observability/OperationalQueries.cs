using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Infrastructure.Persistence;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Observability;

public sealed class OutboxOperationalQueries : IOutboxOperationalQueries
{
    private readonly IOutboxOperationsQueries _queries;
    private readonly OutboxProcessorHeartbeat _heartbeat;
    private readonly IDateTimeProvider _clock;

    public OutboxOperationalQueries(
        IOutboxOperationsQueries queries,
        OutboxProcessorHeartbeat heartbeat,
        IDateTimeProvider clock)
    {
        _queries = queries;
        _heartbeat = heartbeat;
        _clock = clock;
    }

    public async Task<OutboxOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var status = await _queries.GetStatusAsync(cancellationToken);
        var heartbeat = _heartbeat.GetSnapshot();

        return new OutboxOperationalSnapshot(
            status.Pending,
            status.Processing,
            status.Failed,
            status.Failed,
            status.OldestPendingAtUtc,
            status.LastProcessedAtUtc,
            heartbeat.LastFailureAtUtc,
            ProcessorEnabled: true,
            _clock.UtcNow);
    }
}

public sealed class SearchOperationalQueries : ISearchOperationalQueries
{
    private readonly ISearchOperationalDataSource _dataSource;
    private readonly IDateTimeProvider _clock;

    public SearchOperationalQueries(
        ISearchOperationalDataSource dataSource,
        IDateTimeProvider clock)
    {
        _dataSource = dataSource;
        _clock = clock;
    }

    public async Task<SearchOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dataSource.GetAsync(cancellationToken);
        return new SearchOperationalSnapshot(
            data.PendingProjectionCount,
            data.FailedProjectionCount,
            data.OldestPendingAtUtc,
            data.LastSuccessfulProjectionAtUtc,
            data.LastReindexCompletedAtUtc,
            ProjectionProcessorEnabled: true,
            _clock.UtcNow);
    }
}

public interface ISearchOperationalDataSource
{
    Task<SearchOperationalData> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record SearchOperationalData(
    long PendingProjectionCount,
    long FailedProjectionCount,
    DateTime? OldestPendingAtUtc,
    DateTime? LastSuccessfulProjectionAtUtc,
    DateTime? LastReindexCompletedAtUtc);

public sealed class SearchOperationalDataSource : ISearchOperationalDataSource
{
    private readonly Modules.Search.Application.Persistence.ISearchDbContext _searchDbContext;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly OutboxOptions _outboxOptions;

    public SearchOperationalDataSource(
        Modules.Search.Application.Persistence.ISearchDbContext searchDbContext,
        ApplicationDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<OutboxOptions> outboxOptions)
    {
        _searchDbContext = searchDbContext;
        _dbContext = dbContext;
        _clock = clock;
        _outboxOptions = outboxOptions.Value;
    }

    public async Task<SearchOperationalData> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var maxAttempts = _outboxOptions.MaxAttempts;

        var lastIndexed = await _searchDbContext.SearchDocuments.AsNoTracking()
            .MaxAsync(document => (DateTime?)document.IndexedAtUtc, cancellationToken);

        var pendingQuery = _dbContext.OutboxMessages.AsNoTracking()
            .Where(message =>
                message.ProcessedAtUtc == null
                && message.AttemptCount < maxAttempts
                && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now)
                && (message.Type.StartsWith("content.published")
                    || message.Type.StartsWith("content.updated")
                    || message.Type.StartsWith("learning.course-published")
                    || message.Type.StartsWith("learning.course-updated")));

        var pendingCount = await pendingQuery.CountAsync(cancellationToken);
        var oldestPending = await pendingQuery
            .Select(message => (DateTime?)message.OccurredAtUtc)
            .MinAsync(cancellationToken);

        var failedCount = await _dbContext.OutboxMessages.AsNoTracking()
            .CountAsync(
                message =>
                    message.ProcessedAtUtc == null
                    && message.AttemptCount >= maxAttempts
                    && (message.Type.StartsWith("content.published")
                        || message.Type.StartsWith("content.updated")
                        || message.Type.StartsWith("learning.course-published")
                        || message.Type.StartsWith("learning.course-updated")),
                cancellationToken);

        return new SearchOperationalData(
            pendingCount,
            failedCount,
            oldestPending,
            lastIndexed,
            lastIndexed);
    }
}
