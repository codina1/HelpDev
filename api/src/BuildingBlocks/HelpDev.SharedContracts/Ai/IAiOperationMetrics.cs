namespace HelpDev.SharedContracts.Ai;

/// <summary>In-process AI operational counters (no prompts or generated text).</summary>
public interface IAiOperationMetrics
{
    void RecordSuccess(string operation, string provider, long latencyMs);

    void RecordFailure(string operation, string provider, string errorCode, long latencyMs);

    AiOperationMetricsSnapshot GetSnapshot();
}

public sealed record AiOperationMetricsSnapshot(
    long TotalRequests,
    long SuccessCount,
    long FailureCount,
    double SuccessRate,
    double AverageLatencyMs,
    IReadOnlyDictionary<string, long> LatencyBuckets,
    DateTime? LastSuccessfulCallAtUtc,
    bool ProviderConfigured,
    string ProviderName,
    IReadOnlyDictionary<string, long> FailuresByCode);

/// <summary>
/// AI health probe — configuration + connectivity only. Must NOT run generation.
/// </summary>
public interface IAiHealthProbe
{
    Task<AiHealthProbeResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record AiHealthProbeResult(
    string Status,
    string? Code,
    string? Summary,
    long LatencyMilliseconds,
    DateTime CheckedAtUtc,
    IReadOnlyDictionary<string, string>? SafeDetails);
