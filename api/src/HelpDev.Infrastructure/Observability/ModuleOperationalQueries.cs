using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.SharedContracts.Observability;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Observability;

public sealed class AnalyticsOperationalQueries : IAnalyticsOperationalQueries
{
    private readonly IAnalyticsDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly int _lookbackMinutes;

    public AnalyticsOperationalQueries(
        IAnalyticsDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<ObservabilityOptions> options)
    {
        _dbContext = dbContext;
        _clock = clock;
        _lookbackMinutes = options.Value.Analytics.LookbackMinutes;
    }

    public async Task<AnalyticsOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var since = _clock.UtcNow.AddMinutes(-_lookbackMinutes);

        try
        {
            var recent = await _dbContext.AnalyticsEventReceipts.AsNoTracking()
                .Where(receipt => receipt.ProcessedAtUtc >= since)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    Processed = group.Count(receipt => receipt.ProcessingStatus == EventProcessingStatus.Processed),
                    Failed = group.Count(receipt => receipt.ProcessingStatus == EventProcessingStatus.Failed),
                    LatestProcessed = group.Max(receipt => (DateTime?)receipt.ProcessedAtUtc),
                    LatestFailure = group
                        .Where(receipt => receipt.ProcessingStatus == EventProcessingStatus.Failed)
                        .Max(receipt => (DateTime?)receipt.ProcessedAtUtc),
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new AnalyticsOperationalSnapshot(
                recent?.Processed ?? 0,
                recent?.Failed ?? 0,
                recent?.LatestProcessed,
                recent?.LatestFailure,
                PersistenceAvailable: true,
                _clock.UtcNow);
        }
        catch (Exception)
        {
            return new AnalyticsOperationalSnapshot(
                0,
                0,
                null,
                null,
                PersistenceAvailable: false,
                _clock.UtcNow);
        }
    }
}

public sealed class AuditOperationalQueries : IAuditOperationalQueries
{
    private readonly IAuditDbContext _dbContext;
    private readonly IDateTimeProvider _clock;
    private readonly int _lookbackMinutes;

    public AuditOperationalQueries(
        IAuditDbContext dbContext,
        IDateTimeProvider clock,
        IOptions<ObservabilityOptions> options)
    {
        _dbContext = dbContext;
        _clock = clock;
        _lookbackMinutes = options.Value.Audit.LookbackMinutes;
    }

    public async Task<AuditOperationalSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var since = _clock.UtcNow.AddMinutes(-_lookbackMinutes);

        try
        {
            var stats = await _dbContext.AuditRecords.AsNoTracking()
                .Where(record => record.OccurredAtUtc >= since)
                .GroupBy(_ => 1)
                .Select(group => new
                {
                    Count = group.Count(),
                    Latest = group.Max(record => (DateTime?)record.OccurredAtUtc),
                })
                .FirstOrDefaultAsync(cancellationToken);

            var latestOverall = await _dbContext.AuditRecords.AsNoTracking()
                .MaxAsync(record => (DateTime?)record.OccurredAtUtc, cancellationToken);

            return new AuditOperationalSnapshot(
                PersistenceAvailable: true,
                latestOverall,
                stats?.Count ?? 0,
                _clock.UtcNow);
        }
        catch (Exception)
        {
            return new AuditOperationalSnapshot(
                PersistenceAvailable: false,
                null,
                0,
                _clock.UtcNow);
        }
    }
}
