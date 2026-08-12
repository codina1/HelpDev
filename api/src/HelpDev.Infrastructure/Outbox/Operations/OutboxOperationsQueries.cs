using HelpDev.Infrastructure.Persistence;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Outbox.Operations;

public sealed class OutboxOperationsQueries : IOutboxOperationsQueries
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly OutboxOptions _options;

    public OutboxOperationsQueries(
        ApplicationDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<OutboxOptions> options)
    {
        _dbContext = dbContext;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<OutboxStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var maxAttempts = _options.MaxAttempts;

        var pending = await CountPendingAsync(now, maxAttempts, cancellationToken);
        var processing = await CountProcessingAsync(now, cancellationToken);
        var failed = await CountFailedAsync(now, maxAttempts, cancellationToken);
        var processed = await _dbContext.OutboxMessages.AsNoTracking()
            .CountAsync(message => message.ProcessedAtUtc != null, cancellationToken);

        var oldestPending = await _dbContext.OutboxMessages.AsNoTracking()
            .Where(message =>
                message.ProcessedAtUtc == null
                && message.AttemptCount < maxAttempts
                && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now))
            .Select(message => (DateTime?)message.OccurredAtUtc)
            .MinAsync(cancellationToken);

        var lastProcessed = await _dbContext.OutboxMessages.AsNoTracking()
            .Where(message => message.ProcessedAtUtc != null)
            .Select(message => message.ProcessedAtUtc)
            .MaxAsync(cancellationToken);

        return new OutboxStatusDto(
            pending,
            processing,
            failed,
            processed,
            oldestPending,
            lastProcessed);
    }

    public async Task<OutboxMessagePageDto> ListAsync(
        OutboxMessageFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ValidatePaging(filter.Page, filter.PageSize);

        var now = _clock.UtcNow;
        var maxAttempts = _options.MaxAttempts;
        var status = NormalizeStatus(filter.Status);
        var type = NormalizeType(filter.Type);

        var query = _dbContext.OutboxMessages.AsNoTracking().AsQueryable();
        if (type is not null)
        {
            query = query.Where(message => message.Type == type);
        }

        query = ApplyStatusFilter(query, status, now, maxAttempts);

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(message => message.OccurredAtUtc)
            .ThenByDescending(message => message.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(message => new
            {
                message.Id,
                message.Type,
                message.OccurredAtUtc,
                message.ProcessedAtUtc,
                message.AttemptCount,
                message.LastAttemptAtUtc,
                message.Error,
                message.LockedUntilUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new OutboxMessageListItemDto(
                row.Id,
                row.Type,
                row.OccurredAtUtc,
                row.ProcessedAtUtc,
                row.AttemptCount,
                row.LastAttemptAtUtc,
                row.Error,
                row.LockedUntilUtc,
                OutboxMessageStatuses.Derive(
                    row.ProcessedAtUtc,
                    row.AttemptCount,
                    row.LockedUntilUtc,
                    now,
                    maxAttempts)))
            .ToList();

        return new OutboxMessagePageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<OutboxMessageDetailDto?> GetByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        if (messageId == Guid.Empty)
        {
            throw new OutboxOperationsException(
                "Outbox message id is invalid.",
                OutboxOperationsErrorCodes.OperationInvalid);
        }

        var now = _clock.UtcNow;
        var maxAttempts = _options.MaxAttempts;

        var row = await _dbContext.OutboxMessages.AsNoTracking()
            .Where(message => message.Id == messageId)
            .Select(message => new
            {
                message.Id,
                message.Type,
                message.OccurredAtUtc,
                message.ProcessedAtUtc,
                message.AttemptCount,
                message.LastAttemptAtUtc,
                message.Error,
                message.LockedUntilUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new OutboxMessageDetailDto(
            row.Id,
            row.Type,
            row.OccurredAtUtc,
            row.ProcessedAtUtc,
            row.AttemptCount,
            row.LastAttemptAtUtc,
            row.Error,
            row.LockedUntilUtc,
            OutboxMessageStatuses.Derive(
                row.ProcessedAtUtc,
                row.AttemptCount,
                row.LockedUntilUtc,
                now,
                maxAttempts));
    }

    private Task<int> CountPendingAsync(DateTime now, int maxAttempts, CancellationToken cancellationToken) =>
        _dbContext.OutboxMessages.AsNoTracking()
            .CountAsync(
                message =>
                    message.ProcessedAtUtc == null
                    && message.AttemptCount < maxAttempts
                    && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now),
                cancellationToken);

    private Task<int> CountProcessingAsync(DateTime now, CancellationToken cancellationToken) =>
        _dbContext.OutboxMessages.AsNoTracking()
            .CountAsync(
                message =>
                    message.ProcessedAtUtc == null
                    && message.LockedUntilUtc != null
                    && message.LockedUntilUtc > now,
                cancellationToken);

    private Task<int> CountFailedAsync(DateTime now, int maxAttempts, CancellationToken cancellationToken) =>
        _dbContext.OutboxMessages.AsNoTracking()
            .CountAsync(
                message =>
                    message.ProcessedAtUtc == null
                    && message.AttemptCount >= maxAttempts
                    && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now),
                cancellationToken);

    private static IQueryable<OutboxMessage> ApplyStatusFilter(
        IQueryable<OutboxMessage> query,
        string? status,
        DateTime now,
        int maxAttempts)
    {
        if (status is null)
        {
            return query;
        }

        return status switch
        {
            OutboxMessageStatuses.Processed =>
                query.Where(message => message.ProcessedAtUtc != null),
            OutboxMessageStatuses.Processing =>
                query.Where(message =>
                    message.ProcessedAtUtc == null
                    && message.LockedUntilUtc != null
                    && message.LockedUntilUtc > now),
            OutboxMessageStatuses.Failed =>
                query.Where(message =>
                    message.ProcessedAtUtc == null
                    && message.AttemptCount >= maxAttempts
                    && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now)),
            OutboxMessageStatuses.Pending =>
                query.Where(message =>
                    message.ProcessedAtUtc == null
                    && message.AttemptCount < maxAttempts
                    && (message.LockedUntilUtc == null || message.LockedUntilUtc <= now)),
            _ => query,
        };
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new OutboxOperationsException(
                "Page must be greater than or equal to 1.",
                OutboxOperationsErrorCodes.PageInvalid);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new OutboxOperationsException(
                $"Page size must be between 1 and {MaxPageSize}.",
                OutboxOperationsErrorCodes.PageSizeInvalid);
        }
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (!OutboxMessageStatuses.IsKnown(normalized))
        {
            throw new OutboxOperationsException(
                $"Unsupported outbox status '{status}'.",
                OutboxOperationsErrorCodes.StatusInvalid);
        }

        return normalized;
    }

    private static string? NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return type.Trim();
    }
}
