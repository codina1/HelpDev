using HelpDev.Modules.Analytics.Domain;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Analytics.Domain.Events;

public sealed class AnalyticsEventReceipt
{
    private AnalyticsEventReceipt()
    {
    }

    public Guid EventId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime ProcessedAtUtc { get; private set; }

    public string ProcessingStatus { get; private set; } = string.Empty;

    public string? ErrorCode { get; private set; }

    public DateOnly MetricDateUtc { get; private set; }

    public int SchemaVersion { get; private set; }

    public static AnalyticsEventReceipt CreateProcessed(
        Guid eventId,
        string eventType,
        DateTime occurredAtUtc,
        DateTime processedAtUtc,
        DateOnly metricDateUtc,
        int schemaVersion)
    {
        if (eventId == Guid.Empty)
        {
            throw new DomainException(AnalyticsErrorCodes.EventIdRequired, "Event id is required.");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new DomainException(AnalyticsErrorCodes.EventTypeRequired, "Event type is required.");
        }

        return new AnalyticsEventReceipt
        {
            EventId = eventId,
            EventType = eventType.Trim(),
            OccurredAtUtc = occurredAtUtc,
            ProcessedAtUtc = processedAtUtc,
            ProcessingStatus = EventProcessingStatus.Processed,
            ErrorCode = null,
            MetricDateUtc = metricDateUtc,
            SchemaVersion = schemaVersion,
        };
    }
}
