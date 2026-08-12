namespace HelpDev.SharedContracts.Analytics;

public sealed record AnalyticsEventEnvelope(
    Guid EventId,
    string EventType,
    DateTime OccurredAtUtc,
    Guid? ActorUserId,
    Guid? SubjectId,
    string? SubjectType,
    IReadOnlyDictionary<string, string>? Dimensions,
    long Quantity = 1,
    long? DurationMilliseconds = null,
    string? SubjectDisplayName = null,
    string? SubjectSlug = null,
    int SchemaVersion = 1);
