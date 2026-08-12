using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Events;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Analytics.Tests;

public sealed class AnalyticsEventReceiptTests
{
    private static readonly DateTime Now = new(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateProcessed_returns_receipt_with_correct_values()
    {
        var eventId = Guid.NewGuid();
        var eventType = "identity.user_registered";
        var occurred = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        var metricDate = DateOnly.FromDateTime(occurred);

        var receipt = AnalyticsEventReceipt.CreateProcessed(
            eventId, eventType, occurred, Now, metricDate, schemaVersion: 1);

        Assert.Equal(eventId, receipt.EventId);
        Assert.Equal(eventType, receipt.EventType);
        Assert.Equal(occurred, receipt.OccurredAtUtc);
        Assert.Equal(Now, receipt.ProcessedAtUtc);
        Assert.Equal(metricDate, receipt.MetricDateUtc);
        Assert.Equal(1, receipt.SchemaVersion);
        Assert.Equal(EventProcessingStatus.Processed, receipt.ProcessingStatus);
        Assert.Null(receipt.ErrorCode);
    }

    [Fact]
    public void CreateProcessed_throws_when_event_id_is_empty()
    {
        var ex = Assert.Throws<DomainException>(() =>
            AnalyticsEventReceipt.CreateProcessed(
                Guid.Empty, "identity.user_registered", Now, Now,
                DateOnly.FromDateTime(Now), 1));

        Assert.Equal(AnalyticsErrorCodes.EventIdRequired, ex.Message);
    }

    [Fact]
    public void CreateProcessed_throws_when_event_type_is_empty()
    {
        var ex = Assert.Throws<DomainException>(() =>
            AnalyticsEventReceipt.CreateProcessed(
                Guid.NewGuid(), "  ", Now, Now,
                DateOnly.FromDateTime(Now), 1));

        Assert.Equal(AnalyticsErrorCodes.EventTypeRequired, ex.Message);
    }

    [Fact]
    public void EventType_is_trimmed()
    {
        var receipt = AnalyticsEventReceipt.CreateProcessed(
            Guid.NewGuid(), "  identity.user_registered  ", Now, Now,
            DateOnly.FromDateTime(Now), 1);

        Assert.Equal("identity.user_registered", receipt.EventType);
    }
}
