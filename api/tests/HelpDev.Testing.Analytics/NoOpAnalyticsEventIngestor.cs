using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Testing.Analytics;

public sealed class NoOpAnalyticsEventIngestor : IAnalyticsEventIngestor
{
    public List<AnalyticsEventEnvelope> Events { get; } = [];

    public Task IngestAsync(AnalyticsEventEnvelope analyticsEvent, CancellationToken cancellationToken = default)
    {
        Events.Add(analyticsEvent);
        return Task.CompletedTask;
    }
}
