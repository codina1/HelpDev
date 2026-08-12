using HelpDev.Infrastructure.Outbox.Operations;
using HelpDev.Modules.Administration.Application.Dashboard;

namespace HelpDev.Infrastructure.Administration;

public sealed class OutboxAdministrationStatisticsSource : IOutboxAdministrationStatisticsSource
{
    private readonly IOutboxOperationsQueries _queries;

    public OutboxAdministrationStatisticsSource(IOutboxOperationsQueries queries)
    {
        _queries = queries;
    }

    public async Task<OutboxAdministrationStatistics> GetAsync(CancellationToken cancellationToken = default)
    {
        var status = await _queries.GetStatusAsync(cancellationToken);
        return new OutboxAdministrationStatistics(
            status.Pending,
            status.Processing,
            status.Failed,
            status.Processed,
            status.OldestPendingAtUtc,
            status.LastProcessedAtUtc);
    }
}
