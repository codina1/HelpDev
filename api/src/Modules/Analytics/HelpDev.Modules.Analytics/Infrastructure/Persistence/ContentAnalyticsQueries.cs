using HelpDev.Modules.Analytics.Application.ContentAnalytics;
using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.ContentAnalytics;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

/// <summary>
/// Content analytics read model over existing <c>analytics_daily_metrics</c>.
/// No duplicate storage. Global totals use SubjectId == null to avoid double-counting
/// with subject-scoped view rows.
/// </summary>
public sealed class ContentAnalyticsQueries : IContentAnalyticsQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly IContentAnalyticsFactsSource _factsSource;
    private readonly AnalyticsOptions _options;
    private readonly IDateTimeProvider _clock;

    public ContentAnalyticsQueries(
        IAnalyticsDbContext dbContext,
        IContentAnalyticsFactsSource factsSource,
        IOptions<AnalyticsOptions> options,
        IDateTimeProvider clock)
    {
        _dbContext = dbContext;
        _factsSource = factsSource;
        _options = options.Value;
        _clock = clock;
    }

    public async Task<ContentAnalyticsOverviewDto> GetContentOverviewAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);

        var metrics = _dbContext.DailyMetrics.AsNoTracking()
            .Where(m => m.DateUtc >= range.FromUtc && m.DateUtc <= range.ToUtc);

        var totalViews = await metrics
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews && m.SubjectId == null)
            .SumAsync(m => (long?)m.Count, cancellationToken) ?? 0;

        var created = await metrics
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentCreated && m.SubjectId == null)
            .SumAsync(m => (long?)m.Count, cancellationToken) ?? 0;

        var published = await metrics
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentPublished && m.SubjectId == null)
            .SumAsync(m => (long?)m.Count, cancellationToken) ?? 0;

        var contentsWithViews = await metrics
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews && m.SubjectId != null)
            .Select(m => m.SubjectId!.Value)
            .Distinct()
            .CountAsync(cancellationToken);

        var supported = new List<ContentMetricDto>
        {
            new(ContentMetricType.View, totalViews, range.FromUtc, range.ToUtc),
        };

        return new ContentAnalyticsOverviewDto(
            range,
            totalViews,
            created,
            published,
            contentsWithViews,
            supported);
    }

    public async Task<IReadOnlyList<ContentPerformanceDto>> GetTopContentAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        limit = Math.Clamp(limit, 1, _options.MaxTopLimit);
        var generatedAt = _clock.UtcNow;

        var ranked = await _dbContext.DailyMetrics.AsNoTracking()
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews
                        && m.SubjectId != null
                        && m.DateUtc >= range.FromUtc
                        && m.DateUtc <= range.ToUtc)
            .GroupBy(m => m.SubjectId!.Value)
            .Select(g => new { ContentId = g.Key, Views = g.Sum(x => x.Count) })
            .OrderByDescending(x => x.Views)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (ranked.Count == 0)
        {
            return [];
        }

        var ids = ranked.Select(r => r.ContentId).ToList();
        var snapshots = await _dbContext.AnalyticsSubjectSnapshots.AsNoTracking()
            .Where(s => ids.Contains(s.SubjectId) && s.SubjectType == AnalyticsSubjectTypes.Content)
            .ToDictionaryAsync(s => s.SubjectId, cancellationToken);

        return ranked.Select(row =>
        {
            snapshots.TryGetValue(row.ContentId, out var snap);
            return new ContentPerformanceDto(
                row.ContentId,
                snap?.DisplayName ?? row.ContentId.ToString("N"),
                snap?.Slug,
                row.Views,
                [new ContentMetricDto(ContentMetricType.View, row.Views, range.FromUtc, range.ToUtc)],
                generatedAt);
        }).ToList();
    }

    public async Task<ContentPerformanceDto?> GetContentPerformanceAsync(
        Guid contentId,
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        var generatedAt = _clock.UtcNow;

        var views = await _dbContext.DailyMetrics.AsNoTracking()
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews
                        && m.SubjectId == contentId
                        && m.DateUtc >= range.FromUtc
                        && m.DateUtc <= range.ToUtc)
            .SumAsync(m => (long?)m.Count, cancellationToken) ?? 0;

        var facts = await _factsSource.GetByIdAsync(contentId, cancellationToken);
        var snapshot = await _dbContext.AnalyticsSubjectSnapshots.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.SubjectId == contentId && s.SubjectType == AnalyticsSubjectTypes.Content,
                cancellationToken);

        var title = facts?.Title ?? snapshot?.DisplayName;
        if (title is null && views == 0 && facts is null)
        {
            return null;
        }

        return new ContentPerformanceDto(
            contentId,
            title ?? contentId.ToString("N"),
            facts?.Slug ?? snapshot?.Slug,
            views,
            [new ContentMetricDto(ContentMetricType.View, views, range.FromUtc, range.ToUtc)],
            generatedAt);
    }

    public async Task<IReadOnlyList<ContentHealthIndicatorDto>> GetContentHealthAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        limit = Math.Clamp(limit, 1, _options.MaxTopLimit);

        var facts = await _factsSource.ListRecentAsync(limit * 2, cancellationToken);
        if (facts.Count == 0)
        {
            return [];
        }

        var viewLookup = await LoadViewsLookupAsync(
            facts.Select(f => f.ContentId).ToList(),
            range,
            cancellationToken);

        var now = _clock.UtcNow;
        var results = new List<ContentHealthIndicatorDto>();
        foreach (var fact in facts)
        {
            viewLookup.TryGetValue(fact.ContentId, out var views);
            var health = ContentHealthEvaluator.Evaluate(
                new ContentHealthInput(
                    fact.UpdatedAtUtc,
                    fact.RevisionCount,
                    fact.MissingSeoTitle,
                    fact.MissingSeoDescription,
                    fact.MissingCoverImage,
                    views),
                now);

            results.Add(new ContentHealthIndicatorDto(
                fact.ContentId,
                fact.Title,
                fact.Status,
                health.Status,
                health.Reasons,
                views,
                fact.RevisionCount,
                fact.UpdatedAtUtc));
        }

        return results
            .OrderByDescending(r => r.HealthStatus)
            .ThenBy(r => r.UpdatedAtUtc)
            .Take(limit)
            .ToList();
    }

    public async Task<ContentHealthIndicatorDto?> GetContentHealthByIdAsync(
        Guid contentId,
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default)
    {
        range.Validate(_options);
        var fact = await _factsSource.GetByIdAsync(contentId, cancellationToken);
        if (fact is null)
        {
            return null;
        }

        var views = await _dbContext.DailyMetrics.AsNoTracking()
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews
                        && m.SubjectId == contentId
                        && m.DateUtc >= range.FromUtc
                        && m.DateUtc <= range.ToUtc)
            .SumAsync(m => (long?)m.Count, cancellationToken);

        var health = ContentHealthEvaluator.Evaluate(
            new ContentHealthInput(
                fact.UpdatedAtUtc,
                fact.RevisionCount,
                fact.MissingSeoTitle,
                fact.MissingSeoDescription,
                fact.MissingCoverImage,
                views ?? 0),
            _clock.UtcNow);

        return new ContentHealthIndicatorDto(
            fact.ContentId,
            fact.Title,
            fact.Status,
            health.Status,
            health.Reasons,
            views ?? 0,
            fact.RevisionCount,
            fact.UpdatedAtUtc);
    }

    private async Task<Dictionary<Guid, long>> LoadViewsLookupAsync(
        IReadOnlyList<Guid> contentIds,
        AnalyticsDateRange range,
        CancellationToken cancellationToken)
    {
        if (contentIds.Count == 0)
        {
            return new Dictionary<Guid, long>();
        }

        var rows = await _dbContext.DailyMetrics.AsNoTracking()
            .Where(m => m.MetricKey == AnalyticsMetricKeys.ContentViews
                        && m.SubjectId != null
                        && contentIds.Contains(m.SubjectId.Value)
                        && m.DateUtc >= range.FromUtc
                        && m.DateUtc <= range.ToUtc)
            .GroupBy(m => m.SubjectId!.Value)
            .Select(g => new { ContentId = g.Key, Views = g.Sum(x => x.Count) })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.ContentId, r => r.Views);
    }
}
