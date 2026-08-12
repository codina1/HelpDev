using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Search.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Administration;

public sealed class SearchAdministrationStatisticsSource : ISearchAdministrationStatisticsSource
{
    private readonly ISearchDbContext _dbContext;

    public SearchAdministrationStatisticsSource(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default)
    {
        var totals = await _dbContext.SearchDocuments.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Published = group.Count(item => item.IsPublished),
                LastIndexedAtUtc = group.Max(item => (DateTime?)item.IndexedAtUtc),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new SearchAdministrationStatistics(
            totals?.Total ?? 0,
            totals?.Published ?? 0,
            totals?.LastIndexedAtUtc);
    }
}
