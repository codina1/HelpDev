namespace HelpDev.Modules.Administration.Application.Dashboard;

public sealed record AdministrationDashboardDto(
    UserStatisticsDto Users,
    ContentStatisticsDto Content,
    LearningStatisticsDto Learning,
    SearchStatisticsDto Search,
    OutboxStatisticsDto Outbox,
    AnalyticsDashboardStatisticsDto Analytics,
    IReadOnlyList<RecentAdminActivityDto> RecentItems);

public sealed record UserStatisticsDto(
    int TotalUsers,
    int? ActiveUsers,
    int RegistrationsToday);

public sealed record ContentStatisticsDto(
    int TotalContent,
    int PublishedContent,
    int DraftContent,
    int? PublicationsToday);

public sealed record LearningStatisticsDto(
    int TotalCourses,
    int PublishedCourses,
    int TotalEnrollments,
    int EnrollmentsToday);

public sealed record SearchStatisticsDto(
    int TotalSearchDocuments,
    int PublishedSearchDocuments,
    DateTime? LastIndexedAtUtc);

public sealed record OutboxStatisticsDto(
    int Pending,
    int Processing,
    int Failed,
    int Processed,
    DateTime? OldestPendingAtUtc,
    DateTime? LastProcessedAtUtc);

public sealed record RecentAdminActivityDto(
    string Category,
    Guid Id,
    string Title,
    DateTime OccurredAtUtc);

public interface IAdministrationDashboardQueries
{
    Task<AdministrationDashboardDto> GetAsync(CancellationToken cancellationToken = default);
}

public sealed record IdentityAdministrationStatistics(
    int TotalUsers,
    int? ActiveUsers,
    int RegistrationsToday,
    IReadOnlyList<RecentAdminActivityDto> RecentUsers);

public sealed record ContentAdministrationStatistics(
    int TotalContent,
    int PublishedContent,
    int DraftContent,
    int? PublicationsToday,
    IReadOnlyList<RecentAdminActivityDto> RecentPublishedContent);

public sealed record LearningAdministrationStatistics(
    int TotalCourses,
    int PublishedCourses,
    int TotalEnrollments,
    int EnrollmentsToday,
    IReadOnlyList<RecentAdminActivityDto> RecentPublishedCourses);

public sealed record SearchAdministrationStatistics(
    int TotalSearchDocuments,
    int PublishedSearchDocuments,
    DateTime? LastIndexedAtUtc);

public sealed record OutboxAdministrationStatistics(
    int Pending,
    int Processing,
    int Failed,
    int Processed,
    DateTime? OldestPendingAtUtc,
    DateTime? LastProcessedAtUtc);

public sealed record AnalyticsDashboardStatisticsDto(
    long RegistrationsLast30Days,
    long ActiveUsersLast30Days,
    long ContentViewsLast30Days,
    long EnrollmentsLast30Days,
    long ToolboxExecutionsLast30Days,
    long PromptRendersLast30Days,
    long SearchesLast30Days,
    decimal SearchZeroResultRateLast30Days);

public sealed record AnalyticsAdministrationStatistics(
    long RegistrationsLast30Days,
    long ActiveUsersLast30Days,
    long ContentViewsLast30Days,
    long EnrollmentsLast30Days,
    long ToolboxExecutionsLast30Days,
    long PromptRendersLast30Days,
    long SearchesLast30Days,
    decimal SearchZeroResultRateLast30Days);

public interface IIdentityAdministrationStatisticsSource
{
    Task<IdentityAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

public interface IContentAdministrationStatisticsSource
{
    Task<ContentAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

public interface ILearningAdministrationStatisticsSource
{
    Task<LearningAdministrationStatistics> GetAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

public interface ISearchAdministrationStatisticsSource
{
    Task<SearchAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default);
}

public interface IOutboxAdministrationStatisticsSource
{
    Task<OutboxAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default);
}

public interface IAnalyticsAdministrationStatisticsSource
{
    Task<AnalyticsAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default);
}
