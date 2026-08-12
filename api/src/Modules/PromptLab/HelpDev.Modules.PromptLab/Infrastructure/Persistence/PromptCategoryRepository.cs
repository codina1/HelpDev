using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptCategoryRepository : IPromptCategoryRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptCategoryRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PromptCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.PromptCategories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<PromptCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug);
        return _dbContext.PromptCategories.FirstOrDefaultAsync(
            category => category.Slug == slugValue,
            cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug);
        return _dbContext.PromptCategories.AnyAsync(
            category => category.Slug == slugValue,
            cancellationToken);
    }

    public async Task AddAsync(PromptCategory category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        await _dbContext.PromptCategories.AddAsync(category, cancellationToken);
    }
}
