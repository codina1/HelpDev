using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Administration.Application.Dashboard;

public sealed class AdministrationDashboardQueries : IAdministrationDashboardQueries
{
    public const int RecentItemLimit = 5;

    private readonly IIdentityAdministrationStatisticsSource _identity;
    private readonly IContentAdministrationStatisticsSource _content;
    private readonly ILearningAdministrationStatisticsSource _learning;
    private readonly ISearchAdministrationStatisticsSource _search;
    private readonly IOutboxAdministrationStatisticsSource _outbox;
    private readonly IAnalyticsAdministrationStatisticsSource _analytics;
    private readonly IDateTimeProvider _clock;

    public AdministrationDashboardQueries(
        IIdentityAdministrationStatisticsSource identity,
        IContentAdministrationStatisticsSource content,
        ILearningAdministrationStatisticsSource learning,
        ISearchAdministrationStatisticsSource search,
        IOutboxAdministrationStatisticsSource outbox,
        IAnalyticsAdministrationStatisticsSource analytics,
        IDateTimeProvider clock)
    {
        _identity = identity;
        _content = content;
        _learning = learning;
        _search = search;
        _outbox = outbox;
        _analytics = analytics;
        _clock = clock;
    }

    public async Task<AdministrationDashboardDto> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = _clock.UtcNow;

            // Sequential by design: adapters may share a scoped DbContext that is not thread-safe.
            var users = await _identity.GetAsync(now, cancellationToken);
            var content = await _content.GetAsync(now, cancellationToken);
            var learning = await _learning.GetAsync(now, cancellationToken);
            var search = await _search.GetAsync(cancellationToken);
            var outbox = await _outbox.GetAsync(cancellationToken);
            var analytics = await _analytics.GetAsync(cancellationToken);

            var recentItems = users.RecentUsers
                .Concat(content.RecentPublishedContent)
                .Concat(learning.RecentPublishedCourses)
                .OrderByDescending(item => item.OccurredAtUtc)
                .ThenBy(item => item.Id)
                .ToList();

            return new AdministrationDashboardDto(
                new UserStatisticsDto(users.TotalUsers, users.ActiveUsers, users.RegistrationsToday),
                new ContentStatisticsDto(
                    content.TotalContent,
                    content.PublishedContent,
                    content.DraftContent,
                    content.PublicationsToday),
                new LearningStatisticsDto(
                    learning.TotalCourses,
                    learning.PublishedCourses,
                    learning.TotalEnrollments,
                    learning.EnrollmentsToday),
                new SearchStatisticsDto(
                    search.TotalSearchDocuments,
                    search.PublishedSearchDocuments,
                    search.LastIndexedAtUtc),
                new OutboxStatisticsDto(
                    outbox.Pending,
                    outbox.Processing,
                    outbox.Failed,
                    outbox.Processed,
                    outbox.OldestPendingAtUtc,
                    outbox.LastProcessedAtUtc),
                new AnalyticsDashboardStatisticsDto(
                    analytics.RegistrationsLast30Days,
                    analytics.ActiveUsersLast30Days,
                    analytics.ContentViewsLast30Days,
                    analytics.EnrollmentsLast30Days,
                    analytics.ToolboxExecutionsLast30Days,
                    analytics.PromptRendersLast30Days,
                    analytics.SearchesLast30Days,
                    analytics.SearchZeroResultRateLast30Days),
                recentItems);
        }
        catch (AdministrationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AdministrationException(
                "Administration dashboard is temporarily unavailable.",
                AdministrationApplicationErrorCodes.DashboardUnavailable,
                ex);
        }
    }
}
