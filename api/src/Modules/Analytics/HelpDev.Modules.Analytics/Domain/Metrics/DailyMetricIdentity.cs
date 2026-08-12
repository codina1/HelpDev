namespace HelpDev.Modules.Analytics.Domain.Metrics;

public sealed record DailyMetricIdentity(
    DateOnly DateUtc,
    string MetricKey,
    Guid? SubjectId,
    string? SubjectType,
    string Dimension1Key,
    string Dimension1Value,
    string Dimension2Key,
    string Dimension2Value);
