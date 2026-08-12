using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptDefinitionRepository : IPromptDefinitionRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptDefinitionRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PromptDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.PromptDefinitions
            .Include(prompt => prompt.Versions)
            .ThenInclude(version => version.Variables)
            .FirstOrDefaultAsync(prompt => prompt.Id == id, cancellationToken);

    public Task<PromptDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug);
        return _dbContext.PromptDefinitions
            .Include(prompt => prompt.Versions)
            .ThenInclude(version => version.Variables)
            .FirstOrDefaultAsync(prompt => prompt.Slug == slugValue, cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug);
        return _dbContext.PromptDefinitions.AnyAsync(
            prompt => prompt.Slug == slugValue,
            cancellationToken);
    }

    public async Task AddAsync(PromptDefinition prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        await _dbContext.PromptDefinitions.AddAsync(prompt, cancellationToken);
    }
}
