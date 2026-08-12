using HelpDev.SharedContracts.Observability;

namespace HelpDev.Infrastructure.Observability;

public sealed class HealthSnapshotCache : IHealthSnapshotCache
{
    private readonly Dictionary<string, CachedHealthResult> _entries = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    public bool TryGet(string checkName, out CachedHealthResult? result)
    {
        lock (_lock)
        {
            if (_entries.TryGetValue(checkName, out var cached) && cached.ExpiresAtUtc > DateTime.UtcNow)
            {
                result = cached;
                return true;
            }

            result = null;
            return false;
        }
    }

    public void Set(string checkName, CachedHealthResult result)
    {
        lock (_lock)
        {
            _entries[checkName] = result;
        }
    }
}

public sealed class OperationalSafeDetailsSanitizer : IOperationalSafeDetailsSanitizer
{
    private const int MaxEntries = 10;
    private const int MaxKeyLength = 50;
    private const int MaxValueLength = 200;

    private static readonly HashSet<string> AllowedKeys = new(StringComparer.Ordinal)
    {
        "connectivity", "latencyBucket", "pendingBucket", "oldestPendingAgeBucket",
        "deadLetterBucket", "pendingProjectionBucket", "oldestProjectionAgeBucket",
        "lastSuccessfulIndexAgeBucket", "recentProcessingFailureBucket", "latestReceiptAgeBucket",
        "ingestionAvailable", "persistenceAvailable", "latestWriteAgeBucket", "processorEnabled",
        "scope",
    };

    private static readonly string[] SensitivePatterns =
    [
        "password", "otp", "token", "secret", "connection", "exception", "sql", "host",
        "phone", "email", "authorization", "cookie", "payload", "body", "query",
    ];

    public IReadOnlyDictionary<string, string>? Sanitize(IReadOnlyDictionary<string, string>? details)
    {
        if (details is null || details.Count == 0 || details.Count > MaxEntries)
        {
            return null;
        }

        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in details)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > MaxKeyLength || !AllowedKeys.Contains(key))
            {
                continue;
            }

            if (IsSensitive(key) || IsSensitive(value) || value.Length > MaxValueLength ||
                value.Any(static c => char.IsControl(c)))
            {
                continue;
            }

            sanitized[key] = value;
        }

        return sanitized.Count == 0 ? null : sanitized;
    }

    private static bool IsSensitive(string value)
    {
        foreach (var pattern in SensitivePatterns)
        {
            if (value.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
