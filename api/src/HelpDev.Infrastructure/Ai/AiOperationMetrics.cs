using System.Collections.Concurrent;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

public sealed class AiOperationMetrics : IAiOperationMetrics
{
    private static readonly long[] BucketEdgesMs = [50, 100, 250, 500, 1000, 2500, 5000, 15000];

    private readonly object _gate = new();
    private readonly ConcurrentDictionary<string, long> _failuresByCode = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, long> _latencyBuckets = new(StringComparer.Ordinal);
    private readonly IOptions<AiProviderOptions> _options;
    private readonly IDateTimeProvider _clock;

    private long _total;
    private long _success;
    private long _failure;
    private long _latencySumMs;
    private DateTime? _lastSuccessUtc;

    public AiOperationMetrics(IOptions<AiProviderOptions> options, IDateTimeProvider clock)
    {
        _options = options;
        _clock = clock;
        foreach (var edge in BucketEdgesMs)
        {
            _latencyBuckets[$"le_{edge}"] = 0;
        }

        _latencyBuckets["le_inf"] = 0;
    }

    public void RecordSuccess(string operation, string provider, long latencyMs)
    {
        lock (_gate)
        {
            _total++;
            _success++;
            _latencySumMs += Math.Max(0, latencyMs);
            _lastSuccessUtc = _clock.UtcNow;
            IncrementBucket(latencyMs);
        }
    }

    public void RecordFailure(string operation, string provider, string errorCode, long latencyMs)
    {
        lock (_gate)
        {
            _total++;
            _failure++;
            _latencySumMs += Math.Max(0, latencyMs);
            IncrementBucket(latencyMs);
            var code = string.IsNullOrWhiteSpace(errorCode) ? AiErrorCodes.GenerationFailed : errorCode;
            _failuresByCode.AddOrUpdate(code, 1, static (_, n) => n + 1);
        }
    }

    public AiOperationMetricsSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            var options = _options.Value;
            var avg = _total == 0 ? 0d : (double)_latencySumMs / _total;
            var rate = _total == 0 ? 0d : (double)_success / _total;
            return new AiOperationMetricsSnapshot(
                _total,
                _success,
                _failure,
                Math.Round(rate, 4),
                Math.Round(avg, 2),
                new Dictionary<string, long>(_latencyBuckets, StringComparer.Ordinal),
                _lastSuccessUtc,
                options.Enabled && !string.IsNullOrWhiteSpace(options.ProviderName),
                (options.ProviderName ?? "Fake").Trim(),
                new Dictionary<string, long>(_failuresByCode, StringComparer.Ordinal));
        }
    }

    private void IncrementBucket(long latencyMs)
    {
        foreach (var edge in BucketEdgesMs)
        {
            if (latencyMs <= edge)
            {
                _latencyBuckets.AddOrUpdate($"le_{edge}", 1, static (_, n) => n + 1);
                return;
            }
        }

        _latencyBuckets.AddOrUpdate("le_inf", 1, static (_, n) => n + 1);
    }
}
