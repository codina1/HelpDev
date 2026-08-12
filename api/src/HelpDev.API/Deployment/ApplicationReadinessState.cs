using HelpDev.SharedContracts.Observability;

namespace HelpDev.API.Deployment;

public sealed class ApplicationReadinessState : IApplicationReadinessState
{
    private int _status = (int)ApplicationReadinessStatus.Starting;

    public ApplicationReadinessStatus Status => (ApplicationReadinessStatus)Volatile.Read(ref _status);

    public bool IsAcceptingTraffic => Status == ApplicationReadinessStatus.Ready;

    public void MarkReady() => Volatile.Write(ref _status, (int)ApplicationReadinessStatus.Ready);

    public void MarkStopping() => Volatile.Write(ref _status, (int)ApplicationReadinessStatus.Stopping);

    public void MarkFailed() => Volatile.Write(ref _status, (int)ApplicationReadinessStatus.Failed);
}

public static class DeploymentLogEvents
{
    public const string ApplicationStarting = "ApplicationStarting";
    public const string ProductionSafetyValidationStarted = "ProductionSafetyValidationStarted";
    public const string ProductionSafetyValidationFailed = "ProductionSafetyValidationFailed";
    public const string ProductionSafetyWarning = "ProductionSafetyWarning";
    public const string ApplicationStarted = "ApplicationStarted";
    public const string ApplicationStopping = "ApplicationStopping";
    public const string ApplicationStopped = "ApplicationStopped";
    public const string HostedServiceShutdownTimedOut = "HostedServiceShutdownTimedOut";
}
