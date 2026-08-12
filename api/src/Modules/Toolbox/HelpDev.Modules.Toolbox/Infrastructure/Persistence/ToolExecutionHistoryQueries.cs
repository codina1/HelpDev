using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.History;
using HelpDev.Modules.Toolbox.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolExecutionHistoryQueries : IToolExecutionHistoryQueries
{
    private readonly IToolboxDbContext _dbContext;

    public ToolExecutionHistoryQueries(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ToolExecutionHistoryPageDto> GetMyHistoryAsync(
        Guid userId,
        ToolExecutionHistoryFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query =
            from record in _dbContext.ToolExecutionRecords.AsNoTracking()
            join tool in _dbContext.ToolDefinitions.AsNoTracking()
                on record.ToolId equals tool.Id
            where record.UserId == userId
            select new { record, tool };

        if (filter.ToolId.HasValue)
        {
            query = query.Where(row => row.record.ToolId == filter.ToolId.Value);
        }

        if (filter.Succeeded.HasValue)
        {
            query = query.Where(row => row.record.Succeeded == filter.Succeeded.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(row => row.record.ExecutedAtUtc)
            .ThenByDescending(row => row.record.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.record.Id,
                row.record.ToolId,
                ToolSlug = row.tool.Slug,
                ToolName = row.tool.Name,
                Type = row.record.ToolType,
                row.record.Succeeded,
                row.record.DurationMilliseconds,
                row.record.InputPreview,
                row.record.OutputPreview,
                row.record.ErrorCode,
                row.record.ExecutedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ToolExecutionHistoryItemDto(
                row.Id,
                row.ToolId,
                row.ToolSlug.Value,
                row.ToolName,
                row.Type.ToString(),
                row.Succeeded,
                row.DurationMilliseconds,
                row.InputPreview,
                row.OutputPreview,
                row.ErrorCode,
                row.ExecutedAtUtc))
            .ToList();

        return new ToolExecutionHistoryPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<ToolExecutionHistoryItemDto?> GetMyExecutionAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await (
            from record in _dbContext.ToolExecutionRecords.AsNoTracking()
            join tool in _dbContext.ToolDefinitions.AsNoTracking()
                on record.ToolId equals tool.Id
            where record.Id == id && record.UserId == userId
            select new
            {
                record.Id,
                record.ToolId,
                ToolSlug = tool.Slug,
                ToolName = tool.Name,
                Type = record.ToolType,
                record.Succeeded,
                record.DurationMilliseconds,
                record.InputPreview,
                record.OutputPreview,
                record.ErrorCode,
                record.ExecutedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new ToolExecutionHistoryItemDto(
            row.Id,
            row.ToolId,
            row.ToolSlug.Value,
            row.ToolName,
            row.Type.ToString(),
            row.Succeeded,
            row.DurationMilliseconds,
            row.InputPreview,
            row.OutputPreview,
            row.ErrorCode,
            row.ExecutedAtUtc);
    }

    private static void EnsureValidPaging(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > ToolboxPaging.MaxPageSize)
        {
            throw new ToolboxException(
                $"Page must be >= 1 and pageSize must be between 1 and {ToolboxPaging.MaxPageSize}.",
                ToolboxApplicationErrorCodes.PaginationInvalid);
        }
    }
}
