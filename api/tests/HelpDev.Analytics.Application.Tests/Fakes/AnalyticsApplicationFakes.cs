using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Analytics.Application.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }

    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeAnalyticsEventReceiptRepository : IAnalyticsEventReceiptRepository
{
    private readonly HashSet<Guid> _existingIds = [];
    private readonly List<AnalyticsEventReceipt> _receipts = [];

    public IReadOnlyList<AnalyticsEventReceipt> Receipts => _receipts;

    public void SeedExisting(Guid eventId) => _existingIds.Add(eventId);

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_existingIds.Contains(eventId));

    public Task<AnalyticsEventReceipt?> GetAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_receipts.FirstOrDefault(r => r.EventId == eventId));

    public Task AddAsync(AnalyticsEventReceipt receipt, CancellationToken cancellationToken = default)
    {
        _existingIds.Add(receipt.EventId);
        _receipts.Add(receipt);
        return Task.CompletedTask;
    }
}

internal sealed class FakeDailyMetricRepository : IDailyMetricRepository
{
    private readonly List<DailyMetric> _metrics = [];

    public IReadOnlyList<DailyMetric> Metrics => _metrics;

    public int AddCallCount { get; private set; }

    public Task<DailyMetric?> GetForUpdateAsync(DailyMetricIdentity identity, CancellationToken cancellationToken = default)
    {
        var found = _metrics.FirstOrDefault(m =>
            m.DateUtc == identity.DateUtc &&
            m.MetricKey == identity.MetricKey &&
            m.SubjectId == identity.SubjectId &&
            m.SubjectType == identity.SubjectType &&
            m.Dimension1Key == identity.Dimension1Key &&
            m.Dimension1Value == identity.Dimension1Value);
        return Task.FromResult(found);
    }

    public Task AddAsync(DailyMetric metric, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _metrics.Add(metric);
        return Task.CompletedTask;
    }

    public Task UpsertIncrementAsync(
        Guid newMetricId,
        DailyMetricIdentity identity,
        long quantity,
        bool incrementSuccess,
        bool incrementFailure,
        long? durationMilliseconds,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = _metrics.FirstOrDefault(m =>
            m.DateUtc == identity.DateUtc &&
            m.MetricKey == identity.MetricKey &&
            m.SubjectId == identity.SubjectId &&
            m.SubjectType == identity.SubjectType &&
            m.Dimension1Key == identity.Dimension1Key &&
            m.Dimension1Value == identity.Dimension1Value &&
            m.Dimension2Key == identity.Dimension2Key &&
            m.Dimension2Value == identity.Dimension2Value);

        if (existing is null)
        {
            var metric = DailyMetric.Create(
                newMetricId,
                identity.DateUtc,
                identity.MetricKey,
                identity.SubjectId,
                identity.SubjectType,
                identity.Dimension1Key,
                identity.Dimension1Value,
                identity.Dimension2Key,
                identity.Dimension2Value,
                nowUtc);
            metric.ApplyIncrement(quantity, incrementSuccess, incrementFailure, durationMilliseconds, nowUtc);
            _metrics.Add(metric);
            AddCallCount++;
            return Task.CompletedTask;
        }

        existing.ApplyIncrement(quantity, incrementSuccess, incrementFailure, durationMilliseconds, nowUtc);
        return Task.CompletedTask;
    }
}

internal sealed class FakeDailyActiveUserRepository : IDailyActiveUserRepository
{
    private readonly HashSet<(DateOnly, Guid)> _existing = [];
    private readonly List<DailyActiveUser> _added = [];

    public IReadOnlyList<DailyActiveUser> Added => _added;

    public void SeedExisting(DateOnly date, Guid userId) => _existing.Add((date, userId));

    public Task<bool> ExistsAsync(DateOnly dateUtc, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_existing.Contains((dateUtc, userId)));

    public Task AddAsync(DailyActiveUser marker, CancellationToken cancellationToken = default)
    {
        _existing.Add((marker.DateUtc, marker.UserId));
        _added.Add(marker);
        return Task.CompletedTask;
    }
}

internal sealed class FakeAnalyticsSubjectSnapshotRepository : IAnalyticsSubjectSnapshotRepository
{
    private readonly List<AnalyticsSubjectSnapshot> _snapshots = [];

    public IReadOnlyList<AnalyticsSubjectSnapshot> Snapshots => _snapshots;

    public Task<AnalyticsSubjectSnapshot?> GetAsync(string subjectType, Guid subjectId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_snapshots.FirstOrDefault(s => s.SubjectType == subjectType && s.SubjectId == subjectId));

    public Task AddAsync(AnalyticsSubjectSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        _snapshots.Add(snapshot);
        return Task.CompletedTask;
    }
}

internal static class ProcessorFactory
{
    public static (
        AnalyticsEventProcessor Processor,
        FakeAnalyticsEventReceiptRepository ReceiptRepo,
        FakeDailyMetricRepository MetricRepo,
        FakeDailyActiveUserRepository ActiveUserRepo,
        FakeAnalyticsSubjectSnapshotRepository SnapshotRepo,
        FakeUnitOfWork UnitOfWork,
        FakeDateTimeProvider Clock) Create(DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider(utcNow ?? new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc));
        var receiptRepo = new FakeAnalyticsEventReceiptRepository();
        var metricRepo = new FakeDailyMetricRepository();
        var activeUserRepo = new FakeDailyActiveUserRepository();
        var snapshotRepo = new FakeAnalyticsSubjectSnapshotRepository();
        var unitOfWork = new FakeUnitOfWork();
        var processor = new AnalyticsEventProcessor(
            receiptRepo,
            metricRepo,
            activeUserRepo,
            snapshotRepo,
            unitOfWork,
            clock,
            NullLogger<AnalyticsEventProcessor>.Instance);

        return (processor, receiptRepo, metricRepo, activeUserRepo, snapshotRepo, unitOfWork, clock);
    }
}
