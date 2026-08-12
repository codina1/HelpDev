using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

internal static class AnalyticsMetricQueryHelper
{
    public static IQueryable<DailyMetric> FilterRange(IQueryable<DailyMetric> query, AnalyticsDateRange range) =>
        query.Where(metric => metric.DateUtc >= range.FromUtc && metric.DateUtc <= range.ToUtc);

    public static long SumCount(IQueryable<DailyMetric> query, string metricKey, AnalyticsDateRange range) =>
        FilterRange(query.Where(metric => metric.MetricKey == metricKey), range)
            .Sum(metric => metric.Count);

    public static (long TotalDuration, long TotalCount) SumDuration(
        IQueryable<DailyMetric> query,
        string metricKey,
        AnalyticsDateRange range)
    {
        var rows = FilterRange(query.Where(metric => metric.MetricKey == metricKey), range)
            .Select(metric => new { metric.TotalDurationMilliseconds, metric.Count })
            .ToList();

        return (rows.Sum(row => row.TotalDurationMilliseconds), rows.Sum(row => row.Count));
    }

    public static decimal SafeRate(long numerator, long denominator) =>
        denominator == 0 ? 0m : Math.Round((decimal)numerator / denominator, 4);
}

public sealed class AnalyticsOverviewQueries : IAnalyticsOverviewQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly AnalyticsOptions _options;
    private readonly IDateTimeProvider _clock;

    public AnalyticsOverviewQueries(
        IAnalyticsDbContext dbContext,
        IOptions<AnalyticsOptions> options,
        IDateTimeProvider clock)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _clock = clock;
    }

    public async Task<AnalyticsOverviewDto> GetOverviewAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);

        var metrics = _dbContext.DailyMetrics.AsNoTracking();
        var activeUsers = await _dbContext.DailyActiveUsers
            .AsNoTracking()
            .Where(marker => marker.DateUtc >= range.FromUtc && marker.DateUtc <= range.ToUtc)
            .Select(marker => marker.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var toolboxExecutions = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ToolboxExecutions, range);
        var toolboxSucceeded = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ToolboxExecutionsSucceeded, range);
        var toolboxFailed = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ToolboxExecutionsFailed, range);
        var toolboxDuration = AnalyticsMetricQueryHelper.SumDuration(metrics, AnalyticsMetricKeys.ToolboxExecutionDuration, range);

        var promptRenders = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.PromptLabRenders, range);
        var promptSucceeded = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.PromptLabRendersSucceeded, range);
        var promptFailed = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.PromptLabRendersFailed, range);
        var promptDuration = AnalyticsMetricQueryHelper.SumDuration(metrics, AnalyticsMetricKeys.PromptLabRenderDuration, range);

        var searches = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.SearchExecutions, range);
        var zeroResults = AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.SearchZeroResults, range);

        return new AnalyticsOverviewDto(
            range,
            new AnalyticsUsersOverviewDto(
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.UsersRegistered, range),
                activeUsers,
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.UsersLoginSucceeded, range)),
            new AnalyticsContentOverviewDto(
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ContentViews, range),
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ContentCreated, range),
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.ContentPublished, range)),
            new AnalyticsLearningOverviewDto(
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.LearningCoursesCreated, range),
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.LearningCoursesPublished, range),
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.LearningEnrollments, range),
                AnalyticsMetricQueryHelper.SumCount(metrics, AnalyticsMetricKeys.LearningLessonsCompleted, range)),
            new AnalyticsSearchOverviewDto(
                searches,
                zeroResults,
                AnalyticsMetricQueryHelper.SafeRate(zeroResults, searches)),
            new AnalyticsToolboxOverviewDto(
                toolboxExecutions,
                toolboxSucceeded,
                toolboxFailed,
                AnalyticsMetricQueryHelper.SafeRate(toolboxSucceeded, toolboxExecutions),
                toolboxDuration.TotalCount == 0 ? 0 : toolboxDuration.TotalDuration / toolboxDuration.TotalCount),
            new AnalyticsPromptLabOverviewDto(
                promptRenders,
                promptSucceeded,
                promptFailed,
                AnalyticsMetricQueryHelper.SafeRate(promptSucceeded, promptRenders),
                promptDuration.TotalCount == 0 ? 0 : promptDuration.TotalDuration / promptDuration.TotalCount));
    }
}
