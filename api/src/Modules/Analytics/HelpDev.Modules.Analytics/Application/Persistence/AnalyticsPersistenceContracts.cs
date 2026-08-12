using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.Modules.Analytics.Application.Queries;
using HelpDev.Modules.Analytics.Domain.AiUsage;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.Modules.Analytics.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Application.Persistence;

public interface IAnalyticsDbContext
{
    DbSet<AnalyticsEventReceipt> AnalyticsEventReceipts { get; }

    DbSet<DailyMetric> DailyMetrics { get; }

    DbSet<DailyActiveUser> DailyActiveUsers { get; }

    DbSet<AnalyticsSubjectSnapshot> AnalyticsSubjectSnapshots { get; }

    DbSet<AiUsageRecord> AiUsageRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IAiUsageRecordRepository
{
    Task AddAsync(AiUsageRecord record, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IAnalyticsEventReceiptRepository
{
    Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<AnalyticsEventReceipt?> GetAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task AddAsync(AnalyticsEventReceipt receipt, CancellationToken cancellationToken = default);
}

public interface IDailyMetricRepository
{
    Task<DailyMetric?> GetForUpdateAsync(DailyMetricIdentity identity, CancellationToken cancellationToken = default);

    Task AddAsync(DailyMetric metric, CancellationToken cancellationToken = default);

    Task UpsertIncrementAsync(
        Guid newMetricId,
        DailyMetricIdentity identity,
        long quantity,
        bool incrementSuccess,
        bool incrementFailure,
        long? durationMilliseconds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}

public interface IDailyActiveUserRepository
{
    Task<bool> ExistsAsync(DateOnly dateUtc, Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(DailyActiveUser marker, CancellationToken cancellationToken = default);
}

public interface IAnalyticsSubjectSnapshotRepository
{
    Task<AnalyticsSubjectSnapshot?> GetAsync(string subjectType, Guid subjectId, CancellationToken cancellationToken = default);

    Task AddAsync(AnalyticsSubjectSnapshot snapshot, CancellationToken cancellationToken = default);
}
