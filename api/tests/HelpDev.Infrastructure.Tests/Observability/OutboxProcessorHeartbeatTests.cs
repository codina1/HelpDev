using HelpDev.Infrastructure.Observability;

namespace HelpDev.Infrastructure.Tests.Observability;



public sealed class OutboxProcessorHeartbeatTests

{

    private static readonly DateTime Now = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);



    [Fact]

    public void MarkCycleStarted_marks_processor_running()

    {

        var heartbeat = new OutboxProcessorHeartbeat();



        heartbeat.MarkCycleStarted(Now);



        var snapshot = heartbeat.GetSnapshot();

        Assert.True(snapshot.IsRunning);

        Assert.Equal(Now, snapshot.LastCycleStartedAtUtc);

    }



    [Fact]

    public void MarkCycleCompleted_clears_running_and_records_completion()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleStarted(Now);



        heartbeat.MarkCycleCompleted(Now.AddSeconds(5), hadSuccessfulProcessing: true);



        var snapshot = heartbeat.GetSnapshot();

        Assert.False(snapshot.IsRunning);

        Assert.Equal(Now.AddSeconds(5), snapshot.LastCycleCompletedAtUtc);

        Assert.Equal(Now.AddSeconds(5), snapshot.LastSuccessfulProcessingAtUtc);

    }



    [Fact]

    public void MarkCycleCompleted_without_success_does_not_update_last_success()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleCompleted(Now, hadSuccessfulProcessing: true);

        var previousSuccess = heartbeat.GetSnapshot().LastSuccessfulProcessingAtUtc;



        heartbeat.MarkCycleCompleted(Now.AddMinutes(1), hadSuccessfulProcessing: false);



        var snapshot = heartbeat.GetSnapshot();

        Assert.Equal(Now.AddMinutes(1), snapshot.LastCycleCompletedAtUtc);

        Assert.Equal(previousSuccess, snapshot.LastSuccessfulProcessingAtUtc);

    }



    [Fact]

    public void MarkCycleFailed_records_failure_and_stops_running()

    {

        var heartbeat = new OutboxProcessorHeartbeat();

        heartbeat.MarkCycleStarted(Now);



        heartbeat.MarkCycleFailed(Now.AddSeconds(2), "processor_error");



        var snapshot = heartbeat.GetSnapshot();

        Assert.False(snapshot.IsRunning);

        Assert.Equal(Now.AddSeconds(2), snapshot.LastFailureAtUtc);

        Assert.Equal("processor_error", snapshot.LastFailureCode);

    }

}


