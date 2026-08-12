using Microsoft.Extensions.Logging;

namespace HelpDev.Integration.Tests.Helpers;

public sealed class CapturedLogEntry
{
    public required string Category { get; init; }

    public required LogLevel Level { get; init; }

    public required EventId EventId { get; init; }

    public string? EventName { get; init; }

    public required string Message { get; init; }

    public Exception? Exception { get; init; }

    public IReadOnlyDictionary<string, object?> State { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, object?> Scopes { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);

    public string StateAsString =>
        string.Join(
            "; ",
            State.Select(pair => $"{pair.Key}={pair.Value}"));
}
