using HelpDev.Modules.Analytics.Domain.ContentAnalytics;

namespace HelpDev.Analytics.Tests;

public sealed class ContentAnalyticsDomainTests
{
    [Fact]
    public void Snapshot_is_immutable_and_rejects_negative_value()
    {
        var id = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;
        var snap = new ContentAnalyticsSnapshot(id, ContentMetricType.View, 12, start, end, DateTime.UtcNow);

        Assert.Equal(12, snap.Value);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContentAnalyticsSnapshot(id, ContentMetricType.View, -1, start, end, DateTime.UtcNow));
    }

    [Fact]
    public void Only_View_is_supported_in_v1()
    {
        Assert.True(ContentMetricTypeCatalog.IsSupported(ContentMetricType.View));
        Assert.False(ContentMetricTypeCatalog.IsSupported(ContentMetricType.Favorite));
        Assert.False(ContentMetricTypeCatalog.IsSupported(ContentMetricType.Share));
        Assert.Equal("content.views", ContentMetricTypeCatalog.ToMetricKey(ContentMetricType.View));
    }

    [Fact]
    public void Health_evaluator_reports_missing_seo_without_score()
    {
        var result = ContentHealthEvaluator.Evaluate(
            new ContentHealthInput(
                DateTime.UtcNow.AddDays(-10),
                RevisionCount: 2,
                MissingSeoTitle: true,
                MissingSeoDescription: false,
                MissingCoverImage: false,
                ViewsInPeriod: 5),
            DateTime.UtcNow);

        Assert.Equal(ContentHealthStatus.Critical, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("Missing SEO title", StringComparison.Ordinal));
        Assert.Null(typeof(ContentHealthResult).GetProperty("Score"));
    }

    [Fact]
    public void Health_evaluator_flags_stale_content()
    {
        var result = ContentHealthEvaluator.Evaluate(
            new ContentHealthInput(
                DateTime.UtcNow.AddDays(-(ContentHealthEvaluator.StaleDaysThreshold + 1)),
                RevisionCount: 1,
                MissingSeoTitle: false,
                MissingSeoDescription: false,
                MissingCoverImage: false,
                ViewsInPeriod: 1),
            DateTime.UtcNow);

        Assert.Equal(ContentHealthStatus.NeedsAttention, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("not updated recently", StringComparison.Ordinal));
    }

    [Fact]
    public void Health_evaluator_handles_empty_reasons_as_healthy()
    {
        var result = ContentHealthEvaluator.Evaluate(
            new ContentHealthInput(
                DateTime.UtcNow.AddDays(-1),
                RevisionCount: 3,
                MissingSeoTitle: false,
                MissingSeoDescription: false,
                MissingCoverImage: false,
                ViewsInPeriod: 10),
            DateTime.UtcNow);

        Assert.Equal(ContentHealthStatus.Healthy, result.Status);
        Assert.Empty(result.Reasons);
    }
}
