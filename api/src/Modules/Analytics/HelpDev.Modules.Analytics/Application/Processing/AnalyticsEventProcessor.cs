using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Analytics.Application.Processing;

public sealed class AnalyticsEventProcessor : IAnalyticsEventProcessor
{
    private readonly IAnalyticsEventReceiptRepository _receiptRepository;
    private readonly IDailyMetricRepository _metricRepository;
    private readonly IDailyActiveUserRepository _activeUserRepository;
    private readonly IAnalyticsSubjectSnapshotRepository _snapshotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<AnalyticsEventProcessor> _logger;

    public AnalyticsEventProcessor(
        IAnalyticsEventReceiptRepository receiptRepository,
        IDailyMetricRepository metricRepository,
        IDailyActiveUserRepository activeUserRepository,
        IAnalyticsSubjectSnapshotRepository snapshotRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<AnalyticsEventProcessor> logger)
    {
        _receiptRepository = receiptRepository;
        _metricRepository = metricRepository;
        _activeUserRepository = activeUserRepository;
        _snapshotRepository = snapshotRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AnalyticsEventProcessResult> ProcessAsync(
        AnalyticsEventEnvelope analyticsEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analyticsEvent);

        AnalyticsEventValidator.Validate(analyticsEvent);

        if (await _receiptRepository.ExistsAsync(analyticsEvent.EventId, cancellationToken))
        {
            _logger.LogInformation(
                "Analytics event already processed. Operation={Operation} EventId={EventId} EventType={EventType} Duplicate={Duplicate}",
                "analytics_event_duplicate",
                analyticsEvent.EventId,
                analyticsEvent.EventType,
                true);
            return new AnalyticsEventProcessResult(WasDuplicate: true, Committed: false);
        }

        var mapping = AnalyticsMetricMapper.Map(analyticsEvent);
        var now = _clock.UtcNow;
        var metricDate = DateOnly.FromDateTime(analyticsEvent.OccurredAtUtc);

        try
        {
            foreach (var plan in mapping.Metrics)
            {
                await ApplyMetricIncrementAsync(plan, analyticsEvent.Quantity, now, cancellationToken);
            }

            if (mapping.MarkActiveUser && analyticsEvent.ActorUserId.HasValue)
            {
                await EnsureActiveUserMarkerAsync(
                    metricDate,
                    analyticsEvent.ActorUserId.Value,
                    analyticsEvent.OccurredAtUtc,
                    cancellationToken);
            }

            if (mapping.Snapshot is not null)
            {
                await UpsertSnapshotAsync(mapping.Snapshot, now, cancellationToken);
            }

            var receipt = AnalyticsEventReceipt.CreateProcessed(
                analyticsEvent.EventId,
                analyticsEvent.EventType,
                analyticsEvent.OccurredAtUtc,
                now,
                metricDate,
                analyticsEvent.SchemaVersion);

            await _receiptRepository.AddAsync(receipt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Analytics event processed. Operation={Operation} EventId={EventId} EventType={EventType} MetricDateUtc={MetricDateUtc}",
                "analytics_event_processed",
                analyticsEvent.EventId,
                analyticsEvent.EventType,
                metricDate);

            return new AnalyticsEventProcessResult(WasDuplicate: false, Committed: true);
        }
        catch (Exception ex) when (IsUniqueViolation(ex))
        {
            if (await _receiptRepository.ExistsAsync(analyticsEvent.EventId, cancellationToken))
            {
                return new AnalyticsEventProcessResult(WasDuplicate: true, Committed: false);
            }

            throw new AnalyticsException(
                "Analytics aggregation concurrency conflict.",
                AnalyticsApplicationErrorCodes.ConcurrencyConflict);
        }
    }

    private async Task ApplyMetricIncrementAsync(
        MetricIncrementPlan plan,
        long quantity,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        await _metricRepository.UpsertIncrementAsync(
            Guid.NewGuid(),
            plan.Identity,
            quantity,
            plan.IncrementSuccess,
            plan.IncrementFailure,
            plan.DurationMilliseconds,
            nowUtc,
            cancellationToken);
    }

    private async Task EnsureActiveUserMarkerAsync(
        DateOnly dateUtc,
        Guid userId,
        DateTime firstSeenAtUtc,
        CancellationToken cancellationToken)
    {
        if (await _activeUserRepository.ExistsAsync(dateUtc, userId, cancellationToken))
        {
            return;
        }

        await _activeUserRepository.AddAsync(
            DailyActiveUser.Create(dateUtc, userId, firstSeenAtUtc),
            cancellationToken);
    }

    private async Task UpsertSnapshotAsync(
        SubjectSnapshotPlan snapshotPlan,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var existing = await _snapshotRepository.GetAsync(snapshotPlan.SubjectType, snapshotPlan.SubjectId, cancellationToken);
        if (existing is null)
        {
            await _snapshotRepository.AddAsync(
                AnalyticsSubjectSnapshot.Create(
                    Guid.NewGuid(),
                    snapshotPlan.SubjectType,
                    snapshotPlan.SubjectId,
                    snapshotPlan.DisplayName,
                    snapshotPlan.Slug,
                    nowUtc),
                cancellationToken);
            return;
        }

        existing.Update(snapshotPlan.DisplayName, snapshotPlan.Slug, nowUtc);
    }

    private static bool IsUniqueViolation(Exception exception) =>
        ContainsUniqueViolation(exception.Message)
        || (exception.InnerException is not null && ContainsUniqueViolation(exception.InnerException.Message));

    private static bool ContainsUniqueViolation(string? message) =>
        message?.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
        || message?.Contains("unique", StringComparison.OrdinalIgnoreCase) == true;
}
