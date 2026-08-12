using HelpDev.Modules.Toolbox.Application.Categories;
using HelpDev.Modules.Toolbox.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolCategoryQueries : IToolCategoryQueries
{
    private readonly IToolboxDbContext _dbContext;

    public ToolCategoryQueries(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ToolCategoryAdminDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.ToolCategories
            .AsNoTracking()
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
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ToolCategoryAdminDto(
                row.Id,
                row.Name,
                row.Slug.Value,
                row.Description,
                row.Icon,
                row.DisplayOrder,
                row.IsActive,
                row.CreatedAtUtc,
                row.UpdatedAtUtc))
            .ToList();
    }

    public async Task<ToolCategoryAdminDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.ToolCategories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Icon,
                category.DisplayOrder,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new ToolCategoryAdminDto(
            row.Id,
            row.Name,
            row.Slug.Value,
            row.Description,
            row.Icon,
            row.DisplayOrder,
            row.IsActive,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
