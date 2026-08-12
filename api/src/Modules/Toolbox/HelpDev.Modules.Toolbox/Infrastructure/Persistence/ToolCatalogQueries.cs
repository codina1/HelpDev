using HelpDev.Modules.Toolbox.Application.Catalog;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolCatalogQueries : IToolCatalogQueries
{
    private readonly IToolboxDbContext _dbContext;

    public ToolCatalogQueries(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ToolCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.ToolCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Icon,
                category.DisplayOrder,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ToolCategoryDto(
                row.Id,
                row.Name,
                row.Slug.Value,
                row.Description,
                row.Icon,
                row.DisplayOrder))
            .ToList();
    }

    public async Task<ToolCatalogPageDto> GetToolsAsync(
        ToolCatalogFilter filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        EnsureValidPaging(filter.Page, filter.PageSize);

        var query =
            from tool in _dbContext.ToolDefinitions.AsNoTracking()
            join category in _dbContext.ToolCategories.AsNoTracking()
                on tool.CategoryId equals category.Id
            where category.IsActive
                && tool.IsPublished
                && tool.IsEnabled
            select new { tool, category };

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            var categorySlug = ToolSlug.FromPersisted(filter.CategorySlug.Trim().ToLowerInvariant());
            query = query.Where(row => row.category.Slug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLowerInvariant();
            query = query.Where(row =>
                row.tool.Name.ToLower().Contains(term)
                || row.tool.Summary.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderBy(row => row.category.DisplayOrder)
            .ThenBy(row => row.tool.DisplayOrder)
            .ThenBy(row => row.tool.Name)
            .ThenBy(row => row.tool.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(row => new
            {
                row.tool.Id,
                ToolSlug = row.tool.Slug,
                row.tool.Name,
                row.tool.Summary,
                row.tool.Type,
                CategorySlug = row.category.Slug,
                CategoryName = row.category.Name,
                row.tool.RequiresAuthentication,
                row.tool.DisplayOrder,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(row => new ToolCatalogItemDto(
                row.Id,
                row.ToolSlug.Value,
                row.Name,
                row.Summary,
                row.Type.ToString(),
                row.CategorySlug.Value,
                row.CategoryName,
                row.RequiresAuthentication,
                row.DisplayOrder))
            .ToList();

        return new ToolCatalogPageDto(filter.Page, filter.PageSize, total, items);
    }

    public async Task<ToolDetailsDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var slugValue = ToolSlug.FromPersisted(slug);

        var row = await (
            from tool in _dbContext.ToolDefinitions.AsNoTracking()
            join category in _dbContext.ToolCategories.AsNoTracking()
                on tool.CategoryId equals category.Id
            where tool.Slug == slugValue
                && tool.IsPublished
                && tool.IsEnabled
                && category.IsActive
            select new
            {
                tool.Id,
                ToolSlug = tool.Slug,
                tool.Name,
                tool.Summary,
                tool.Description,
                tool.Type,
                tool.InputSchema,
                tool.ExampleInput,
                tool.RequiresAuthentication,
                tool.AllowHistory,
                tool.DisplayOrder,
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategorySlug = category.Slug,
                CategoryIcon = category.Icon,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new ToolDetailsDto(
            row.Id,
            row.ToolSlug.Value,
            row.Name,
            row.Summary,
            row.Description,
            row.Type.ToString(),
            row.InputSchema,
            row.ExampleInput,
            row.RequiresAuthentication,
            row.AllowHistory,
            row.DisplayOrder,
            new ToolDetailsCategoryDto(
                row.CategoryId,
                row.CategoryName,
                row.CategorySlug.Value,
                row.CategoryIcon));
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
