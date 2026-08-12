using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Administration;

public sealed class ContentAdministrationStatisticsSource : IContentAdministrationStatisticsSource
{
    private readonly IContentDbContext _dbContext;

    public ContentAdministrationStatisticsSource(IContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContentAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var totals = await _dbContext.Contents.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Published = group.Count(item => item.Status == ContentStatus.Published),
                Draft = group.Count(item => item.Status == ContentStatus.Draft),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var recentPublished = await _dbContext.Contents.AsNoTracking()
            .Where(item => item.Status == ContentStatus.Published)
            .OrderByDescending(item => item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Take(AdministrationDashboardQueries.RecentItemLimit)
            .Select(item => new RecentAdminActivityDto(
                "content",
                item.Id,
                item.Title,
                item.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ContentAdministrationStatistics(
            totals?.Total ?? 0,
            totals?.Published ?? 0,
            totals?.Draft ?? 0,
            PublicationsToday: null,
            recentPublished);
    }
}
