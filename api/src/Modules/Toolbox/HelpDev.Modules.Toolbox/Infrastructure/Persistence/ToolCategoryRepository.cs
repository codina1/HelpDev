using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolCategoryRepository : IToolCategoryRepository
{
    private readonly IToolboxDbContext _dbContext;

    public ToolCategoryRepository(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ToolCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ToolCategories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<ToolCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = ToolSlug.FromPersisted(slug);
        return _dbContext.ToolCategories.FirstOrDefaultAsync(
            category => category.Slug == slugValue,
            cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = ToolSlug.FromPersisted(slug);
        return _dbContext.ToolCategories.AnyAsync(
            category => category.Slug == slugValue,
            cancellationToken);
    }

    public async Task AddAsync(ToolCategory category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        await _dbContext.ToolCategories.AddAsync(category, cancellationToken);
    }
}
