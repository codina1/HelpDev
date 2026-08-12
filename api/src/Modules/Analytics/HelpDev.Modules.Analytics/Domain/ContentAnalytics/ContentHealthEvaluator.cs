namespace HelpDev.Modules.Analytics.Domain.ContentAnalytics;

public enum ContentHealthStatus
{
    Healthy = 0,
    NeedsAttention = 1,
    Critical = 2,
    Unknown = 3,
}

/// <summary>
/// Pure health evaluation from real facts only. No numeric score or ranking.
/// </summary>
public static class ContentHealthEvaluator
{
    public const int StaleDaysThreshold = 90;
    public const int CriticalStaleDaysThreshold = 180;

    public static ContentHealthResult Evaluate(
        ContentHealthInput input,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(input);

        var reasons = new List<string>();

        var ageDays = (nowUtc - input.UpdatedAtUtc).TotalDays;
        if (ageDays >= CriticalStaleDaysThreshold)
        {
            reasons.Add("Content not updated for a long time");
        }
        else if (ageDays >= StaleDaysThreshold)
        {
            reasons.Add("Content not updated recently");
        }

        if (input.MissingSeoTitle)
        {
            reasons.Add("Missing SEO title");
        }

        if (input.MissingSeoDescription)
        {
            reasons.Add("Missing SEO description");
        }

        if (input.MissingCoverImage)
        {
            reasons.Add("Missing cover image");
        }

        if (input.RevisionCount == 0)
        {
            reasons.Add("No revision history yet");
        }

        // Views are optional — only mention when we know the period had zero subject views.
        if (input.ViewsInPeriod.HasValue && input.ViewsInPeriod.Value == 0)
        {
            reasons.Add("No recorded views in the selected period");
        }

        var status = reasons.Count == 0
            ? ContentHealthStatus.Healthy
            : reasons.Any(r => r.Contains("long time", StringComparison.Ordinal)
                               || r.Contains("Missing SEO title", StringComparison.Ordinal))
                ? ContentHealthStatus.Critical
                : ContentHealthStatus.NeedsAttention;

        if (reasons.Count == 0 && !input.ViewsInPeriod.HasValue)
        {
            // No metric data does not invent a healthy label when engagement is unknown —
            // still Healthy for editorial factors that passed; Unknown only when no content facts.
            status = ContentHealthStatus.Healthy;
        }

        return new ContentHealthResult(status, reasons);
    }
}

public sealed record ContentHealthInput(
    DateTime UpdatedAtUtc,
    int RevisionCount,
    bool MissingSeoTitle,
    bool MissingSeoDescription,
    bool MissingCoverImage,
    long? ViewsInPeriod);

public sealed record ContentHealthResult(
    ContentHealthStatus Status,
    IReadOnlyList<string> Reasons);
