namespace HelpDev.SharedContracts.Analytics;

/// <summary>
/// Production-safe seam for Testing-only analytics ingestion failure injection.
/// Production registration is a no-op that never fails.
/// </summary>
public interface IAnalyticsFailureInjector
{
    void FailNextIngestion(string? eventType = null, string? reason = null);

    void Reset();

    void ThrowIfConfiguredToFail(string eventType);
}

public sealed class NoOpAnalyticsFailureInjector : IAnalyticsFailureInjector
{
    public void FailNextIngestion(string? eventType = null, string? reason = null)
    {
    }

    public void Reset()
    {
    }

    public void ThrowIfConfiguredToFail(string eventType)
    {
    }
}
