namespace HelpDev.Modules.Search.Domain;

/// <summary>
/// pgvector-backed embedding row. Never logged or returned via API.
/// </summary>
public sealed class SearchVector
{
    private SearchVector()
    {
    }

    public Guid Id { get; private set; }

    public Guid ChunkId { get; private set; }

    public float[] Embedding { get; private set; } = [];

    public int Dimensions { get; private set; }

    public string Model { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static SearchVector Create(
        Guid id,
        Guid chunkId,
        float[] embedding,
        string model,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || chunkId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        ArgumentNullException.ThrowIfNull(embedding);
        if (embedding.Length is < 8 or > 4096)
        {
            throw new ArgumentOutOfRangeException(nameof(embedding));
        }

        if (string.IsNullOrWhiteSpace(model) || model.Length > 100)
        {
            throw new ArgumentException("Model is invalid.");
        }

        return new SearchVector
        {
            Id = id,
            ChunkId = chunkId,
            Embedding = embedding,
            Dimensions = embedding.Length,
            Model = model.Trim(),
            CreatedAtUtc = createdAtUtc,
        };
    }
}
