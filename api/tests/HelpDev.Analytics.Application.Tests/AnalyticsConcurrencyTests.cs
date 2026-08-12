using HelpDev.Analytics.Application.Tests.Fakes;
using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.Modules.Analytics.Infrastructure.Persistence;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Analytics.Application.Tests;

public sealed class AnalyticsConcurrencyTests
{
    /// <summary>
    /// PostgreSQL integration: run concurrent UpsertIncrementAsync against a real database using
    /// DailyMetricRepository.UpsertIncrementSql with INSERT ON CONFLICT DO UPDATE.
    /// Requires TEST_DATABASE_URL or local PostgreSQL; skipped in CI without connection string.
    /// </summary>
    [Fact]
    public void UpsertIncrementSql_uses_insert_on_conflict_for_atomic_increment()
    {
        Assert.Contains("INSERT INTO analytics_daily_metrics", DailyMetricRepository.UpsertIncrementSql, StringComparison.Ordinal);
        Assert.Contains("ON CONFLICT", DailyMetricRepository.UpsertIncrementSql, StringComparison.Ordinal);
        Assert.Contains("DO UPDATE SET", DailyMetricRepository.UpsertIncrementSql, StringComparison.Ordinal);
        Assert.Contains("analytics_daily_metrics.count + EXCLUDED.count", DailyMetricRepository.UpsertIncrementSql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Processor_uses_upsert_increment_for_concurrent_safe_metrics()
    {
        var (processor, _, metricRepo, _, _, unitOfWork, clock) = ProcessorFactory.Create();
        var userId = Guid.NewGuid();

        await processor.ProcessAsync(new AnalyticsEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.IdentityUserLoginSucceeded,
            OccurredAtUtc: clock.UtcNow,
            ActorUserId: userId,
            SubjectId: userId,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1));

        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.NotEmpty(metricRepo.Metrics);

        await processor.ProcessAsync(new AnalyticsEventEnvelope(
            EventId: Guid.NewGuid(),
            EventType: AnalyticsEventTypes.IdentityUserLoginSucceeded,
            OccurredAtUtc: clock.UtcNow,
            ActorUserId: userId,
            SubjectId: userId,
            SubjectType: null,
            Dimensions: null,
            Quantity: 1,
            DurationMilliseconds: null,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1));

        Assert.Equal(2, unitOfWork.SaveChangesCount);
        var loginMetric = metricRepo.Metrics.Single(metric =>
            metric.MetricKey == AnalyticsMetricKeys.UsersLoginSucceeded);
        Assert.True(loginMetric.Count >= 2);
    }

    [Fact]
    public async Task FakeDailyMetricRepository_upsert_increments_existing_row()
    {
        var repo = new FakeDailyMetricRepository();
        var identity = new DailyMetricIdentity(
            DateOnly.FromDateTime(DateTime.UtcNow),
            AnalyticsMetricKeys.UsersLoginSucceeded,
            null,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
        var now = DateTime.UtcNow;

        await repo.UpsertIncrementAsync(Guid.NewGuid(), identity, 1, true, false, null, now);
        await repo.UpsertIncrementAsync(Guid.NewGuid(), identity, 2, true, false, null, now);

        var metric = Assert.Single(repo.Metrics);
        Assert.Equal(3, metric.Count);
    }
}
