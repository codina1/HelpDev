using HelpDev.Modules.Analytics.Application;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Analytics.Application.Queries;

public sealed record AnalyticsDateRange(DateOnly FromUtc, DateOnly ToUtc)
{
    public static AnalyticsDateRange Last30Days(DateOnly todayUtc) =>
        new(todayUtc.AddDays(-29), todayUtc);

    public void Validate(AnalyticsOptions options)
    {
        if (FromUtc > ToUtc)
        {
            throw new AnalyticsException(
                "Analytics date range is invalid.",
                AnalyticsApplicationErrorCodes.DateRangeInvalid);
        }

        var spanDays = ToUtc.DayNumber - FromUtc.DayNumber + 1;
        if (spanDays > options.MaxQueryRangeDays)
        {
            throw new AnalyticsException(
                $"Analytics date range cannot exceed {options.MaxQueryRangeDays} days.",
                AnalyticsApplicationErrorCodes.DateRangeTooLarge);
        }
    }
}

public sealed record AnalyticsOverviewDto(
    AnalyticsDateRange Range,
    AnalyticsUsersOverviewDto Users,
    AnalyticsContentOverviewDto Content,
    AnalyticsLearningOverviewDto Learning,
    AnalyticsSearchOverviewDto Search,
    AnalyticsToolboxOverviewDto Toolbox,
    AnalyticsPromptLabOverviewDto PromptLab);

public sealed record AnalyticsUsersOverviewDto(
    long Registrations,
    long ActiveUsers,
    long SuccessfulLogins);

public sealed record AnalyticsContentOverviewDto(
    long Views,
    long Created,
    long Published);

public sealed record AnalyticsLearningOverviewDto(
    long CoursesCreated,
    long CoursesPublished,
    long Enrollments,
    long LessonsCompleted);

public sealed record AnalyticsSearchOverviewDto(
    long Searches,
    long ZeroResultSearches,
    decimal ZeroResultRate);

public sealed record AnalyticsToolboxOverviewDto(
    long Executions,
    long SuccessfulExecutions,
    long FailedExecutions,
    decimal SuccessRate,
    long AverageDurationMilliseconds);

public sealed record AnalyticsPromptLabOverviewDto(
    long Renders,
    long SuccessfulRenders,
    long FailedRenders,
    decimal SuccessRate,
    long AverageDurationMilliseconds);

public interface IAnalyticsOverviewQueries
{
    Task<AnalyticsOverviewDto> GetOverviewAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);
}

public sealed record AnalyticsTimeSeriesRequest(
    string MetricKey,
    DateOnly FromUtc,
    DateOnly ToUtc,
    Guid? SubjectId = null,
    string? DimensionKey = null,
    string? DimensionValue = null);

public sealed record AnalyticsTimeSeriesPointDto(
    DateOnly DateUtc,
    long Count,
    long SuccessCount,
    long FailureCount,
    long? AverageDurationMilliseconds);

public sealed record AnalyticsTimeSeriesDto(
    string MetricKey,
    DateOnly FromUtc,
    DateOnly ToUtc,
    IReadOnlyList<AnalyticsTimeSeriesPointDto> Points);

public interface IAnalyticsTimeSeriesQueries
{
    Task<AnalyticsTimeSeriesDto> GetTimeSeriesAsync(
        AnalyticsTimeSeriesRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AnalyticsTopItemDto(
    Guid SubjectId,
    string DisplayName,
    string? Slug,
    long MetricValue,
    long? SuccessCount,
    long? FailureCount,
    long? AverageDurationMilliseconds);

public interface IAnalyticsTopItemsQueries
{
    Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopContentAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopCoursesAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopToolsAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopPromptsAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record SearchAnalyticsDto(
    long TotalSearches,
    long ZeroResultSearches,
    decimal ZeroResultRate,
    long AuthenticatedSearches,
    long AnonymousSearches,
    IReadOnlyDictionary<string, long> ResultBucketDistribution,
    IReadOnlyList<AnalyticsTimeSeriesPointDto> DailySeries);

public interface ISearchAnalyticsQueries
{
    Task<SearchAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);
}

public sealed record ExecutionAnalyticsDto(
    long Total,
    long SuccessCount,
    long FailureCount,
    decimal SuccessRate,
    long AverageDurationMilliseconds,
    IReadOnlyList<AnalyticsTopItemDto> TopItems,
    IReadOnlyDictionary<string, long> FailureCodeDistribution);

public interface IToolboxAnalyticsQueries
{
    Task<ExecutionAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);
}

public interface IPromptLabAnalyticsQueries
{
    Task<ExecutionAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);
}
