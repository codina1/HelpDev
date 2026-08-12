using HelpDev.Modules.Administration.Application.Dashboard;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Administration;

public sealed class AnalyticsAdministrationStatisticsSource : IAnalyticsAdministrationStatisticsSource
{
    private readonly IAnalyticsOverviewQueries _overviewQueries;
    private readonly IDateTimeProvider _clock;

    public AnalyticsAdministrationStatisticsSource(
        IAnalyticsOverviewQueries overviewQueries,
        IDateTimeProvider clock)
    {
        _overviewQueries = overviewQueries;
        _clock = clock;
    }

    public async Task<AnalyticsAdministrationStatistics> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_clock.UtcNow);
        var range = new AnalyticsDateRange(today.AddDays(-29), today);
        var overview = await _overviewQueries.GetOverviewAsync(range, cancellationToken);

        return new AnalyticsAdministrationStatistics(
            overview.Users.Registrations,
            overview.Users.ActiveUsers,
            overview.Content.Views,
            overview.Learning.Enrollments,
            overview.Toolbox.Executions,
            overview.PromptLab.Renders,
            overview.Search.Searches,
            overview.Search.ZeroResultRate);
    }
}
