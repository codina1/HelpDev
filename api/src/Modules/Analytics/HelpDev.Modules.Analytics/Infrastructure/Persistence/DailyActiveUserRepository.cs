using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class DailyActiveUserRepository : IDailyActiveUserRepository
{
    private readonly IAnalyticsDbContext _dbContext;

    public DailyActiveUserRepository(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(DateOnly dateUtc, Guid userId, CancellationToken cancellationToken = default) =>
        _dbContext.DailyActiveUsers
            .AsNoTracking()
            .AnyAsync(marker => marker.DateUtc == dateUtc && marker.UserId == userId, cancellationToken);

    public async Task AddAsync(DailyActiveUser marker, CancellationToken cancellationToken = default)
    {
        await _dbContext.DailyActiveUsers.AddAsync(marker, cancellationToken);
    }
}
