using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class DailyMetricRepository : IDailyMetricRepository
{
    /// <summary>
    /// PostgreSQL upsert for atomic daily metric increments (INSERT ON CONFLICT DO UPDATE).
    /// </summary>
    public const string UpsertIncrementSql = """
        INSERT INTO analytics_daily_metrics (
            "Id", date_utc, metric_key, subject_id, subject_type,
            dimension1_key, dimension1_value, dimension2_key, dimension2_value,
            count, success_count, failure_count,
            total_duration_milliseconds, min_duration_milliseconds, max_duration_milliseconds,
            created_at_utc, updated_at_utc)
        VALUES (
            {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8},
            {9}, {10}, {11}, {12}, {13}, {14}, {15}, {15})
        ON CONFLICT (date_utc, metric_key, subject_id, subject_type, dimension1_key, dimension1_value, dimension2_key, dimension2_value)
        DO UPDATE SET
            count = analytics_daily_metrics.count + EXCLUDED.count,
            success_count = analytics_daily_metrics.success_count + EXCLUDED.success_count,
            failure_count = analytics_daily_metrics.failure_count + EXCLUDED.failure_count,
            total_duration_milliseconds = analytics_daily_metrics.total_duration_milliseconds + EXCLUDED.total_duration_milliseconds,
            min_duration_milliseconds = CASE
                WHEN {16} IS NULL THEN analytics_daily_metrics.min_duration_milliseconds
                WHEN analytics_daily_metrics.count = 0 THEN EXCLUDED.min_duration_milliseconds
                ELSE LEAST(analytics_daily_metrics.min_duration_milliseconds, EXCLUDED.min_duration_milliseconds)
            END,
            max_duration_milliseconds = CASE
                WHEN {16} IS NULL THEN analytics_daily_metrics.max_duration_milliseconds
                WHEN analytics_daily_metrics.count = 0 THEN EXCLUDED.max_duration_milliseconds
                ELSE GREATEST(analytics_daily_metrics.max_duration_milliseconds, EXCLUDED.max_duration_milliseconds)
            END,
            updated_at_utc = EXCLUDED.updated_at_utc
        """;

    private readonly IAnalyticsDbContext _dbContext;

    public DailyMetricRepository(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DailyMetric?> GetForUpdateAsync(
        DailyMetricIdentity identity,
        CancellationToken cancellationToken = default) =>
        _dbContext.DailyMetrics.FirstOrDefaultAsync(
            metric =>
                metric.DateUtc == identity.DateUtc
                && metric.MetricKey == identity.MetricKey
                && metric.SubjectId == identity.SubjectId
                && metric.SubjectType == identity.SubjectType
                && metric.Dimension1Key == identity.Dimension1Key
                && metric.Dimension1Value == identity.Dimension1Value
                && metric.Dimension2Key == identity.Dimension2Key
                && metric.Dimension2Value == identity.Dimension2Value,
            cancellationToken);

    public async Task AddAsync(DailyMetric metric, CancellationToken cancellationToken = default)
    {
        await _dbContext.DailyMetrics.AddAsync(metric, cancellationToken);
    }

    public async Task UpsertIncrementAsync(
        Guid newMetricId,
        DailyMetricIdentity identity,
        long quantity,
        bool incrementSuccess,
        bool incrementFailure,
        long? durationMilliseconds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (_dbContext is not DbContext dbContext)
        {
            throw new InvalidOperationException("Analytics db context must be an EF DbContext for upsert increments.");
        }

        var successDelta = incrementSuccess ? quantity : 0L;
        var failureDelta = incrementFailure ? quantity : 0L;
        var totalDurationDelta = durationMilliseconds.HasValue ? durationMilliseconds.Value * quantity : 0L;
        var insertMinDuration = durationMilliseconds ?? 0L;
        var insertMaxDuration = durationMilliseconds ?? 0L;
        var hasDuration = durationMilliseconds.HasValue;

        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO analytics_daily_metrics (
                "Id", date_utc, metric_key, subject_id, subject_type,
                dimension1_key, dimension1_value, dimension2_key, dimension2_value,
                count, success_count, failure_count,
                total_duration_milliseconds, min_duration_milliseconds, max_duration_milliseconds,
                created_at_utc, updated_at_utc)
            VALUES (
                {newMetricId}, {identity.DateUtc}, {identity.MetricKey}, {identity.SubjectId}, {identity.SubjectType},
                {identity.Dimension1Key}, {identity.Dimension1Value}, {identity.Dimension2Key}, {identity.Dimension2Value},
                {quantity}, {successDelta}, {failureDelta}, {totalDurationDelta}, {insertMinDuration}, {insertMaxDuration},
                {nowUtc}, {nowUtc})
            ON CONFLICT (date_utc, metric_key, subject_id, subject_type, dimension1_key, dimension1_value, dimension2_key, dimension2_value)
            DO UPDATE SET
                count = analytics_daily_metrics.count + EXCLUDED.count,
                success_count = analytics_daily_metrics.success_count + EXCLUDED.success_count,
                failure_count = analytics_daily_metrics.failure_count + EXCLUDED.failure_count,
                total_duration_milliseconds = analytics_daily_metrics.total_duration_milliseconds + EXCLUDED.total_duration_milliseconds,
                min_duration_milliseconds = CASE
                    WHEN {hasDuration} = FALSE THEN analytics_daily_metrics.min_duration_milliseconds
                    WHEN analytics_daily_metrics.count = 0 THEN EXCLUDED.min_duration_milliseconds
                    ELSE LEAST(analytics_daily_metrics.min_duration_milliseconds, EXCLUDED.min_duration_milliseconds)
                END,
                max_duration_milliseconds = CASE
                    WHEN {hasDuration} = FALSE THEN analytics_daily_metrics.max_duration_milliseconds
                    WHEN analytics_daily_metrics.count = 0 THEN EXCLUDED.max_duration_milliseconds
                    ELSE GREATEST(analytics_daily_metrics.max_duration_milliseconds, EXCLUDED.max_duration_milliseconds)
                END,
                updated_at_utc = EXCLUDED.updated_at_utc
            """, cancellationToken);
    }
}
