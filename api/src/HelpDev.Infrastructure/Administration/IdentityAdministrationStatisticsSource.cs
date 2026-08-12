using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Identity.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Administration;

public sealed class IdentityAdministrationStatisticsSource : IIdentityAdministrationStatisticsSource
{
    private readonly IIdentityDbContext _dbContext;

    public IdentityAdministrationStatisticsSource(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdentityAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var todayStart = utcNow.Date;
        var activeSince = utcNow.AddDays(-30);

        var totalUsers = await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);
        var activeUsers = await _dbContext.Users.AsNoTracking()
            .CountAsync(user => user.LastLogin != null && user.LastLogin >= activeSince, cancellationToken);
        var registrationsToday = await _dbContext.Users.AsNoTracking()
            .CountAsync(user => user.CreatedAt >= todayStart, cancellationToken);

        var recentUsers = await _dbContext.Users.AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .ThenByDescending(user => user.Id)
            .Take(AdministrationDashboardQueries.RecentItemLimit)
            .Select(user => new RecentAdminActivityDto(
                "user",
                user.Id,
                user.FullName,
                user.CreatedAt))
            .ToListAsync(cancellationToken);

        return new IdentityAdministrationStatistics(
            totalUsers,
            activeUsers,
            registrationsToday,
            recentUsers);
    }
}
