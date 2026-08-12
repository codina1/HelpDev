using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain.ContentAnalytics;

namespace HelpDev.Modules.Analytics.Application.ContentAnalytics;

public interface IContentAnalyticsQueries
{
    Task<ContentAnalyticsOverviewDto> GetContentOverviewAsync(
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentPerformanceDto>> GetTopContentAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);

    Task<ContentPerformanceDto?> GetContentPerformanceAsync(
        Guid contentId,
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentHealthIndicatorDto>> GetContentHealthAsync(
        AnalyticsDateRange range,
        int limit,
        CancellationToken cancellationToken = default);

    Task<ContentHealthIndicatorDto?> GetContentHealthByIdAsync(
        Guid contentId,
        AnalyticsDateRange range,
        CancellationToken cancellationToken = default);
}

/// <summary>Port for editorial content facts used by health indicators (no Analytics→Content.Infrastructure).</summary>
public interface IContentAnalyticsFactsSource
{
    Task<ContentAnalyticsFacts?> GetByIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentAnalyticsFacts>> ListRecentAsync(
        int take,
        CancellationToken cancellationToken = default);
}

public sealed record ContentAnalyticsFacts(
    Guid ContentId,
    string Title,
    string? Slug,
    string Status,
    DateTime UpdatedAtUtc,
    int RevisionCount,
    bool MissingSeoTitle,
    bool MissingSeoDescription,
    bool MissingCoverImage,
    bool MissingCanonical);

public sealed record ContentMetricDto(
    ContentMetricType MetricType,
    long Value,
    DateOnly PeriodStartUtc,
    DateOnly PeriodEndUtc);

public sealed record ContentAnalyticsOverviewDto(
    AnalyticsDateRange Range,
    long TotalViews,
    long ContentCreated,
    long ContentPublished,
    int ContentsWithViews,
    IReadOnlyList<ContentMetricDto> SupportedMetrics);

public sealed record ContentPerformanceDto(
    Guid ContentId,
    string Title,
    string? Slug,
    long Views,
    IReadOnlyList<ContentMetricDto> Metrics,
    DateTime GeneratedAtUtc);

public sealed record ContentHealthIndicatorDto(
    Guid ContentId,
    string Title,
    string? Status,
    ContentHealthStatus HealthStatus,
    IReadOnlyList<string> Reasons,
    long? ViewsInPeriod,
    int RevisionCount,
    DateTime UpdatedAtUtc);
