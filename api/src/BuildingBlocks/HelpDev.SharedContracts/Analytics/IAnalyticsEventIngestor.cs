namespace HelpDev.SharedContracts.Analytics;

public interface IAnalyticsEventIngestor
{
    Task IngestAsync(
        AnalyticsEventEnvelope analyticsEvent,
        CancellationToken cancellationToken = default);
}
