using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Content.Application.Roadmaps;

public sealed class RoadmapQueries : IRoadmapQueries
{
    private readonly IContentDbContext _dbContext;

    public RoadmapQueries(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RoadmapListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var query =
            from roadmap in _dbContext.RoadmapMetadata.AsNoTracking()
            join content in _dbContext.Contents.AsNoTracking() on roadmap.ContentId equals content.Id
            where content.Type == ContentType.Roadmap
            select new { roadmap, content };

        if (!actor.CanManageAllContent)
        {
            query = query.Where(row => row.content.AuthorId == actor.UserId);
        }

        var rows = await query
            .OrderByDescending(row => row.roadmap.UpdatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new RoadmapListItemDto(
                row.roadmap.Id,
                row.roadmap.ContentId,
                row.roadmap.Level.ToString(),
                row.roadmap.EstimatedDuration,
                row.content.Slug.Value,
                row.content.Status.ToString(),
                row.content.Title,
                row.roadmap.UpdatedAtUtc))
            .ToList();
    }
}
