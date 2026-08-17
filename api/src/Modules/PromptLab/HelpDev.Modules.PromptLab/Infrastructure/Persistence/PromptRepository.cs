using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptRepository : IPromptRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Prompt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Prompts.FirstOrDefaultAsync(prompt => prompt.Id == id, cancellationToken);

    public Task<bool> ExistsBySlugAsync(
        string slug,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var slugValue = PromptSlug.FromPersisted(slug);
        var query = _dbContext.Prompts.Where(prompt => prompt.Slug == slugValue);
        if (excludingId.HasValue)
        {
            query = query.Where(prompt => prompt.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prompt);
        await _dbContext.Prompts.AddAsync(prompt, cancellationToken);
    }
}
