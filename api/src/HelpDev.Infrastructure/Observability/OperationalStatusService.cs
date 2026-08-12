using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Observability;

public sealed class OperationalStatusService : IOperationalStatusService
{
    private const string InstanceScope = "Instance";

    private readonly IPostgreSqlHealthProbe _postgreSqlHealthProbe;
    private readonly IOutboxOperationalQueries _outboxQueries;
    private readonly ISearchOperationalQueries _searchQueries;
    private readonly IAnalyticsOperationalQueries _analyticsQueries;
    private readonly IAuditOperationalQueries _auditQueries;
    private readonly IAiHealthProbe _aiHealthProbe;
    private readonly OutboxProcessorHeartbeat _outboxHeartbeat;
    private readonly IApplicationInfo _applicationInfo;
    private readonly IApplicationLifetimeInfo _lifetimeInfo;
    private readonly IOperationalSafeDetailsSanitizer _safeDetailsSanitizer;
    private readonly IHealthSnapshotCache _cache;
    private readonly ObservabilityOptions _options;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<OperationalStatusService> _logger;

    public OperationalStatusService(
        IPostgreSqlHealthProbe postgreSqlHealthProbe,
        IOutboxOperationalQueries outboxQueries,
        ISearchOperationalQueries searchQueries,
        IAnalyticsOperationalQueries analyticsQueries,
        IAuditOperationalQueries auditQueries,
        IAiHealthProbe aiHealthProbe,
        OutboxProcessorHeartbeat outboxHeartbeat,
        IApplicationInfo applicationInfo,
        IApplicationLifetimeInfo lifetimeInfo,
        IOperationalSafeDetailsSanitizer safeDetailsSanitizer,
        IHealthSnapshotCache cache,
        IOptions<ObservabilityOptions> options,
        IDateTimeProvider clock,
        ILogger<OperationalStatusService> logger)
    {
        _postgreSqlHealthProbe = postgreSqlHealthProbe;
        _outboxQueries = outboxQueries;
        _searchQueries = searchQueries;
        _analyticsQueries = analyticsQueries;
        _auditQueries = auditQueries;
        _aiHealthProbe = aiHealthProbe;
        _outboxHeartbeat = outboxHeartbeat;
        _applicationInfo = applicationInfo;
        _lifetimeInfo = lifetimeInfo;
        _safeDetailsSanitizer = safeDetailsSanitizer;
        _cache = cache;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    public async Task<string> GetReadinessStatusAsync(CancellationToken cancellationToken = default)
    {
        var components = await EvaluateAllComponentsAsync(cancellationToken);
        return HealthStatusAggregator.Aggregate(components);
    }

    public async Task<OperationalStatusDto> GetDetailedStatusAsync(CancellationToken cancellationToken = default)
    {
        var components = await EvaluateAllComponentsAsync(cancellationToken);
        var ordered = HealthStatusAggregator.OrderComponents(components);
        var overall = HealthStatusAggregator.Aggregate(ordered);

        return new OperationalStatusDto(
            _clock.UtcNow,
            overall,
            _applicationInfo.Version,
            _applicationInfo.EnvironmentName,
            (long)_lifetimeInfo.GetUptime().TotalSeconds,
            InstanceScope,
            ordered.Select(MapComponent).ToList());
    }

    public async Task<OperationsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var components = await EvaluateAllComponentsAsync(cancellationToken);
        var overall = HealthStatusAggregator.Aggregate(components);

        var postgres = components.First(c => c.Name == HealthCheckNames.PostgreSql);
        var outbox = components.First(c => c.Name == HealthCheckNames.Outbox);
        var search = components.First(c => c.Name == HealthCheckNames.Search);
        var analytics = components.First(c => c.Name == HealthCheckNames.Analytics);
        var audit = components.First(c => c.Name == HealthCheckNames.Audit);

        var outboxSnapshot = await _outboxQueries.GetSnapshotAsync(cancellationToken);
        var searchSnapshot = await _searchQueries.GetSnapshotAsync(cancellationToken);
        var analyticsSnapshot = await _analyticsQueries.GetSnapshotAsync(cancellationToken);
        var auditSnapshot = await _auditQueries.GetSnapshotAsync(cancellationToken);

        return new OperationsSummaryDto(
            overall,
            _clock.UtcNow,
            InstanceScope,
            new OperationsApplicationSummaryDto(
                _applicationInfo.Version,
                _applicationInfo.EnvironmentName,
                (long)_lifetimeInfo.GetUptime().TotalSeconds),
            new OperationsDatabaseSummaryDto(postgres.Status, postgres.DurationMilliseconds),
            new OperationsOutboxSummaryDto(
                outbox.Status,
                outboxSnapshot.PendingCount,
                outboxSnapshot.FailedCount,
                outboxSnapshot.DeadLetterCount,
                ToAgeSeconds(outboxSnapshot.OldestPendingAtUtc)),
            new OperationsSearchSummaryDto(
                search.Status,
                searchSnapshot.PendingProjectionCount,
                searchSnapshot.FailedProjectionCount,
                ToAgeSeconds(searchSnapshot.OldestPendingAtUtc)),
            new OperationsAnalyticsSummaryDto(
                analytics.Status,
                analyticsSnapshot.RecentProcessedCount,
                analyticsSnapshot.RecentFailedCount),
            new OperationsAuditSummaryDto(
                audit.Status,
                auditSnapshot.PersistenceAvailable));
    }

    private async Task<IReadOnlyList<ComponentHealthResult>> EvaluateAllComponentsAsync(
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(_options.MaximumConcurrentChecks);
        var checks = new (string Name, Func<CancellationToken, Task<ComponentHealthResult>> Evaluator, bool IsCritical)[]
        {
            (HealthCheckNames.Self, EvaluateSelfAsync, false),
            (HealthCheckNames.PostgreSql, EvaluatePostgreSqlAsync, _options.PostgreSql.IsCritical),
            (HealthCheckNames.Outbox, EvaluateOutboxAsync, _options.Outbox.IsCritical),
            (HealthCheckNames.Search, EvaluateSearchAsync, _options.Search.IsCritical),
            (HealthCheckNames.Analytics, EvaluateAnalyticsAsync, _options.Analytics.IsCritical),
            (HealthCheckNames.Audit, EvaluateAuditAsync, _options.Audit.IsCritical),
            (HealthCheckNames.Ai, EvaluateAiAsync, _options.Ai.IsCritical),
        };

        var results = new List<ComponentHealthResult>(checks.Length);
        foreach (var (name, evaluator, isCritical) in checks)
        {
            results.Add(await RunCheckAsync(name, evaluator, isCritical, semaphore, cancellationToken));
        }

        return results;
    }

    private async Task<ComponentHealthResult> RunCheckAsync(
        string name,
        Func<CancellationToken, Task<ComponentHealthResult>> evaluator,
        bool isCritical,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        if (_cache.TryGet(name, out var cached) && cached is not null)
        {
            return new ComponentHealthResult(
                name,
                cached.Status,
                null,
                null,
                0,
                _clock.UtcNow,
                isCritical,
                null);
        }

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.GlobalTimeoutSeconds));
            var result = await evaluator(timeoutCts.Token);
            var ttl = result.Status == OperationalHealthStates.Healthy
                ? _options.PublicHealthCacheSeconds
                : _options.FailureCacheSeconds;
            _cache.Set(name, new CachedHealthResult(result.Status, _clock.UtcNow.AddSeconds(ttl), result.Status != OperationalHealthStates.Healthy));
            return result with { IsCritical = isCritical };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ComponentHealthResult(
                name,
                OperationalHealthStates.Unhealthy,
                HealthCheckCodes.Timeout,
                "Health check timed out.",
                _options.GlobalTimeoutSeconds * 1000L,
                _clock.UtcNow,
                isCritical,
                null);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private Task<ComponentHealthResult> EvaluateSelfAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ComponentHealthResult(
            HealthCheckNames.Self,
            OperationalHealthStates.Healthy,
            null,
            "Process is running.",
            0,
            _clock.UtcNow,
            true,
            new Dictionary<string, string> { ["scope"] = InstanceScope }));
    }

    private async Task<ComponentHealthResult> EvaluatePostgreSqlAsync(CancellationToken cancellationToken)
    {
        var started = _clock.UtcNow;
        var result = await _postgreSqlHealthProbe.CheckAsync(cancellationToken);
        var duration = (long)(_clock.UtcNow - started).TotalMilliseconds;

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.DatabaseHealthCheckCompleted,
            result.Status,
            duration);

        return new ComponentHealthResult(
            HealthCheckNames.PostgreSql,
            result.Status,
            result.Code,
            result.IsAvailable ? "Database connectivity verified." : "Database connectivity failed.",
            result.LatencyMilliseconds,
            _clock.UtcNow,
            _options.PostgreSql.IsCritical,
            _safeDetailsSanitizer.Sanitize(new Dictionary<string, string>
            {
                ["connectivity"] = result.IsAvailable ? "available" : "unavailable",
                ["latencyBucket"] = OperationalBucketFormatter.LatencyBucket(result.LatencyMilliseconds),
            }));
    }

    private async Task<ComponentHealthResult> EvaluateOutboxAsync(CancellationToken cancellationToken)
    {
        var started = _clock.UtcNow;
        var snapshot = await _outboxQueries.GetSnapshotAsync(cancellationToken);
        var heartbeat = _outboxHeartbeat.GetSnapshot();
        var duration = (long)(_clock.UtcNow - started).TotalMilliseconds;

        var status = OperationalHealthStates.Healthy;
        string? code = null;
        var summary = "Outbox backlog is within thresholds.";

        if (snapshot.PendingCount >= _options.Outbox.CriticalPendingCount ||
            IsOlderThan(snapshot.OldestPendingAtUtc, _options.Outbox.CriticalOldestAgeMinutes) ||
            snapshot.DeadLetterCount >= _options.Outbox.CriticalDeadLetterCount)
        {
            status = _options.Outbox.IsCritical
                ? OperationalHealthStates.Unhealthy
                : OperationalHealthStates.Degraded;
            code = HealthCheckCodes.OutboxBacklogCritical;
            summary = "Outbox backlog exceeds critical threshold.";
        }
        else if (snapshot.PendingCount >= _options.Outbox.WarningPendingCount ||
                 IsOlderThan(snapshot.OldestPendingAtUtc, _options.Outbox.WarningOldestAgeMinutes) ||
                 snapshot.DeadLetterCount >= _options.Outbox.WarningDeadLetterCount)
        {
            status = OperationalHealthStates.Degraded;
            code = HealthCheckCodes.OutboxBacklogWarning;
            summary = "Outbox backlog exceeds warning threshold.";
        }

        if (heartbeat.LastCycleCompletedAtUtc is null ||
            (_clock.UtcNow - heartbeat.LastCycleCompletedAtUtc.Value).TotalMinutes > _options.Outbox.ProcessorStaleMinutes)
        {
            status = OperationalHealthStates.Degraded;
            code = HealthCheckCodes.OutboxProcessorStale;
            summary = "Outbox processor heartbeat is stale.";
        }

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.OutboxHealthCheckCompleted,
            status,
            duration);

        return new ComponentHealthResult(
            HealthCheckNames.Outbox,
            status,
            code,
            summary,
            duration,
            _clock.UtcNow,
            _options.Outbox.IsCritical,
            _safeDetailsSanitizer.Sanitize(new Dictionary<string, string>
            {
                ["pendingBucket"] = OperationalBucketFormatter.PendingBucket(snapshot.PendingCount),
                ["oldestPendingAgeBucket"] = OperationalBucketFormatter.AgeBucket(
                    snapshot.OldestPendingAtUtc.HasValue ? _clock.UtcNow - snapshot.OldestPendingAtUtc.Value : null),
                ["deadLetterBucket"] = OperationalBucketFormatter.PendingBucket(snapshot.DeadLetterCount),
                ["processorEnabled"] = snapshot.ProcessorEnabled.ToString(),
            }));
    }

    private async Task<ComponentHealthResult> EvaluateSearchAsync(CancellationToken cancellationToken)
    {
        var started = _clock.UtcNow;
        var snapshot = await _searchQueries.GetSnapshotAsync(cancellationToken);
        var duration = (long)(_clock.UtcNow - started).TotalMilliseconds;

        var status = OperationalHealthStates.Healthy;
        string? code = null;
        var summary = "Search projection is healthy.";

        if (snapshot.PendingProjectionCount >= _options.Search.CriticalPendingCount ||
            IsOlderThan(snapshot.OldestPendingAtUtc, _options.Search.CriticalOldestAgeMinutes))
        {
            status = _options.Search.IsCritical
                ? OperationalHealthStates.Unhealthy
                : OperationalHealthStates.Degraded;
            code = HealthCheckCodes.SearchProjectionDelayed;
            summary = "Search projection backlog is critical.";
        }
        else if (snapshot.PendingProjectionCount >= _options.Search.WarningPendingCount ||
                 snapshot.FailedProjectionCount > 0 ||
                 IsOlderThan(snapshot.OldestPendingAtUtc, _options.Search.WarningOldestAgeMinutes))
        {
            status = OperationalHealthStates.Degraded;
            code = snapshot.FailedProjectionCount > 0
                ? HealthCheckCodes.SearchProjectionFailed
                : HealthCheckCodes.SearchProjectionDelayed;
            summary = "Search projection is delayed.";
        }

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.SearchHealthCheckCompleted,
            status,
            duration);

        return new ComponentHealthResult(
            HealthCheckNames.Search,
            status,
            code,
            summary,
            duration,
            _clock.UtcNow,
            _options.Search.IsCritical,
            _safeDetailsSanitizer.Sanitize(new Dictionary<string, string>
            {
                ["pendingProjectionBucket"] = OperationalBucketFormatter.PendingBucket(snapshot.PendingProjectionCount),
                ["oldestProjectionAgeBucket"] = OperationalBucketFormatter.AgeBucket(
                    snapshot.OldestPendingAtUtc.HasValue ? _clock.UtcNow - snapshot.OldestPendingAtUtc.Value : null),
                ["lastSuccessfulIndexAgeBucket"] = OperationalBucketFormatter.AgeBucket(
                    snapshot.LastSuccessfulProjectionAtUtc.HasValue
                        ? _clock.UtcNow - snapshot.LastSuccessfulProjectionAtUtc.Value
                        : null),
            }));
    }

    private async Task<ComponentHealthResult> EvaluateAnalyticsAsync(CancellationToken cancellationToken)
    {
        var started = _clock.UtcNow;
        var snapshot = await _analyticsQueries.GetSnapshotAsync(cancellationToken);
        var duration = (long)(_clock.UtcNow - started).TotalMilliseconds;

        var status = OperationalHealthStates.Healthy;
        string? code = null;
        var summary = "Analytics ingestion is healthy.";

        if (!snapshot.PersistenceAvailable)
        {
            status = _options.Analytics.IsCritical
                ? OperationalHealthStates.Unhealthy
                : OperationalHealthStates.Degraded;
            code = HealthCheckCodes.AnalyticsDegraded;
            summary = "Analytics persistence is unavailable.";
        }
        else if (snapshot.RecentFailedCount >= _options.Analytics.CriticalFailureCount)
        {
            status = _options.Analytics.IsCritical
                ? OperationalHealthStates.Unhealthy
                : OperationalHealthStates.Degraded;
            code = HealthCheckCodes.AnalyticsDegraded;
            summary = "Analytics failure count is elevated.";
        }
        else if (snapshot.RecentFailedCount >= _options.Analytics.WarningFailureCount)
        {
            status = OperationalHealthStates.Degraded;
            code = HealthCheckCodes.AnalyticsDegraded;
            summary = "Analytics has recent failures.";
        }

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.AnalyticsHealthCheckCompleted,
            status,
            duration);

        return new ComponentHealthResult(
            HealthCheckNames.Analytics,
            status,
            code,
            summary,
            duration,
            _clock.UtcNow,
            _options.Analytics.IsCritical,
            _safeDetailsSanitizer.Sanitize(new Dictionary<string, string>
            {
                ["ingestionAvailable"] = snapshot.PersistenceAvailable.ToString(),
                ["recentProcessingFailureBucket"] = OperationalBucketFormatter.PendingBucket(snapshot.RecentFailedCount),
                ["latestReceiptAgeBucket"] = OperationalBucketFormatter.AgeBucket(
                    snapshot.LatestProcessedAtUtc.HasValue
                        ? _clock.UtcNow - snapshot.LatestProcessedAtUtc.Value
                        : null),
            }));
    }

    private async Task<ComponentHealthResult> EvaluateAuditAsync(CancellationToken cancellationToken)
    {
        var started = _clock.UtcNow;
        var snapshot = await _auditQueries.GetSnapshotAsync(cancellationToken);
        var duration = (long)(_clock.UtcNow - started).TotalMilliseconds;

        var status = OperationalHealthStates.Healthy;
        string? code = null;
        var summary = "Audit persistence is available.";

        if (!snapshot.PersistenceAvailable)
        {
            status = _options.Audit.IsCritical
                ? OperationalHealthStates.Unhealthy
                : OperationalHealthStates.Degraded;
            code = HealthCheckCodes.AuditUnavailable;
            summary = "Audit persistence is unavailable.";
        }

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.AuditHealthCheckCompleted,
            status,
            duration);

        return new ComponentHealthResult(
            HealthCheckNames.Audit,
            status,
            code,
            summary,
            duration,
            _clock.UtcNow,
            _options.Audit.IsCritical,
            _safeDetailsSanitizer.Sanitize(new Dictionary<string, string>
            {
                ["persistenceAvailable"] = snapshot.PersistenceAvailable.ToString(),
                ["latestWriteAgeBucket"] = OperationalBucketFormatter.AgeBucket(
                    snapshot.LatestRecordAtUtc.HasValue
                        ? _clock.UtcNow - snapshot.LatestRecordAtUtc.Value
                        : null),
            }));
    }

    private async Task<ComponentHealthResult> EvaluateAiAsync(CancellationToken cancellationToken)
    {
        var probe = await _aiHealthProbe.CheckAsync(cancellationToken);

        _logger.LogInformation(
            "Event={Event} Outcome={Outcome} DurationMilliseconds={DurationMilliseconds}",
            LoggingEventNames.AiHealthCheckCompleted,
            probe.Status,
            probe.LatencyMilliseconds);

        return new ComponentHealthResult(
            HealthCheckNames.Ai,
            probe.Status,
            probe.Code,
            probe.Summary,
            probe.LatencyMilliseconds,
            probe.CheckedAtUtc,
            _options.Ai.IsCritical,
            _safeDetailsSanitizer.Sanitize(
                probe.SafeDetails is null
                    ? null
                    : new Dictionary<string, string>(probe.SafeDetails)));
    }

    private static OperationalComponentDto MapComponent(ComponentHealthResult component) =>
        new(
            component.Name,
            component.Status,
            component.Code,
            component.Summary,
            component.DurationMilliseconds,
            component.CheckedAtUtc,
            component.SafeDetails);

    private static long? ToAgeSeconds(DateTime? timestampUtc) =>
        timestampUtc.HasValue
            ? (long)Math.Max(0, (DateTime.UtcNow - timestampUtc.Value).TotalSeconds)
            : null;

    private static bool IsOlderThan(DateTime? timestampUtc, int minutes) =>
        timestampUtc.HasValue &&
        (DateTime.UtcNow - timestampUtc.Value).TotalMinutes >= minutes;
}
