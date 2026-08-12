using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.SharedContracts.Auditing;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Auditing.Application.Queries;

public sealed record AuditRecordDto(
    Guid Id,
    DateTime OccurredAtUtc,
    string Category,
    string Action,
    string Outcome,
    Guid? ActorUserId,
    string ActorType,
    Guid? SubjectId,
    string? SubjectType,
    string? SubjectDisplay,
    string? ReasonCode,
    string? CorrelationId,
    string? RequestMethod,
    string? RequestPathTemplate,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record AuditQueryFilter(
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? Category,
    string? Action,
    string? Outcome,
    Guid? ActorUserId,
    Guid? SubjectId,
    string? SubjectType,
    int Page,
    int PageSize);

public sealed record AuditPageResult(
    IReadOnlyList<AuditRecordDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public interface IAuditQueries
{
    Task<AuditPageResult> GetPageAsync(AuditQueryFilter filter, CancellationToken cancellationToken = default);

    Task<AuditRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class AuditQueries : IAuditQueries
{
    private const int MaxDateRangeDays = 366;

    private readonly IAuditDbContext _dbContext;

    public AuditQueries(IAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditPageResult> GetPageAsync(
        AuditQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateFilter(filter);

        var query = _dbContext.AuditRecords.AsNoTracking();

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(record => record.OccurredAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(record => record.OccurredAtUtc <= filter.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(record => record.Category == filter.Category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action))
        {
            query = query.Where(record => record.Action == filter.Action);
        }

        if (!string.IsNullOrWhiteSpace(filter.Outcome))
        {
            query = query.Where(record => record.Outcome == filter.Outcome);
        }

        if (filter.ActorUserId.HasValue)
        {
            query = query.Where(record => record.ActorUserId == filter.ActorUserId.Value);
        }

        if (filter.SubjectId.HasValue)
        {
            query = query.Where(record => record.SubjectId == filter.SubjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SubjectType))
        {
            query = query.Where(record => record.SubjectType == filter.SubjectType);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(record => record.OccurredAtUtc)
            .ThenByDescending(record => record.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(record => Map(record))
            .ToListAsync(cancellationToken);

        return new AuditPageResult(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<AuditRecordDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.AuditRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (record is null)
        {
            throw new AuditException("Audit record was not found.", AuditErrorCodes.RecordNotFound);
        }

        return Map(record);
    }

    private static void ValidateFilter(AuditQueryFilter filter)
    {
        if (filter.Page < 1 || filter.PageSize is < 1 or > 100)
        {
            throw new AuditException("Audit page parameters are invalid.", AuditErrorCodes.PageInvalid);
        }

        if (filter.FromUtc.HasValue && filter.ToUtc.HasValue && filter.FromUtc > filter.ToUtc)
        {
            throw new AuditException("Audit date range is invalid.", AuditErrorCodes.DateRangeInvalid);
        }

        if (filter.FromUtc.HasValue && filter.ToUtc.HasValue)
        {
            var rangeDays = (filter.ToUtc.Value - filter.FromUtc.Value).TotalDays;
            if (rangeDays > MaxDateRangeDays)
            {
                throw new AuditException("Audit date range exceeds maximum allowed.", AuditErrorCodes.DateRangeTooLarge);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.Category) && !AuditCategories.IsSupported(filter.Category))
        {
            throw new AuditException("Audit category filter is invalid.", AuditErrorCodes.CategoryInvalid);
        }

        if (!string.IsNullOrWhiteSpace(filter.Action) && !AuditActions.IsSupported(filter.Action))
        {
            throw new AuditException("Audit action filter is invalid.", AuditErrorCodes.ActionUnsupported);
        }

        if (!string.IsNullOrWhiteSpace(filter.Outcome) && !AuditOutcomes.IsSupported(filter.Outcome))
        {
            throw new AuditException("Audit outcome filter is invalid.", AuditErrorCodes.OutcomeInvalid);
        }
    }

    private static AuditRecordDto Map(AuditRecord record) =>
        new(
            record.Id,
            record.OccurredAtUtc,
            record.Category,
            record.Action,
            record.Outcome,
            record.ActorUserId,
            record.ActorType,
            record.SubjectId,
            record.SubjectType,
            record.SubjectDisplay,
            record.ReasonCode,
            record.CorrelationId,
            record.RequestMethod,
            record.RequestPathTemplate,
            record.Metadata);
}
