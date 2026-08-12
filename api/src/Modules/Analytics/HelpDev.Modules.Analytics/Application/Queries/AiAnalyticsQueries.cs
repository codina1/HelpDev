using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Application.Queries;

public interface IAiAnalyticsQueries
{
    Task<AiDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}

public sealed record AiDashboardDto(
    int RequestsToday,
    double SuccessRate,
    double AverageLatencyMs,
    AiProviderStatusDto Provider,
    IReadOnlyList<AiFailureBucketDto> Failures,
    IReadOnlyList<AiUsagePointDto> UsageByHour,
    IReadOnlyList<AiOperationCountDto> ByOperation,
    DateTime GeneratedAtUtc);

public sealed record AiProviderStatusDto(
    string Name,
    bool Configured,
    string HealthStatus,
    DateTime? LastSuccessfulCallAtUtc);

public sealed record AiFailureBucketDto(string ErrorCode, long Count);

public sealed record AiUsagePointDto(DateTime HourUtc, long Requests, long Successes, long Failures);

public sealed record AiOperationCountDto(string Operation, long Count, long Successes);

public sealed class AiAnalyticsQueries : IAiAnalyticsQueries
{
    private readonly IAnalyticsDbContext _db;
    private readonly IAiOperationMetrics _metrics;
    private readonly IAiHealthProbe _healthProbe;
    private readonly IDateTimeProvider _clock;

    public AiAnalyticsQueries(
        IAnalyticsDbContext db,
        IAiOperationMetrics metrics,
        IAiHealthProbe healthProbe,
        IDateTimeProvider clock)
    {
        _db = db;
        _metrics = metrics;
        _healthProbe = healthProbe;
        _clock = clock;
    }

    public async Task<AiDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = _clock.UtcNow;
        var dayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        var today = await _db.AiUsageRecords
            .AsNoTracking()
            .Where(r => r.CreatedAtUtc >= dayStart)
            .Select(r => new { r.Success, r.DurationMs, r.ErrorCode, r.TaskType, r.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var requestsToday = today.Count;
        var successCount = today.Count(r => r.Success);
        var successRate = requestsToday == 0 ? 0d : Math.Round((double)successCount / requestsToday, 4);
        var averageLatency = requestsToday == 0
            ? 0d
            : Math.Round(today.Average(r => (double)r.DurationMs), 2);

        var failures = today
            .Where(r => !r.Success)
            .GroupBy(r => string.IsNullOrWhiteSpace(r.ErrorCode) ? AiErrorCodes.GenerationFailed : r.ErrorCode!)
            .Select(g => new AiFailureBucketDto(g.Key, g.LongCount()))
            .OrderByDescending(f => f.Count)
            .ToList();

        var byOperation = today
            .GroupBy(r => r.TaskType)
            .Select(g => new AiOperationCountDto(g.Key, g.LongCount(), g.LongCount(x => x.Success)))
            .OrderByDescending(o => o.Count)
            .ToList();

        var usageByHour = today
            .GroupBy(r => new DateTime(r.CreatedAtUtc.Year, r.CreatedAtUtc.Month, r.CreatedAtUtc.Day, r.CreatedAtUtc.Hour, 0, 0, DateTimeKind.Utc))
            .OrderBy(g => g.Key)
            .Select(g => new AiUsagePointDto(
                g.Key,
                g.LongCount(),
                g.LongCount(x => x.Success),
                g.LongCount(x => !x.Success)))
            .ToList();

        var snapshot = _metrics.GetSnapshot();
        var health = await _healthProbe.CheckAsync(cancellationToken);

        return new AiDashboardDto(
            requestsToday,
            successRate,
            averageLatency,
            new AiProviderStatusDto(
                snapshot.ProviderName,
                snapshot.ProviderConfigured,
                health.Status,
                snapshot.LastSuccessfulCallAtUtc),
            failures,
            usageByHour,
            byOperation,
            now);
    }
}
