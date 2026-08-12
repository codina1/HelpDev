using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Logging;

namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// Decorates an inner provider with bounded retry, operation metrics, and safe results.
/// Never logs prompts or generated text.
/// </summary>
public sealed class ResilientAiTextGenerator : IAiTextGenerator
{
    private readonly IAiTextGenerator _inner;
    private readonly AiRetryPolicy _retry;
    private readonly IAiOperationMetrics _metrics;
    private readonly ILogger<ResilientAiTextGenerator> _logger;

    public ResilientAiTextGenerator(
        IAiTextGenerator inner,
        AiRetryPolicy retry,
        IAiOperationMetrics metrics,
        ILogger<ResilientAiTextGenerator> logger)
    {
        _inner = inner;
        _retry = retry;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<AiTextResponse> GenerateAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await GenerateSafeAsync(request, cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"AI generation failed ({result.ErrorCode ?? AiErrorCodes.GenerationFailed}).");
        }

        return new AiTextResponse(
            result.Content!,
            result.Model ?? "unknown",
            result.Provider ?? "unknown",
            result.Usage);
    }

    public async Task<AiGenerationResult> GenerateSafeAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operation = string.IsNullOrWhiteSpace(request.TaskType) ? "Unknown" : request.TaskType.Trim();
        var result = await _retry.ExecuteAsync(
            ct => _inner.GenerateSafeAsync(request, ct),
            cancellationToken);

        var provider = result.Provider ?? "unknown";
        if (result.Success)
        {
            _metrics.RecordSuccess(operation, provider, result.LatencyMs);
        }
        else
        {
            _metrics.RecordFailure(
                operation,
                provider,
                result.ErrorCode ?? AiErrorCodes.GenerationFailed,
                result.LatencyMs);
            _logger.LogWarning(
                "AI generation failed. Operation={Operation} ErrorCode={ErrorCode} LatencyMs={LatencyMs}",
                operation,
                result.ErrorCode,
                result.LatencyMs);
        }

        return result;
    }
}
