using HelpDev.Infrastructure.Observability;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HelpDev.Integration.Tests.HostedServices;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class HostedServiceLifecycleIntegrationTests : IntegrationTestClassBase
{
    public HostedServiceLifecycleIntegrationTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task ProcessBatchAsync_respects_cancellation_token()
    {
        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();

        // Empty outbox completes promptly when not cancelled.
        await processor.ProcessBatchAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => processor.ProcessBatchAsync(cts.Token));
    }

    [PostgreSqlFact]
    public void Heartbeat_MarkCycleCompleted_updates_snapshot()
    {
        var heartbeat = Factory.Services.GetRequiredService<OutboxProcessorHeartbeat>();
        var completedAt = DateTime.UtcNow;

        heartbeat.MarkCycleCompleted(completedAt, hadSuccessfulProcessing: true);
        var after = heartbeat.GetSnapshot();

        Assert.NotNull(after.LastCycleCompletedAtUtc);
        Assert.NotNull(after.LastSuccessfulProcessingAtUtc);
        Assert.False(after.IsRunning);
    }

    [PostgreSqlFact]
    public void OutboxProcessor_hosted_service_descriptor_removed_by_factory()
    {
        using var scope1 = Factory.Services.CreateScope();
        using var scope2 = Factory.Services.CreateScope();
        Assert.NotSame(scope1.ServiceProvider, scope2.ServiceProvider);

        var processor1 = Factory.Services.GetRequiredService<OutboxProcessor>();
        var processor2 = Factory.Services.GetRequiredService<OutboxProcessor>();
        Assert.Same(processor1, processor2);

        var hostedServices = Factory.Services.GetServices<IHostedService>().ToList();
        Assert.DoesNotContain(hostedServices, service => service.GetType() == typeof(OutboxProcessor));
    }
}
