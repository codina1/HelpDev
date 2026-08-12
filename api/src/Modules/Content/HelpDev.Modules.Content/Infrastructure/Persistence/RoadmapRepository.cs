using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Roadmaps;
using HelpDev.Modules.Content.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Infrastructure.Persistence;

public sealed class RoadmapRepository : IRoadmapRepository
{
    private readonly IContentDbContext _dbContext;

    public RoadmapRepository(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RoadmapMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) =>
        _dbContext.RoadmapMetadata
            .Include(roadmap => roadmap.Steps)
            .ThenInclude(step => step.Topics)
            .Include(roadmap => roadmap.Steps)
            .ThenInclude(step => step.Resources)
            .FirstOrDefaultAsync(roadmap => roadmap.ContentId == contentId, cancellationToken);

    public Task<RoadmapMetadata?> GetByIdAsync(Guid roadmapId, CancellationToken cancellationToken = default) =>
        _dbContext.RoadmapMetadata
            .Include(roadmap => roadmap.Steps)
            .ThenInclude(step => step.Topics)
            .Include(roadmap => roadmap.Steps)
            .ThenInclude(step => step.Resources)
            .FirstOrDefaultAsync(roadmap => roadmap.Id == roadmapId, cancellationToken);

    public Task AddAsync(RoadmapMetadata metadata, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _dbContext.RoadmapMetadata.Add(metadata);
        return Task.CompletedTask;
    }

    public Task AddStepAsync(RoadmapStep step, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(step);
        _dbContext.RoadmapSteps.Add(step);
        return Task.CompletedTask;
    }
}
