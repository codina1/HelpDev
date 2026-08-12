namespace HelpDev.Modules.Search.Domain;

/// <summary>
/// Per-source semantic indexing state for admin dashboards (no embeddings/prompts).
/// </summary>
public sealed class SearchSemanticIndexState
{
    private SearchSemanticIndexState()
    {
    }

    public Guid Id { get; private set; }

    public string SourceType { get; private set; } = string.Empty;

    public Guid SourceId { get; private set; }

    public string Status { get; private set; } = SearchSemanticIndexStatuses.Pending;

    public int ChunkCount { get; private set; }

    public Guid LastEventId { get; private set; }

    public DateTime? LastIndexedAtUtc { get; private set; }

    public string? FailureCode { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static SearchSemanticIndexState Create(
        Guid id,
        string sourceType,
        Guid sourceId,
        DateTime updatedAtUtc)
    {
        return new SearchSemanticIndexState
        {
            Id = id,
            SourceType = sourceType,
            SourceId = sourceId,
            Status = SearchSemanticIndexStatuses.Pending,
            ChunkCount = 0,
            LastEventId = Guid.Empty,
            UpdatedAtUtc = updatedAtUtc,
        };
    }

    public void MarkIndexed(int chunkCount, Guid eventId, DateTime atUtc)
    {
        Status = SearchSemanticIndexStatuses.Indexed;
        ChunkCount = chunkCount;
        LastEventId = eventId;
        LastIndexedAtUtc = atUtc;
        FailureCode = null;
        UpdatedAtUtc = atUtc;
    }

    public void MarkFailed(string failureCode, Guid eventId, DateTime atUtc)
    {
        Status = SearchSemanticIndexStatuses.Failed;
        FailureCode = failureCode;
        LastEventId = eventId;
        UpdatedAtUtc = atUtc;
    }

    public void MarkRemoved(Guid eventId, DateTime atUtc)
    {
        Status = SearchSemanticIndexStatuses.Removed;
        ChunkCount = 0;
        FailureCode = null;
        LastEventId = eventId;
        UpdatedAtUtc = atUtc;
    }
}

public static class SearchSemanticIndexStatuses
{
    public const string Pending = "Pending";
    public const string Indexed = "Indexed";
    public const string Failed = "Failed";
    public const string Removed = "Removed";
}
