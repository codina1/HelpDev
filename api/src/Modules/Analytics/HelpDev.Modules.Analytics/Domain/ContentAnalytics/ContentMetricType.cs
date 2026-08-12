namespace HelpDev.Modules.Analytics.Domain.ContentAnalytics;

/// <summary>
/// Content analytics metric kinds. Only <see cref="View"/> is produced today
/// (<c>content.item_viewed</c> → <c>content.views</c>). Other values are reserved
/// until real producers exist — never invent traffic for them.
/// </summary>
public enum ContentMetricType
{
    View = 0,
    SearchClick = 1,
    Favorite = 2,
    Save = 3,
    Share = 4,
    Completion = 5,
}

public static class ContentMetricTypeCatalog
{
    /// <summary>Metric types that currently have producers and storage.</summary>
    public static IReadOnlyList<ContentMetricType> Supported { get; } = [ContentMetricType.View];

    public static bool IsSupported(ContentMetricType type) => Supported.Contains(type);

    public static string ToMetricKey(ContentMetricType type) =>
        type switch
        {
            ContentMetricType.View => AnalyticsMetricKeys.ContentViews,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Metric type has no producer in Content Analytics v1."),
        };

    public static ContentMetricType? TryFromMetricKey(string metricKey) =>
        metricKey switch
        {
            AnalyticsMetricKeys.ContentViews => ContentMetricType.View,
            _ => null,
        };
}
