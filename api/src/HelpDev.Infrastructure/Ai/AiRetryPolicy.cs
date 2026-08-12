using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// Bounded exponential retry for transient AI provider failures only.
/// Does not retry validation/unauthorized/malformed errors.
/// </summary>
public sealed class AiRetryPolicy
{
    public const int DefaultMaxAttempts = 3;
    public const int DefaultBaseDelayMs = 100;
    public const int DefaultMaxDelayMs = 2000;

    private readonly int _maxAttempts;
    private readonly int _baseDelayMs;
    private readonly int _maxDelayMs;
    private readonly ILogger<AiRetryPolicy>? _logger;

    public AiRetryPolicy(
        int maxAttempts = DefaultMaxAttempts,
        int baseDelayMs = DefaultBaseDelayMs,
        int maxDelayMs = DefaultMaxDelayMs,
        ILogger<AiRetryPolicy>? logger = null)
    {
        if (maxAttempts < 1 || maxAttempts > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        }

        _maxAttempts = maxAttempts;
        _baseDelayMs = Math.Clamp(baseDelayMs, 10, 5000);
        _maxDelayMs = Math.Max(_baseDelayMs, maxDelayMs);
        _logger = logger;
    }

    public async Task<AiGenerationResult> ExecuteAsync(
        Func<CancellationToken, Task<AiGenerationResult>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        AiGenerationResult? last = null;
        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            last = await action(cancellationToken);
            if (last.Success)
            {
                return last;
            }

            if (!AiErrorCodes.IsTransient(last.ErrorCode) || attempt >= _maxAttempts)
            {
                return last;
            }

            var delay = Math.Min(_maxDelayMs, _baseDelayMs * (int)Math.Pow(2, attempt - 1));
            _logger?.LogWarning(
                "AI provider transient failure {ErrorCode}; retry {Attempt}/{Max} after {DelayMs}ms",
                last.ErrorCode,
                attempt,
                _maxAttempts,
                delay);
            await Task.Delay(delay, cancellationToken);
        }

        return last ?? AiGenerationResult.Fail(AiErrorCodes.GenerationFailed, 0);
    }
}
