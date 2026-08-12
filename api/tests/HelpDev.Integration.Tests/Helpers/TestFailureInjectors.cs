using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedContracts.Auditing;

namespace HelpDev.Integration.Tests.Helpers;

public sealed class TestAuditPersistenceFailureInjector : IAuditPersistenceFailureInjector
{
    private readonly object _gate = new();
    private bool _failNext;
    private string? _reason;

    public void FailNextWrite(string? reason = null)
    {
        lock (_gate)
        {
            _failNext = true;
            _reason = reason;
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _failNext = false;
            _reason = null;
        }
    }

    public void ThrowIfConfiguredToFail()
    {
        lock (_gate)
        {
            if (!_failNext)
            {
                return;
            }

            _failNext = false;
            var reason = _reason ?? "Injected audit persistence failure.";
            _reason = null;
            throw new InvalidOperationException(reason);
        }
    }
}

public sealed class TestAnalyticsFailureInjector : IAnalyticsFailureInjector
{
    private readonly object _gate = new();
    private bool _failNext;
    private string? _eventType;
    private string? _reason;

    public void FailNextIngestion(string? eventType = null, string? reason = null)
    {
        lock (_gate)
        {
            _failNext = true;
            _eventType = eventType;
            _reason = reason;
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _failNext = false;
            _eventType = null;
            _reason = null;
        }
    }

    public void ThrowIfConfiguredToFail(string eventType)
    {
        lock (_gate)
        {
            if (!_failNext)
            {
                return;
            }

            if (_eventType is not null
                && !string.Equals(_eventType, eventType, StringComparison.Ordinal))
            {
                return;
            }

            _failNext = false;
            var matchedType = _eventType;
            var reason = _reason ?? "Injected analytics ingestion failure.";
            _eventType = null;
            _reason = null;
            throw new InvalidOperationException(
                matchedType is null
                    ? reason
                    : $"{reason} (eventType={matchedType})");
        }
    }
}
