using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Domain.Tools;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class ToolRepository : IToolRepository
{
    private readonly IContentDbContext _dbContext;

    public ToolRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ToolMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) =>
        _dbContext.ToolMetadata
            .Include(tool => tool.Features)
            .Include(tool => tool.Alternatives)
            .FirstOrDefaultAsync(tool => tool.ContentId == contentId, cancellationToken);

    public Task<ToolMetadata?> GetByIdAsync(Guid toolId, CancellationToken cancellationToken = default) =>
        _dbContext.ToolMetadata
            .Include(tool => tool.Features)
            .Include(tool => tool.Alternatives)
            .FirstOrDefaultAsync(tool => tool.Id == toolId, cancellationToken);

    public Task AddAsync(ToolMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _dbContext.ToolMetadata.Add(metadata);
        return Task.CompletedTask;
    }

    public Task AddFeatureAsync(ToolFeature feature, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(feature);
        _dbContext.ToolFeatures.Add(feature);
        return Task.CompletedTask;
    }

    public async Task<int> GetNextFeatureOrderAsync(Guid toolId, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.ToolFeatures.AnyAsync(f => f.ToolId == toolId, cancellationToken).ConfigureAwait(false))
        {
            return 0;
        }

        var max = await _dbContext.ToolFeatures
            .Where(f => f.ToolId == toolId)
            .MaxAsync(f => f.Order, cancellationToken)
            .ConfigureAwait(false);
        return max + 1;
    }
}
