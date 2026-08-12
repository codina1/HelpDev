namespace HelpDev.Modules.Analytics.Domain.ContentAnalytics;

/// <summary>
/// Immutable period snapshot of a content metric. Analytical output only —
/// no update/delete methods.
/// </summary>
public sealed class ContentAnalyticsSnapshot
{
    public ContentAnalyticsSnapshot(
        Guid contentId,
        ContentMetricType metricType,
        long value,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        DateTime generatedAtUtc)
    {
        if (contentId == Guid.Empty)
        {
            throw new ArgumentException("ContentId is required.", nameof(contentId));
        }

        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");
        }

        if (periodEndUtc < periodStartUtc)
        {
            throw new ArgumentException("PeriodEndUtc must be on or after PeriodStartUtc.");
        }

        ContentId = contentId;
        MetricType = metricType;
        Value = value;
        PeriodStartUtc = periodStartUtc;
        PeriodEndUtc = periodEndUtc;
        GeneratedAtUtc = generatedAtUtc;
    }

    public Guid ContentId { get; }

    public ContentMetricType MetricType { get; }

    public long Value { get; }

    public DateTime PeriodStartUtc { get; }

    public DateTime PeriodEndUtc { get; }

    public DateTime GeneratedAtUtc { get; }
}
