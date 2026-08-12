namespace HelpDev.SharedContracts.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool Enabled { get; set; } = true;

    public int PublicHealthCacheSeconds { get; set; } = 5;

    public int FailureCacheSeconds { get; set; } = 2;

    public int GlobalTimeoutSeconds { get; set; } = 5;

    public int MaximumConcurrentChecks { get; set; } = 4;

    public bool IncludeEnvironmentInAdminStatus { get; set; } = true;

    public bool IncludeVersionInAdminStatus { get; set; } = true;

    public PostgreSqlHealthOptions PostgreSql { get; set; } = new();

    public OutboxHealthOptions Outbox { get; set; } = new();

    public SearchHealthOptions Search { get; set; } = new();

    public AnalyticsHealthOptions Analytics { get; set; } = new();

    public AuditHealthOptions Audit { get; set; } = new();

    public AiHealthOptions Ai { get; set; } = new();

    public SlowRequestOptions SlowRequests { get; set; } = new();
}

public sealed class AiHealthOptions
{
    public int TimeoutSeconds { get; set; } = 3;

    public bool IsCritical { get; set; } = false;
}

public sealed class PostgreSqlHealthOptions
{
    public int TimeoutSeconds { get; set; } = 3;

    public int DegradedLatencyMilliseconds { get; set; } = 500;

    public int UnhealthyLatencyMilliseconds { get; set; } = 2000;

    public bool IsCritical { get; set; } = true;
}

public sealed class OutboxHealthOptions
{
    public int WarningPendingCount { get; set; } = 100;

    public int CriticalPendingCount { get; set; } = 1000;

    public int WarningOldestAgeMinutes { get; set; } = 5;

    public int CriticalOldestAgeMinutes { get; set; } = 30;

    public int WarningDeadLetterCount { get; set; } = 1;

    public int CriticalDeadLetterCount { get; set; } = 100;

    public int TimeoutSeconds { get; set; } = 3;

    public int ProcessorStaleMinutes { get; set; } = 10;

    public bool IsCritical { get; set; } = false;
}

public sealed class SearchHealthOptions
{
    public int WarningPendingCount { get; set; } = 100;

    public int CriticalPendingCount { get; set; } = 1000;

    public int WarningOldestAgeMinutes { get; set; } = 5;

    public int CriticalOldestAgeMinutes { get; set; } = 30;

    public int TimeoutSeconds { get; set; } = 3;

    public bool IsCritical { get; set; } = false;
}

public sealed class AnalyticsHealthOptions
{
    public int LookbackMinutes { get; set; } = 60;

    public int WarningFailureCount { get; set; } = 10;

    public int CriticalFailureCount { get; set; } = 100;

    public int TimeoutSeconds { get; set; } = 3;

    public bool IsCritical { get; set; } = false;
}

public sealed class AuditHealthOptions
{
    public int LookbackMinutes { get; set; } = 60;

    public int TimeoutSeconds { get; set; } = 3;

    public bool IsCritical { get; set; } = false;
}

public sealed class SlowRequestOptions
{
    public bool Enabled { get; set; } = true;

    public int WarningThresholdMilliseconds { get; set; } = 1000;

    public int ErrorThresholdMilliseconds { get; set; } = 5000;

    public string[] ExcludedRoutePrefixes { get; set; } = ["/health", "/swagger"];
}

public static class HealthCheckTags
{
    public const string Live = "live";
    public const string Ready = "ready";
    public const string Critical = "critical";
    public const string Dependency = "dependency";
    public const string Background = "background";
    public const string DegradedAllowed = "degraded-allowed";
}

public static class HealthCheckNames
{
    public const string Self = "self";
    public const string PostgreSql = "postgresql";
    public const string Outbox = "outbox";
    public const string Search = "search_projection";
    public const string Analytics = "analytics";
    public const string Audit = "audit";
    public const string Ai = "ai";
}
