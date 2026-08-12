using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Modules.Analytics.Application.Processing;

public sealed record AnalyticsEventProcessResult(
    bool WasDuplicate,
    bool Committed);

public interface IAnalyticsEventProcessor
{
    Task<AnalyticsEventProcessResult> ProcessAsync(
        AnalyticsEventEnvelope analyticsEvent,
        CancellationToken cancellationToken = default);
}
