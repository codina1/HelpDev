namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// Test/dev failure injection for Fake provider. Never holds prompts or generated text.
/// </summary>
public sealed class FakeAiFailureInjector
{
    private int _remainingFailures;
    private string? _errorCode;

    public void Arm(string errorCode, int failureCount = 1)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException("Error code is required.", nameof(errorCode));
        }

        if (failureCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(failureCount));
        }

        _errorCode = errorCode.Trim();
        _remainingFailures = failureCount;
    }

    public void Clear()
    {
        _remainingFailures = 0;
        _errorCode = null;
    }

    public bool TryConsume(out string errorCode)
    {
        if (_remainingFailures <= 0 || string.IsNullOrWhiteSpace(_errorCode))
        {
            errorCode = string.Empty;
            return false;
        }

        _remainingFailures--;
        errorCode = _errorCode;
        return true;
    }
}
