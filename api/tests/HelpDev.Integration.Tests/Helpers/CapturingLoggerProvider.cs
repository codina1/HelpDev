using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HelpDev.Integration.Tests.Helpers;

public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<CapturedLogEntry> _entries;

    public CapturingLoggerProvider(ConcurrentBag<CapturedLogEntry> entries)
    {
        _entries = entries;
    }

    public ILogger CreateLogger(string categoryName) =>
        new CapturingLogger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ConcurrentBag<CapturedLogEntry> _entries;
        private readonly AsyncLocal<Stack<IReadOnlyDictionary<string, object?>>> _scopes = new();

        public CapturingLogger(string categoryName, ConcurrentBag<CapturedLogEntry> entries)
        {
            _categoryName = categoryName;
            _entries = entries;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            var scopeValues = ExtractDictionary(state);
            var stack = _scopes.Value ??= new Stack<IReadOnlyDictionary<string, object?>>();
            stack.Push(scopeValues);
            return new ScopeHandle(stack);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var stateValues = ExtractDictionary(state);
            var scopes = MergeScopes();
            var eventName = eventId.Name
                ?? TryGetString(stateValues, "Event")
                ?? TryGetString(stateValues, "{OriginalFormat}");

            if (stateValues.TryGetValue("Event", out var eventValue) && eventValue is not null)
            {
                eventName = eventValue.ToString();
            }

            _entries.Add(new CapturedLogEntry
            {
                Category = _categoryName,
                Level = logLevel,
                EventId = eventId,
                EventName = eventName,
                Message = formatter(state, exception),
                Exception = exception,
                State = stateValues,
                Scopes = scopes,
            });
        }

        private IReadOnlyDictionary<string, object?> MergeScopes()
        {
            var merged = new Dictionary<string, object?>(StringComparer.Ordinal);
            var stack = _scopes.Value;
            if (stack is null || stack.Count == 0)
            {
                return merged;
            }

            foreach (var scope in stack.Reverse())
            {
                foreach (var pair in scope)
                {
                    merged[pair.Key] = pair.Value;
                }
            }

            return merged;
        }

        private static IReadOnlyDictionary<string, object?> ExtractDictionary<TState>(TState state)
        {
            var values = new Dictionary<string, object?>(StringComparer.Ordinal);
            if (state is IEnumerable<KeyValuePair<string, object?>> nullablePairs)
            {
                foreach (var pair in nullablePairs)
                {
                    values[pair.Key] = pair.Value;
                }

                return values;
            }

            if (state is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                foreach (var pair in pairs)
                {
                    values[pair.Key] = pair.Value;
                }
            }

            return values;
        }

        private static string? TryGetString(IReadOnlyDictionary<string, object?> values, string key) =>
            values.TryGetValue(key, out var value) ? value?.ToString() : null;

        private sealed class ScopeHandle : IDisposable
        {
            private readonly Stack<IReadOnlyDictionary<string, object?>> _stack;
            private bool _disposed;

            public ScopeHandle(Stack<IReadOnlyDictionary<string, object?>> stack)
            {
                _stack = stack;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                if (_stack.Count > 0)
                {
                    _stack.Pop();
                }

                _disposed = true;
            }
        }
    }
}
