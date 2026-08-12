using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolDefinitionRepository : IToolDefinitionRepository
{
    private readonly IToolboxDbContext _dbContext;

    public ToolDefinitionRepository(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ToolDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.ToolDefinitions.FirstOrDefaultAsync(tool => tool.Id == id, cancellationToken);

    public Task<ToolDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = ToolSlug.FromPersisted(slug);
        return _dbContext.ToolDefinitions.FirstOrDefaultAsync(
            tool => tool.Slug == slugValue,
            cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = ToolSlug.FromPersisted(slug);
        return _dbContext.ToolDefinitions.AnyAsync(
            tool => tool.Slug == slugValue,
            cancellationToken);
    }

    public async Task AddAsync(ToolDefinition tool, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tool);
        await _dbContext.ToolDefinitions.AddAsync(tool, cancellationToken);
    }
}
