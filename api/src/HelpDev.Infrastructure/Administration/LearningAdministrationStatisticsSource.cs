using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Administration;

public sealed class LearningAdministrationStatisticsSource : ILearningAdministrationStatisticsSource
{
    private readonly ILearningDbContext _dbContext;

    public LearningAdministrationStatisticsSource(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LearningAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var todayStart = utcNow.Date;

        var courseTotals = await _dbContext.Courses.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Total = group.Count(),
                Published = group.Count(item => item.Status == CourseStatus.Published),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalEnrollments = await _dbContext.Enrollments.AsNoTracking().CountAsync(cancellationToken);
        var enrollmentsToday = await _dbContext.Enrollments.AsNoTracking()
            .CountAsync(item => item.EnrolledAt >= todayStart, cancellationToken);

        var recentPublished = await _dbContext.Courses.AsNoTracking()
            .Where(item => item.Status == CourseStatus.Published)
            .OrderByDescending(item => item.PublishedAt ?? item.CreatedAt)
            .ThenByDescending(item => item.Id)
            .Take(AdministrationDashboardQueries.RecentItemLimit)
            .Select(item => new RecentAdminActivityDto(
                "course",
                item.Id,
                item.Title,
                item.PublishedAt ?? item.CreatedAt))
            .ToListAsync(cancellationToken);

        return new LearningAdministrationStatistics(
            courseTotals?.Total ?? 0,
            courseTotals?.Published ?? 0,
            totalEnrollments,
            enrollmentsToday,
            recentPublished);
    }
}
