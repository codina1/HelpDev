namespace HelpDev.Infrastructure.Observability;

public sealed class OutboxProcessorHeartbeat
{
    private readonly object _lock = new();
    private DateTime? _lastCycleStartedAtUtc;
    private DateTime? _lastCycleCompletedAtUtc;
    private DateTime? _lastSuccessfulProcessingAtUtc;
    private DateTime? _lastFailureAtUtc;
    private string? _lastFailureCode;
    private bool _isRunning;

    public void MarkCycleStarted(DateTime utcNow)
    {
        lock (_lock)
        {
            _lastCycleStartedAtUtc = utcNow;
            _isRunning = true;
        }
    }

    public void MarkCycleCompleted(DateTime utcNow, bool hadSuccessfulProcessing)
    {
        lock (_lock)
        {
            _lastCycleCompletedAtUtc = utcNow;
            _isRunning = false;
            if (hadSuccessfulProcessing)
            {
                _lastSuccessfulProcessingAtUtc = utcNow;
            }
        }
    }

    public void MarkCycleFailed(DateTime utcNow, string failureCode)
    {
        lock (_lock)
        {
            _lastFailureAtUtc = utcNow;
            _lastFailureCode = failureCode;
            _isRunning = false;
        }
    }

    public OutboxProcessorStateSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return new OutboxProcessorStateSnapshot(
                _lastCycleStartedAtUtc,
                _lastCycleCompletedAtUtc,
                _lastSuccessfulProcessingAtUtc,
                _lastFailureAtUtc,
                _lastFailureCode,
                _isRunning);
        }
    }
}

public sealed record OutboxProcessorStateSnapshot(
    DateTime? LastCycleStartedAtUtc,
    DateTime? LastCycleCompletedAtUtc,
    DateTime? LastSuccessfulProcessingAtUtc,
    DateTime? LastFailureAtUtc,
    string? LastFailureCode,
    bool IsRunning);
