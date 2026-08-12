using HelpDev.Modules.PromptLab.Application.Categories;
using HelpDev.Modules.PromptLab.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptCategoryQueries : IPromptCategoryQueries
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptCategoryQueries(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PromptCategoryAdminDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.PromptCategories
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
            .Select(row => new PromptCategoryAdminDto(
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

    public async Task<PromptCategoryAdminDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.PromptCategories
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

        return new PromptCategoryAdminDto(
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
