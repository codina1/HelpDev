using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Analytics.Application.Processing;

public sealed class AnalyticsEventIngestor : IAnalyticsEventIngestor
{
    private readonly IAnalyticsEventProcessor _processor;
    private readonly IAnalyticsFailureInjector _failureInjector;
    private readonly ILogger<AnalyticsEventIngestor> _logger;

    public AnalyticsEventIngestor(
        IAnalyticsEventProcessor processor,
        IAnalyticsFailureInjector failureInjector,
        ILogger<AnalyticsEventIngestor> logger)
    {
        _processor = processor;
        _failureInjector = failureInjector;
        _logger = logger;
    }

    public async Task IngestAsync(
        AnalyticsEventEnvelope analyticsEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(analyticsEvent);

        try
        {
            _failureInjector.ThrowIfConfiguredToFail(analyticsEvent.EventType);
            await _processor.ProcessAsync(analyticsEvent, cancellationToken);
        }
        catch (AnalyticsException ex)
        {
            _logger.LogWarning(
                ex,
                "Analytics ingestion rejected. Operation={Operation} EventId={EventId} EventType={EventType} ErrorCode={ErrorCode}",
                "analytics_ingestion_rejected",
                analyticsEvent.EventId,
                analyticsEvent.EventType,
                ex.Code);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Analytics ingestion failed. Operation={Operation} EventId={EventId} EventType={EventType}",
                "analytics_ingestion_failed",
                analyticsEvent.EventId,
                analyticsEvent.EventType);
            throw;
        }
    }
}
