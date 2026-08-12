using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.History;
using HelpDev.Modules.PromptLab.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptRenderHistoryQueries : IPromptRenderHistoryQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptRenderHistoryQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PromptRenderHistoryPageDto> GetMyHistoryAsync(
        Guid userId,
        PromptRenderHistoryFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query =
            from record in _dbContext.PromptRenderRecords.AsNoTracking()
            join prompt in _dbContext.PromptDefinitions.AsNoTracking()
                on record.PromptDefinitionId equals prompt.Id
            where record.UserId == userId
            select new { record, prompt };

        if (filter.PromptId.HasValue)
        {
            query = query.Where(row => row.record.PromptDefinitionId == filter.PromptId.Value);
        }

        if (filter.Succeeded.HasValue)
        {
            query = query.Where(row => row.record.Succeeded == filter.Succeeded.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(row => row.record.RenderedAtUtc)
            .ThenByDescending(row => row.record.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.record.Id,
                PromptId = row.record.PromptDefinitionId,
                PromptSlug = row.prompt.Slug,
                PromptName = row.prompt.Name,
                row.record.VersionNumber,
                row.record.Succeeded,
                row.record.DurationMilliseconds,
                row.record.InputPreview,
                row.record.RenderedPreview,
                row.record.ErrorCode,
                row.record.RenderedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new PromptRenderHistoryItemDto(
                row.Id,
                row.PromptId,
                row.PromptSlug.Value,
                row.PromptName,
                row.VersionNumber,
                row.Succeeded,
                row.DurationMilliseconds,
                row.InputPreview,
                row.RenderedPreview,
                row.ErrorCode,
                row.RenderedAtUtc))
            .ToList();

        return new PromptRenderHistoryPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<PromptRenderHistoryItemDto?> GetMyRenderAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await (
            from record in _dbContext.PromptRenderRecords.AsNoTracking()
            join prompt in _dbContext.PromptDefinitions.AsNoTracking()
                on record.PromptDefinitionId equals prompt.Id
            where record.Id == id && record.UserId == userId
            select new
            {
                record.Id,
                PromptId = record.PromptDefinitionId,
                PromptSlug = prompt.Slug,
                PromptName = prompt.Name,
                record.VersionNumber,
                record.Succeeded,
                record.DurationMilliseconds,
                record.InputPreview,
                record.RenderedPreview,
                record.ErrorCode,
                record.RenderedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new PromptRenderHistoryItemDto(
            row.Id,
            row.PromptId,
            row.PromptSlug.Value,
            row.PromptName,
            row.VersionNumber,
            row.Succeeded,
            row.DurationMilliseconds,
            row.InputPreview,
            row.RenderedPreview,
            row.ErrorCode,
            row.RenderedAtUtc);
    }

    private static void EnsureValidPaging(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > PromptLabPaging.MaxPageSize)
        {
            throw new PromptLabException(
                $"Page must be >= 1 and pageSize must be between 1 and {PromptLabPaging.MaxPageSize}.",
                PromptLabApplicationErrorCodes.PaginationInvalid);
        }
    }
}
