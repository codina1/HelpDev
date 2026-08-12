namespace HelpDev.Modules.Search.Domain;

/// <summary>
/// Immutable indexed text chunk for semantic retrieval. Not a cross-module aggregate.
/// </summary>
public sealed class SearchChunk
{
    private SearchChunk()
    {
    }

    public Guid Id { get; private set; }

    public string SourceType { get; private set; } = string.Empty;

    public Guid SourceId { get; private set; }

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    /// <summary>Optional JSON metadata (no secrets, no private drafts).</summary>
    public string? Metadata { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid LastEventId { get; private set; }

    public static SearchChunk Create(
        Guid id,
        string sourceType,
        Guid sourceId,
        int chunkIndex,
        string content,
        string title,
        string? metadata,
        DateTime createdAtUtc,
        Guid lastEventId)
    {
        if (id == Guid.Empty || sourceId == Guid.Empty || lastEventId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        if (chunkIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));
        }

        if (string.IsNullOrWhiteSpace(sourceType) || sourceType.Length > 32)
        {
            throw new ArgumentException("SourceType is invalid.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Length > 300)
        {
            throw new ArgumentException("Title is invalid.");
        }

        return new SearchChunk
        {
            Id = id,
            SourceType = sourceType.Trim(),
            SourceId = sourceId,
            ChunkIndex = chunkIndex,
            Content = content.Trim(),
            Title = title.Trim(),
            Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim(),
            CreatedAtUtc = createdAtUtc,
            LastEventId = lastEventId,
        };
    }
}
