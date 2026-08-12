using HelpDev.Modules.Analytics.Domain;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Analytics.Domain.Metrics;

public sealed class DailyMetric
{
    public static readonly Guid GlobalSubjectSentinel = Guid.Empty;

    private DailyMetric()
    {
    }

    public Guid Id { get; private set; }

    public DateOnly DateUtc { get; private set; }

    public string MetricKey { get; private set; } = string.Empty;

    public Guid? SubjectId { get; private set; }

    public string? SubjectType { get; private set; }

    public string Dimension1Key { get; private set; } = string.Empty;

    public string Dimension1Value { get; private set; } = string.Empty;

    public string Dimension2Key { get; private set; } = string.Empty;

    public string Dimension2Value { get; private set; } = string.Empty;

    public long Count { get; private set; }

    public long SuccessCount { get; private set; }

    public long FailureCount { get; private set; }

    public long TotalDurationMilliseconds { get; private set; }

    public long MinDurationMilliseconds { get; private set; }

    public long MaxDurationMilliseconds { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static DailyMetric Create(
        Guid id,
        DateOnly dateUtc,
        string metricKey,
        Guid? subjectId,
        string? subjectType,
        string dimension1Key,
        string dimension1Value,
        string dimension2Key,
        string dimension2Value,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Metric id is required.");
        }

        if (!AnalyticsMetricKeys.IsSupported(metricKey))
        {
            throw new DomainException(AnalyticsErrorCodes.MetricKeyInvalid, "Metric key is invalid.");
        }

        return new DailyMetric
        {
            Id = id,
            DateUtc = dateUtc,
            MetricKey = metricKey,
            SubjectId = subjectId,
            SubjectType = subjectType,
            Dimension1Key = dimension1Key ?? string.Empty,
            Dimension1Value = dimension1Value ?? string.Empty,
            Dimension2Key = dimension2Key ?? string.Empty,
            Dimension2Value = dimension2Value ?? string.Empty,
            Count = 0,
            SuccessCount = 0,
            FailureCount = 0,
            TotalDurationMilliseconds = 0,
            MinDurationMilliseconds = 0,
            MaxDurationMilliseconds = 0,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
        };
    }

    public bool ApplyIncrement(
        long quantity,
        bool incrementSuccess,
        bool incrementFailure,
        long? durationMilliseconds,
        DateTime updatedAtUtc)
    {
        if (quantity <= 0)
        {
            throw new DomainException(AnalyticsErrorCodes.EventQuantityInvalid, "Quantity must be positive.");
        }

        var changed = false;

        if (Count == 0 && durationMilliseconds.HasValue)
        {
            MinDurationMilliseconds = durationMilliseconds.Value;
            MaxDurationMilliseconds = durationMilliseconds.Value;
            changed = true;
        }

        Count += quantity;
        changed = true;

        if (incrementSuccess)
        {
            SuccessCount += quantity;
        }

        if (incrementFailure)
        {
            FailureCount += quantity;
        }

        if (durationMilliseconds.HasValue)
        {
            var duration = durationMilliseconds.Value;
            if (duration < 0)
            {
                throw new DomainException(AnalyticsErrorCodes.EventProcessingFailed, "Duration cannot be negative.");
            }

            TotalDurationMilliseconds += duration * quantity;
            if (Count > quantity)
            {
                MinDurationMilliseconds = Math.Min(MinDurationMilliseconds, duration);
                MaxDurationMilliseconds = Math.Max(MaxDurationMilliseconds, duration);
            }
        }

        UpdatedAtUtc = updatedAtUtc;
        return changed;
    }
}
