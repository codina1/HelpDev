namespace HelpDev.SharedContracts.Observability;

public enum ApplicationReadinessStatus
{
    Starting = 0,
    Ready = 1,
    Stopping = 2,
    Failed = 3,
}

/// <summary>
/// Tracks the application lifecycle readiness state so the readiness probe does not report ready
/// before startup completes or during shutdown.
/// </summary>
public interface IApplicationReadinessState
{
    ApplicationReadinessStatus Status { get; }

    void MarkReady();

    void MarkStopping();

    void MarkFailed();

    bool IsAcceptingTraffic { get; }
}
