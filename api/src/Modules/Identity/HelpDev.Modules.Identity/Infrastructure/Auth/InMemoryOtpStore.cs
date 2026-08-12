using System.Collections.Concurrent;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Identity.Infrastructure.Auth;

public sealed class InMemoryOtpStore : IOtpStore
{
    private readonly ConcurrentDictionary<string, OtpEntry> _entries = new();
    private readonly IDateTimeProvider _clock;
    private readonly int _maxFailedAttempts;

    public InMemoryOtpStore(IDateTimeProvider clock, IOptions<OtpSettings> options)
    {
        _clock = clock;
        _maxFailedAttempts = options.Value.MaxFailedAttempts;
    }

    public Task StoreAsync(
        string mobile,
        string code,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        _entries[mobile] = new OtpEntry(code, _clock.UtcNow.Add(expiration), FailedAttempts: 0);
        return Task.CompletedTask;
    }

    public Task<bool> ValidateAndRemoveAsync(
        string mobile,
        string code,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (!_entries.TryGetValue(mobile, out var entry))
            {
                return Task.FromResult(false);
            }

            if (entry.ExpiresAt <= _clock.UtcNow)
            {
                TryRemoveExact(mobile, entry);
                return Task.FromResult(false);
            }

            if (string.Equals(entry.Code, code, StringComparison.Ordinal))
            {
                if (TryRemoveExact(mobile, entry))
                {
                    return Task.FromResult(true);
                }

                continue;
            }

            var nextAttempts = entry.FailedAttempts + 1;
            if (nextAttempts >= _maxFailedAttempts)
            {
                if (!TryRemoveExact(mobile, entry))
                {
                    continue;
                }

                return Task.FromResult(false);
            }

            var updated = entry with { FailedAttempts = nextAttempts };
            if (_entries.TryUpdate(mobile, updated, entry))
            {
                return Task.FromResult(false);
            }
        }
    }

    private bool TryRemoveExact(string mobile, OtpEntry entry) =>
        ((ICollection<KeyValuePair<string, OtpEntry>>)_entries)
            .Remove(new KeyValuePair<string, OtpEntry>(mobile, entry));

    private sealed record OtpEntry(string Code, DateTime ExpiresAt, int FailedAttempts);
}
