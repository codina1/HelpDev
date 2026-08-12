using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Analytics.Tests;

public sealed class DailyMetricTests
{
    private static readonly DateTime Now = new(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    private static DailyMetric CreateMetric() =>
        DailyMetric.Create(
            Guid.NewGuid(),
            Today,
            AnalyticsMetricKeys.UsersRegistered,
            subjectId: null,
            subjectType: null,
            dimension1Key: string.Empty,
            dimension1Value: string.Empty,
            dimension2Key: string.Empty,
            dimension2Value: string.Empty,
            Now);

    [Fact]
    public void Create_returns_metric_with_zero_counts()
    {
        var metric = CreateMetric();

        Assert.Equal(0, metric.Count);
        Assert.Equal(0, metric.SuccessCount);
        Assert.Equal(0, metric.FailureCount);
        Assert.Equal(0, metric.TotalDurationMilliseconds);
    }

    [Fact]
    public void Create_throws_when_id_is_empty()
    {
        var ex = Assert.Throws<DomainException>(() =>
            DailyMetric.Create(
                Guid.Empty, Today, AnalyticsMetricKeys.UsersRegistered,
                null, null, string.Empty, string.Empty, string.Empty, string.Empty, Now));

        Assert.Equal(AnalyticsErrorCodes.EventProcessingFailed, ex.Message);
    }

    [Fact]
    public void Create_throws_for_unsupported_metric_key()
    {
        var ex = Assert.Throws<DomainException>(() =>
            DailyMetric.Create(
                Guid.NewGuid(), Today, "totally.unknown.key",
                null, null, string.Empty, string.Empty, string.Empty, string.Empty, Now));

        Assert.Equal(AnalyticsErrorCodes.MetricKeyInvalid, ex.Message);
    }

    [Fact]
    public void ApplyIncrement_increases_count()
    {
        var metric = CreateMetric();

        metric.ApplyIncrement(3, incrementSuccess: true, incrementFailure: false, durationMilliseconds: null, Now);

        Assert.Equal(3, metric.Count);
        Assert.Equal(3, metric.SuccessCount);
        Assert.Equal(0, metric.FailureCount);
    }

    [Fact]
    public void ApplyIncrement_zero_quantity_throws()
    {
        var metric = CreateMetric();

        var ex = Assert.Throws<DomainException>(() =>
            metric.ApplyIncrement(0, false, false, null, Now));

        Assert.Equal(AnalyticsErrorCodes.EventQuantityInvalid, ex.Message);
    }

    [Fact]
    public void ApplyIncrement_negative_quantity_throws()
    {
        var metric = CreateMetric();

        var ex = Assert.Throws<DomainException>(() =>
            metric.ApplyIncrement(-1, false, false, null, Now));

        Assert.Equal(AnalyticsErrorCodes.EventQuantityInvalid, ex.Message);
    }

    [Fact]
    public void ApplyIncrement_negative_duration_throws()
    {
        var metric = CreateMetric();

        var ex = Assert.Throws<DomainException>(() =>
            metric.ApplyIncrement(1, true, false, durationMilliseconds: -100, Now));

        Assert.Equal(AnalyticsErrorCodes.EventProcessingFailed, ex.Message);
    }

    [Fact]
    public void ApplyIncrement_tracks_duration_on_first_increment()
    {
        var metric = CreateMetric();

        metric.ApplyIncrement(1, true, false, durationMilliseconds: 250, Now);

        Assert.Equal(250, metric.MinDurationMilliseconds);
        Assert.Equal(250, metric.MaxDurationMilliseconds);
        Assert.Equal(250, metric.TotalDurationMilliseconds);
    }

    [Fact]
    public void ApplyIncrement_tracks_min_max_on_subsequent_increments()
    {
        var metric = CreateMetric();

        metric.ApplyIncrement(1, true, false, 300, Now);
        metric.ApplyIncrement(1, true, false, 100, Now);
        metric.ApplyIncrement(1, true, false, 500, Now);

        Assert.Equal(100, metric.MinDurationMilliseconds);
        Assert.Equal(500, metric.MaxDurationMilliseconds);
        Assert.Equal(900, metric.TotalDurationMilliseconds);
    }

    [Fact]
    public void ApplyIncrement_increments_failure_count()
    {
        var metric = CreateMetric();

        metric.ApplyIncrement(2, incrementSuccess: false, incrementFailure: true, null, Now);

        Assert.Equal(2, metric.Count);
        Assert.Equal(0, metric.SuccessCount);
        Assert.Equal(2, metric.FailureCount);
    }

    [Fact]
    public void ApplyIncrement_updates_timestamp()
    {
        var metric = CreateMetric();
        var later = Now.AddMinutes(5);

        metric.ApplyIncrement(1, true, false, null, later);

        Assert.Equal(later, metric.UpdatedAtUtc);
    }
}
