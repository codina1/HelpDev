namespace HelpDev.Modules.Search.Application.Reindex;

public sealed record SearchReindexRequest(string? SourceType, int BatchSize);

public sealed record SearchReindexResultDto(
    int Scanned,
    int Created,
    int Updated,
    int Removed,
    int Skipped,
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc);

public interface ISearchReindexService
{
    Task<SearchReindexResultDto> ReindexAsync(
        SearchReindexRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SearchReindexException : Exception
{
    public SearchReindexException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class SearchReindexErrorCodes
{
    public const string SourceInvalid = "search_reindex_source_invalid";
    public const string BatchSizeInvalid = "search_reindex_batch_size_invalid";
    public const string AlreadyRunning = "search_reindex_already_running";
}

/// <summary>
/// Multi-instance reindex mutex. Implementations should release automatically on dispose
/// (and on process/connection loss for PostgreSQL session advisory locks).
/// </summary>
public interface ISearchReindexLock
{
    /// <summary>
    /// Attempts to acquire the reindex lock. Returns null when another run holds it.
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken cancellationToken = default);
}
