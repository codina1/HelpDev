namespace HelpDev.SharedContracts.Observability;

public interface IApplicationInfo
{
    string ApplicationName { get; }

    string Version { get; }

    string? InformationalVersion { get; }

    string EnvironmentName { get; }
}

public interface IApplicationLifetimeInfo
{
    DateTime StartedAtUtc { get; }

    TimeSpan GetUptime();
}

public sealed record CachedHealthResult(
    string Status,
    DateTime ExpiresAtUtc,
    bool IsFailure);

public interface IHealthSnapshotCache
{
    bool TryGet(string checkName, out CachedHealthResult? result);

    void Set(string checkName, CachedHealthResult result);
}

public sealed record ComponentHealthResult(
    string Name,
    string Status,
    string? Code,
    string? Summary,
    long DurationMilliseconds,
    DateTime CheckedAtUtc,
    bool IsCritical,
    IReadOnlyDictionary<string, string>? SafeDetails);

public interface IOperationalSafeDetailsSanitizer
{
    IReadOnlyDictionary<string, string>? Sanitize(IReadOnlyDictionary<string, string>? details);
}

public static class HealthStatusAggregator
{
    public static string Aggregate(IReadOnlyList<ComponentHealthResult> components)
    {
        if (components.Count == 0)
        {
            return OperationalHealthStates.Healthy;
        }

        if (components.Any(c =>
                c.Status == OperationalHealthStates.Unhealthy && c.IsCritical))
        {
            return OperationalHealthStates.Unhealthy;
        }

        if (components.Any(c => c.Status == OperationalHealthStates.Unhealthy) ||
            components.Any(c => c.Status == OperationalHealthStates.Degraded))
        {
            return OperationalHealthStates.Degraded;
        }

        return OperationalHealthStates.Healthy;
    }

    public static IReadOnlyList<ComponentHealthResult> OrderComponents(
        IReadOnlyList<ComponentHealthResult> components) =>
        components
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();
}

public static class OperationalBucketFormatter
{
    public static string PendingBucket(long count) =>
        count switch
        {
            0 => "0",
            <= 100 => "1-100",
            <= 1000 => "101-1000",
            _ => "1001+",
        };

    public static string AgeBucket(TimeSpan? age) =>
        age switch
        {
            null => "unknown",
            { TotalMinutes: < 1 } => "under_1m",
            { TotalMinutes: < 5 } => "1-5m",
            { TotalMinutes: < 30 } => "5-30m",
            _ => "30m+",
        };

    public static string LatencyBucket(long milliseconds) =>
        milliseconds switch
        {
            < 50 => "under_50ms",
            < 500 => "under_500ms",
            < 2000 => "under_2s",
            _ => "over_2s",
        };
}
