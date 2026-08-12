namespace HelpDev.SharedContracts.Observability;

public sealed record PostgreSqlHealthProbeResult(
    bool IsAvailable,
    long LatencyMilliseconds,
    string Status,
    string? Code);

public interface IPostgreSqlHealthProbe
{
    Task<PostgreSqlHealthProbeResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record OutboxOperationalSnapshot(
    long PendingCount,
    long ProcessingCount,
    long FailedCount,
    long DeadLetterCount,
    DateTime? OldestPendingAtUtc,
    DateTime? LastProcessedAtUtc,
    DateTime? LastFailureAtUtc,
    bool ProcessorEnabled,
    DateTime CheckedAtUtc);

public interface IOutboxOperationalQueries
{
    Task<OutboxOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record SearchOperationalSnapshot(
    long PendingProjectionCount,
    long FailedProjectionCount,
    DateTime? OldestPendingAtUtc,
    DateTime? LastSuccessfulProjectionAtUtc,
    DateTime? LastReindexCompletedAtUtc,
    bool ProjectionProcessorEnabled,
    DateTime CheckedAtUtc);

public interface ISearchOperationalQueries
{
    Task<SearchOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record AnalyticsOperationalSnapshot(
    long RecentProcessedCount,
    long RecentFailedCount,
    DateTime? LatestProcessedAtUtc,
    DateTime? LatestFailureAtUtc,
    bool PersistenceAvailable,
    DateTime CheckedAtUtc);

public interface IAnalyticsOperationalQueries
{
    Task<AnalyticsOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record AuditOperationalSnapshot(
    bool PersistenceAvailable,
    DateTime? LatestRecordAtUtc,
    long RecentRecordCount,
    DateTime CheckedAtUtc);

public interface IAuditOperationalQueries
{
    Task<AuditOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

public sealed record OperationalComponentDto(
    string Name,
    string Status,
    string? Code,
    string? Summary,
    long DurationMilliseconds,
    DateTime CheckedAtUtc,
    IReadOnlyDictionary<string, string>? SafeDetails);

public sealed record OperationalStatusDto(
    DateTime CheckedAtUtc,
    string OverallStatus,
    string ApplicationVersion,
    string EnvironmentName,
    long UptimeSeconds,
    string Scope,
    IReadOnlyList<OperationalComponentDto> Components);

public sealed record OperationsSummaryDto(
    string OverallStatus,
    DateTime CheckedAtUtc,
    string Scope,
    OperationsApplicationSummaryDto Application,
    OperationsDatabaseSummaryDto Database,
    OperationsOutboxSummaryDto Outbox,
    OperationsSearchSummaryDto Search,
    OperationsAnalyticsSummaryDto Analytics,
    OperationsAuditSummaryDto Audit);

public sealed record OperationsApplicationSummaryDto(
    string Version,
    string Environment,
    long UptimeSeconds);

public sealed record OperationsDatabaseSummaryDto(string Status, long LatencyMilliseconds);

public sealed record OperationsOutboxSummaryDto(
    string Status,
    long PendingCount,
    long FailedCount,
    long DeadLetterCount,
    long? OldestPendingAgeSeconds);

public sealed record OperationsSearchSummaryDto(
    string Status,
    long PendingCount,
    long FailedCount,
    long? OldestPendingAgeSeconds);

public sealed record OperationsAnalyticsSummaryDto(
    string Status,
    long RecentProcessedCount,
    long RecentFailedCount);

public sealed record OperationsAuditSummaryDto(
    string Status,
    bool PersistenceAvailable);

public interface IOperationalStatusService
{
    Task<OperationalStatusDto> GetDetailedStatusAsync(CancellationToken cancellationToken = default);

    Task<OperationsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<string> GetReadinessStatusAsync(CancellationToken cancellationToken = default);
}

public static class HealthCheckCodes
{
    public const string Timeout = "health_check_timeout";
    public const string PostgreSqlUnavailable = "health_postgresql_unavailable";
    public const string PostgreSqlSlow = "health_postgresql_slow";
    public const string OutboxBacklogWarning = "health_outbox_backlog_warning";
    public const string OutboxBacklogCritical = "health_outbox_backlog_critical";
    public const string OutboxProcessorStale = "health_outbox_processor_stale";
    public const string SearchProjectionDelayed = "health_search_projection_delayed";
    public const string SearchProjectionFailed = "health_search_projection_failed";
    public const string AnalyticsDegraded = "health_analytics_degraded";
    public const string AuditUnavailable = "health_audit_unavailable";
    public const string AiUnavailable = "health_ai_unavailable";
    public const string AiDisabled = "health_ai_disabled";
    public const string ComponentUnavailable = "health_component_unavailable";
    public const string ConfigurationInvalid = "health_configuration_invalid";
}

public static class OperationalHealthStates
{
    public const string Healthy = "Healthy";
    public const string Degraded = "Degraded";
    public const string Unhealthy = "Unhealthy";
}

public static class LoggingEventNames
{
    public const string RequestStarted = "RequestStarted";
    public const string RequestCompleted = "RequestCompleted";
    public const string RequestFailed = "RequestFailed";
    public const string SlowRequestDetected = "SlowRequestDetected";
    public const string DatabaseHealthCheckCompleted = "DatabaseHealthCheckCompleted";
    public const string OutboxHealthCheckCompleted = "OutboxHealthCheckCompleted";
    public const string SearchHealthCheckCompleted = "SearchHealthCheckCompleted";
    public const string AnalyticsHealthCheckCompleted = "AnalyticsHealthCheckCompleted";
    public const string AuditHealthCheckCompleted = "AuditHealthCheckCompleted";
    public const string AiHealthCheckCompleted = "AiHealthCheckCompleted";
    public const string OutboxProcessorCycleStarted = "OutboxProcessorCycleStarted";
    public const string OutboxProcessorCycleCompleted = "OutboxProcessorCycleCompleted";
    public const string OutboxProcessorCycleFailed = "OutboxProcessorCycleFailed";
}
