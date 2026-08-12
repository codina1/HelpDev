namespace HelpDev.SharedContracts.Auditing;

/// <summary>
/// Production-safe seam for Testing-only audit persistence failure injection.
/// Production registration is a no-op that never fails.
/// </summary>
public interface IAuditPersistenceFailureInjector
{
    void FailNextWrite(string? reason = null);

    void Reset();

    void ThrowIfConfiguredToFail();
}

public sealed class NoOpAuditPersistenceFailureInjector : IAuditPersistenceFailureInjector
{
    public void FailNextWrite(string? reason = null)
    {
    }

    public void Reset()
    {
    }

    public void ThrowIfConfiguredToFail()
    {
    }
}
