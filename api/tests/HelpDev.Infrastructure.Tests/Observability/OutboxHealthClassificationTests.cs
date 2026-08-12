using HelpDev.Infrastructure.Observability;

using HelpDev.SharedContracts.Observability;

using Microsoft.Extensions.Logging.Abstractions;

using Microsoft.Extensions.Options;



namespace HelpDev.Infrastructure.Tests.Observability;



public sealed class OutboxHealthClassificationTests

{

    private static readonly DateTime Now = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);



    [Fact]

    public async Task Healthy_backlog_and_fresh_heartbeat_returns_healthy_outbox()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now, hadSuccessfulProcessing: true);

        var service = CreateService(

            outboxSnapshot: FakeOutboxOperationalQueries.HealthySnapshot(Now),

            heartbeat: heartbeat);



        var status = await service.GetDetailedStatusAsync(CancellationToken.None);

        var outbox = status.Components.Single(c => c.Name == HealthCheckNames.Outbox);



        Assert.Equal(OperationalHealthStates.Healthy, outbox.Status);

        Assert.Null(outbox.Code);

    }



    [Fact]

    public async Task Warning_pending_count_classifies_outbox_as_degraded()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now, hadSuccessfulProcessing: true);

        var service = CreateService(

            outboxSnapshot: new OutboxOperationalSnapshot(

                PendingCount: 100,

                ProcessingCount: 0,

                FailedCount: 0,

                DeadLetterCount: 0,

                OldestPendingAtUtc: null,

                LastProcessedAtUtc: Now,

                LastFailureAtUtc: null,

                ProcessorEnabled: true,

                CheckedAtUtc: Now),

            heartbeat: heartbeat);



        var status = await service.GetDetailedStatusAsync(CancellationToken.None);

        var outbox = status.Components.Single(c => c.Name == HealthCheckNames.Outbox);



        Assert.Equal(OperationalHealthStates.Degraded, outbox.Status);

        Assert.Equal(HealthCheckCodes.OutboxBacklogWarning, outbox.Code);

    }



    [Fact]

    public async Task Critical_backlog_classifies_outbox_as_degraded_when_not_critical()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now, hadSuccessfulProcessing: true);

        var options = DefaultOptions();

        options.Outbox.IsCritical = false;

        var service = CreateService(

            outboxSnapshot: new OutboxOperationalSnapshot(

                PendingCount: 1000,

                ProcessingCount: 0,

                FailedCount: 0,

                DeadLetterCount: 0,

                OldestPendingAtUtc: null,

                LastProcessedAtUtc: Now,

                LastFailureAtUtc: null,

                ProcessorEnabled: true,

                CheckedAtUtc: Now),

            heartbeat: heartbeat,

            options: options);



        var status = await service.GetDetailedStatusAsync(CancellationToken.None);

        var outbox = status.Components.Single(c => c.Name == HealthCheckNames.Outbox);



        Assert.Equal(OperationalHealthStates.Degraded, outbox.Status);

        Assert.Equal(HealthCheckCodes.OutboxBacklogCritical, outbox.Code);

    }



    [Fact]

    public async Task Critical_backlog_marks_outbox_unhealthy_when_configured_critical()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now, hadSuccessfulProcessing: true);

        var options = DefaultOptions();

        options.Outbox.IsCritical = true;

        var service = CreateService(

            outboxSnapshot: new OutboxOperationalSnapshot(

                PendingCount: 1000,

                ProcessingCount: 0,

                FailedCount: 0,

                DeadLetterCount: 0,

                OldestPendingAtUtc: null,

                LastProcessedAtUtc: Now,

                LastFailureAtUtc: null,

                ProcessorEnabled: true,

                CheckedAtUtc: Now),

            heartbeat: heartbeat,

            options: options);



        var overall = await service.GetReadinessStatusAsync(CancellationToken.None);

        var status = await service.GetDetailedStatusAsync(CancellationToken.None);

        var outbox = status.Components.Single(c => c.Name == HealthCheckNames.Outbox);



        Assert.Equal(OperationalHealthStates.Unhealthy, outbox.Status);

        Assert.Equal(OperationalHealthStates.Unhealthy, overall);

    }



    [Fact]

    public async Task Stale_processor_heartbeat_classifies_outbox_as_degraded()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now.AddMinutes(-15), hadSuccessfulProcessing: true);

        var service = CreateService(

            outboxSnapshot: FakeOutboxOperationalQueries.HealthySnapshot(Now),

            heartbeat: heartbeat);



        var status = await service.GetDetailedStatusAsync(CancellationToken.None);

        var outbox = status.Components.Single(c => c.Name == HealthCheckNames.Outbox);



        Assert.Equal(OperationalHealthStates.Degraded, outbox.Status);

        Assert.Equal(HealthCheckCodes.OutboxProcessorStale, outbox.Code);

    }



    private static OperationalStatusService CreateService(

        OutboxOperationalSnapshot outboxSnapshot,

        OutboxProcessorHeartbeat heartbeat,

        ObservabilityOptions? options = null)

    {

        var clock = new FakeDateTimeProvider(Now);

        var outboxQueries = new FakeOutboxOperationalQueries { Snapshot = outboxSnapshot };



        return new OperationalStatusService(
            new FakePostgreSqlHealthProbe(),
            outboxQueries,
            new FakeSearchOperationalQueries(),
            new FakeAnalyticsOperationalQueries(),
            new FakeAuditOperationalQueries(),
            new FakeAiHealthProbe(),
            heartbeat,
            new FakeApplicationInfo(),
            new FakeApplicationLifetimeInfo(Now.AddMinutes(-5)),
            new OperationalSafeDetailsSanitizer(),
            new HealthSnapshotCache(),
            Options.Create(options ?? DefaultOptions()),
            clock,
            NullLogger<OperationalStatusService>.Instance);
    }

    private sealed class FakeAiHealthProbe : HelpDev.SharedContracts.Ai.IAiHealthProbe
    {
        public Task<HelpDev.SharedContracts.Ai.AiHealthProbeResult> CheckAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HelpDev.SharedContracts.Ai.AiHealthProbeResult(
                OperationalHealthStates.Healthy,
                null,
                "ok",
                1,
                Now,
                null));
    }

    private static ObservabilityOptions DefaultOptions() => new();
}


