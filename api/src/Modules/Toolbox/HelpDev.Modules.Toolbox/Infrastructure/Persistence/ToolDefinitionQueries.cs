using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Application.Tools;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolDefinitionQueries : IToolDefinitionQueries
{
    private readonly IToolboxDbContext _dbContext;

    public ToolDefinitionQueries(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ToolDefinitionAdminDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.ToolDefinitions
            .AsNoTracking()
            .Where(tool => tool.Id == id)
            .Select(tool => new ToolDefinitionRow(
                tool.Id,
                tool.CategoryId,
                tool.Name,
                tool.Slug,
                tool.Summary,
                tool.Description,
                tool.Type,
                tool.InputSchema,
                tool.ExampleInput,
                tool.IsPublished,
                tool.IsEnabled,
                tool.RequiresAuthentication,
                tool.AllowHistory,
                tool.DisplayOrder,
                tool.CreatedAtUtc,
                tool.UpdatedAtUtc,
                tool.PublishedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToDto(row);
    }

    public async Task<ToolDefinitionPageDto> GetPageAsync(
        ToolDefinitionFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.Page < 1 || filter.PageSize < 1 || filter.PageSize > ToolboxPaging.MaxPageSize)
        {
            throw new ToolboxException(
                $"Page must be >= 1 and pageSize must be between 1 and {ToolboxPaging.MaxPageSize}.",
                ToolboxApplicationErrorCodes.PaginationInvalid);
        }

        var query = _dbContext.ToolDefinitions.AsNoTracking().AsQueryable();

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(tool => tool.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Type)
            && Enum.TryParse<ToolType>(filter.Type.Trim(), ignoreCase: true, out var toolType)
            && Enum.IsDefined(toolType))
        {
            query = query.Where(tool => tool.Type == toolType);
        }

        if (filter.IsPublished.HasValue)
        {
            query = query.Where(tool => tool.IsPublished == filter.IsPublished.Value);
        }

        if (filter.IsEnabled.HasValue)
        {
            query = query.Where(tool => tool.IsEnabled == filter.IsEnabled.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(tool => tool.DisplayOrder)
            .ThenBy(tool => tool.Name)
            .ThenBy(tool => tool.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(tool => new ToolDefinitionRow(
                tool.Id,
                tool.CategoryId,
                tool.Name,
                tool.Slug,
                tool.Summary,
                tool.Description,
                tool.Type,
                tool.InputSchema,
                tool.ExampleInput,
                tool.IsPublished,
                tool.IsEnabled,
                tool.RequiresAuthentication,
                tool.AllowHistory,
                tool.DisplayOrder,
                tool.CreatedAtUtc,
                tool.UpdatedAtUtc,
                tool.PublishedAtUtc))
            .ToListAsync(cancellationToken);

        var items = rows.Select(ToDto).ToList();
        return new ToolDefinitionPageDto(filter.Page, filter.PageSize, total, items);
    }

    private static ToolDefinitionAdminDto ToDto(ToolDefinitionRow row) =>
        new(
            row.Id,
            row.CategoryId,
            row.Name,
            row.Slug.Value,
            row.Summary,
            row.Description,
            row.Type.ToString(),
            row.InputSchema,
            row.ExampleInput,
            row.IsPublished,
            row.IsEnabled,
            row.RequiresAuthentication,
            row.AllowHistory,
            row.DisplayOrder,
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            row.PublishedAtUtc);

    private sealed record ToolDefinitionRow(
        Guid Id,
        Guid CategoryId,
        string Name,
        ToolSlug Slug,
        string Summary,
        string? Description,
        ToolType Type,
        string InputSchema,
        string? ExampleInput,
        bool IsPublished,
        bool IsEnabled,
        bool RequiresAuthentication,
        bool AllowHistory,
        int DisplayOrder,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        DateTime? PublishedAtUtc);
}
