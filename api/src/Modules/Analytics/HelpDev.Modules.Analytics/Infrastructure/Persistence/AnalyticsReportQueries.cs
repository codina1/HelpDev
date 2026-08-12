using HelpDev.Modules.Analytics.Application;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedContracts.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AnalyticsTimeSeriesQueries : IAnalyticsTimeSeriesQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly AnalyticsOptions _options;

    public AnalyticsTimeSeriesQueries(IAnalyticsDbContext dbContext, IOptions<AnalyticsOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<AnalyticsTimeSeriesDto> GetTimeSeriesAsync(
        AnalyticsTimeSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AnalyticsMetricKeys.IsSupported(request.MetricKey))
        {
            throw new AnalyticsException("Metric key is invalid.", AnalyticsApplicationErrorCodes.MetricKeyInvalid);
        }

        var range = new AnalyticsDateRange(request.FromUtc, request.ToUtc);
        range.Validate(_options);

        var query = _dbContext.DailyMetrics
            .AsNoTracking()
            .Where(metric => metric.MetricKey == request.MetricKey
                && metric.DateUtc >= request.FromUtc
                && metric.DateUtc <= request.ToUtc);

        if (request.SubjectId.HasValue)
        {
            query = query.Where(metric => metric.SubjectId == request.SubjectId);
        }

        if (!string.IsNullOrWhiteSpace(request.DimensionKey))
        {
            query = query.Where(metric =>
                metric.Dimension1Key == request.DimensionKey
                || metric.Dimension2Key == request.DimensionKey);
        }

        if (!string.IsNullOrWhiteSpace(request.DimensionValue))
        {
            query = query.Where(metric =>
                metric.Dimension1Value == request.DimensionValue
                || metric.Dimension2Value == request.DimensionValue);
        }

        var grouped = await query
            .GroupBy(metric => metric.DateUtc)
            .Select(group => new
            {
                DateUtc = group.Key,
                Count = group.Sum(metric => metric.Count),
                SuccessCount = group.Sum(metric => metric.SuccessCount),
                FailureCount = group.Sum(metric => metric.FailureCount),
                TotalDuration = group.Sum(metric => metric.TotalDurationMilliseconds),
                DurationCount = group.Sum(metric => metric.Count),
            })
            .ToListAsync(cancellationToken);

        var lookup = grouped.ToDictionary(item => item.DateUtc);
        var points = new List<AnalyticsTimeSeriesPointDto>();
        for (var date = request.FromUtc; date <= request.ToUtc; date = date.AddDays(1))
        {
            if (lookup.TryGetValue(date, out var row))
            {
                points.Add(new AnalyticsTimeSeriesPointDto(
                    date,
                    row.Count,
                    row.SuccessCount,
                    row.FailureCount,
                    row.DurationCount == 0 ? null : row.TotalDuration / row.DurationCount));
            }
            else
            {
                points.Add(new AnalyticsTimeSeriesPointDto(date, 0, 0, 0, null));
            }
        }

        return new AnalyticsTimeSeriesDto(request.MetricKey, request.FromUtc, request.ToUtc, points);
    }
}

public sealed class AnalyticsTopItemsQueries : IAnalyticsTopItemsQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly AnalyticsOptions _options;

    public AnalyticsTopItemsQueries(IAnalyticsDbContext dbContext, IOptions<AnalyticsOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopContentAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default) =>
        GetTopAsync(range, limit, AnalyticsMetricKeys.ContentViews, AnalyticsSubjectTypes.Content, cancellationToken);

    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopCoursesAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default) =>
        GetTopAsync(range, limit, AnalyticsMetricKeys.LearningEnrollments, AnalyticsSubjectTypes.Course, cancellationToken);

    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopToolsAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default) =>
        GetTopAsync(range, limit, AnalyticsMetricKeys.ToolboxExecutions, AnalyticsSubjectTypes.Tool, cancellationToken);

    public Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopPromptsAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default) =>
        GetTopAsync(range, limit, AnalyticsMetricKeys.PromptLabRenders, AnalyticsSubjectTypes.Prompt, cancellationToken);

    private async Task<IReadOnlyList<AnalyticsTopItemDto>> GetTopAsync(
        AnalyticsDateRange range,
        int limit,
        string metricKey,
        string subjectType,
        CancellationToken cancellationToken)
    {
        range.Validate(_options);
        ValidateLimit(limit);

        var aggregates = await _dbContext.DailyMetrics
            .AsNoTracking()
            .Where(metric =>
                metric.MetricKey == metricKey
                && metric.SubjectType == subjectType
                && metric.SubjectId != null
                && metric.DateUtc >= range.FromUtc
                && metric.DateUtc <= range.ToUtc)
            .GroupBy(metric => metric.SubjectId)
            .Select(group => new
            {
                SubjectId = group.Key!.Value,
                MetricValue = group.Sum(metric => metric.Count),
                SuccessCount = group.Sum(metric => metric.SuccessCount),
                FailureCount = group.Sum(metric => metric.FailureCount),
                TotalDuration = group.Sum(metric => metric.TotalDurationMilliseconds),
                DurationCount = group.Sum(metric => metric.Count),
            })
            .OrderByDescending(item => item.MetricValue)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var subjectIds = aggregates.Select(item => item.SubjectId).ToList();
        var snapshots = await _dbContext.AnalyticsSubjectSnapshots
            .AsNoTracking()
            .Where(snapshot => snapshot.SubjectType == subjectType && subjectIds.Contains(snapshot.SubjectId))
            .ToDictionaryAsync(snapshot => snapshot.SubjectId, cancellationToken);

        return aggregates
            .Select(item =>
            {
                snapshots.TryGetValue(item.SubjectId, out var snapshot);
                return new AnalyticsTopItemDto(
                    item.SubjectId,
                    snapshot?.DisplayName ?? subjectType,
                    snapshot?.Slug,
                    item.MetricValue,
                    item.SuccessCount,
                    item.FailureCount,
                    item.DurationCount == 0 ? null : item.TotalDuration / item.DurationCount);
            })
            .OrderByDescending(item => item.MetricValue)
            .ThenBy(item => item.DisplayName, StringComparer.Ordinal)
            .ThenBy(item => item.SubjectId)
            .ToList();
    }

    private void ValidateLimit(int limit)
    {
        if (limit < 1 || limit > _options.MaxTopLimit)
        {
            throw new AnalyticsException("Limit is invalid.", AnalyticsApplicationErrorCodes.LimitInvalid);
        }
    }
}

public sealed class SearchAnalyticsQueries : ISearchAnalyticsQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly AnalyticsOptions _options;

    public SearchAnalyticsQueries(IAnalyticsDbContext dbContext, IOptions<AnalyticsOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<SearchAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);

        var metrics = _dbContext.DailyMetrics.AsNoTracking()
            .Where(metric => metric.DateUtc >= range.FromUtc && metric.DateUtc <= range.ToUtc);

        var totalSearches = await metrics
            .Where(metric => metric.MetricKey == AnalyticsMetricKeys.SearchExecutions)
            .SumAsync(metric => metric.Count, cancellationToken);

        var zeroResults = await metrics
            .Where(metric => metric.MetricKey == AnalyticsMetricKeys.SearchZeroResults)
            .SumAsync(metric => metric.Count, cancellationToken);

        var bucketRows = await metrics
            .Where(metric => metric.MetricKey == AnalyticsMetricKeys.SearchExecutions
                && metric.Dimension1Key == AnalyticsDimensionKeys.ResultBucket)
            .GroupBy(metric => metric.Dimension1Value)
            .Select(group => new { Bucket = group.Key, Count = group.Sum(metric => metric.Count) })
            .ToListAsync(cancellationToken);

        var authenticated = await metrics
            .Where(metric => metric.MetricKey == AnalyticsMetricKeys.SearchExecutions
                && metric.Dimension2Key == AnalyticsDimensionKeys.IsAuthenticated
                && metric.Dimension2Value == "true")
            .SumAsync(metric => metric.Count, cancellationToken);

        var anonymous = await metrics
            .Where(metric => metric.MetricKey == AnalyticsMetricKeys.SearchExecutions
                && metric.Dimension2Key == AnalyticsDimensionKeys.IsAuthenticated
                && metric.Dimension2Value == "false")
            .SumAsync(metric => metric.Count, cancellationToken);

        var timeSeries = new AnalyticsTimeSeriesQueries(_dbContext, Options.Create(_options));
        var daily = await timeSeries.GetTimeSeriesAsync(
            new AnalyticsTimeSeriesRequest(AnalyticsMetricKeys.SearchExecutions, range.FromUtc, range.ToUtc),
            cancellationToken);

        return new SearchAnalyticsDto(
            totalSearches,
            zeroResults,
            AnalyticsMetricQueryHelper.SafeRate(zeroResults, totalSearches),
            authenticated,
            anonymous,
            bucketRows.ToDictionary(row => row.Bucket, row => row.Count, StringComparer.Ordinal),
            daily.Points);
    }
}

public sealed class ToolboxAnalyticsQueries : IToolboxAnalyticsQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly IAnalyticsTopItemsQueries _topItems;
    private readonly AnalyticsOptions _options;

    public ToolboxAnalyticsQueries(
        IAnalyticsDbContext dbContext,
        IAnalyticsTopItemsQueries topItems,
        IOptions<AnalyticsOptions> options)
    {
        _dbContext = dbContext;
        _topItems = topItems;
        _options = options.Value;
    }

    public Task<ExecutionAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        return BuildExecutionAnalyticsAsync(
            range,
            AnalyticsMetricKeys.ToolboxExecutions,
            AnalyticsMetricKeys.ToolboxExecutionsSucceeded,
            AnalyticsMetricKeys.ToolboxExecutionsFailed,
            AnalyticsMetricKeys.ToolboxExecutionDuration,
            _topItems.GetTopToolsAsync,
            _dbContext,
            _options,
            cancellationToken);
    }

    internal static async Task<ExecutionAnalyticsDto> BuildExecutionAnalyticsAsync(
        AnalyticsDateRange range,
        string totalKey,
        string successKey,
        string failureKey,
        string durationKey,
        Func<AnalyticsDateRange, int, CancellationToken, Task<IReadOnlyList<AnalyticsTopItemDto>>> topItems,
        IAnalyticsDbContext dbContext,
        AnalyticsOptions options,
        CancellationToken cancellationToken)
    {
        var metrics = dbContext.DailyMetrics.AsNoTracking();
        var total = AnalyticsMetricQueryHelper.SumCount(metrics, totalKey, range);
        var success = AnalyticsMetricQueryHelper.SumCount(metrics, successKey, range);
        var failure = AnalyticsMetricQueryHelper.SumCount(metrics, failureKey, range);
        var duration = AnalyticsMetricQueryHelper.SumDuration(metrics, durationKey, range);

        var failureCodes = await metrics
            .Where(metric => metric.MetricKey == failureKey
                && metric.DateUtc >= range.FromUtc
                && metric.DateUtc <= range.ToUtc
                && metric.Dimension1Key == AnalyticsDimensionKeys.ErrorCode)
            .GroupBy(metric => metric.Dimension1Value)
            .Select(group => new { Code = group.Key, Count = group.Sum(metric => metric.Count) })
            .ToListAsync(cancellationToken);

        var tops = await topItems(range, options.DefaultTopLimit, cancellationToken);

        return new ExecutionAnalyticsDto(
            total,
            success,
            failure,
            AnalyticsMetricQueryHelper.SafeRate(success, total),
            duration.TotalCount == 0 ? 0 : duration.TotalDuration / duration.TotalCount,
            tops,
            failureCodes.ToDictionary(row => row.Code, row => row.Count, StringComparer.Ordinal));
    }
}

public sealed class PromptLabAnalyticsQueries : IPromptLabAnalyticsQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly IAnalyticsTopItemsQueries _topItems;
    private readonly AnalyticsOptions _options;

    public PromptLabAnalyticsQueries(
        IAnalyticsDbContext dbContext,
        IAnalyticsTopItemsQueries topItems,
        IOptions<AnalyticsOptions> options)
    {
        _dbContext = dbContext;
        _topItems = topItems;
        _options = options.Value;
    }

    public Task<ExecutionAnalyticsDto> GetAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        return ToolboxAnalyticsQueries.BuildExecutionAnalyticsAsync(
            range,
            AnalyticsMetricKeys.PromptLabRenders,
            AnalyticsMetricKeys.PromptLabRendersSucceeded,
            AnalyticsMetricKeys.PromptLabRendersFailed,
            AnalyticsMetricKeys.PromptLabRenderDuration,
            _topItems.GetTopPromptsAsync,
            _dbContext,
            _options,
            cancellationToken);
    }
}
