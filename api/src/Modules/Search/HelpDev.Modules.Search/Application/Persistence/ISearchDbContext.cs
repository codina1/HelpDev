using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Search.Application.Persistence;

public interface ISearchDbContext
{
    DbSet<SearchDocument> SearchDocuments { get; }

    DbSet<SearchChunk> SearchChunks { get; }

    DbSet<SearchVector> SearchVectors { get; }

    DbSet<SearchSemanticIndexState> SearchSemanticIndexStates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISearchChunkRepository
{
    Task<IReadOnlyList<SearchChunk>> ListBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<SearchChunk> chunks, CancellationToken cancellationToken = default);

    void RemoveRange(IEnumerable<SearchChunk> chunks);
}

public interface ISearchVectorRepository
{
    Task AddRangeAsync(IEnumerable<SearchVector> vectors, CancellationToken cancellationToken = default);

    Task RemoveByChunkIdsAsync(IEnumerable<Guid> chunkIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SemanticHit>> SearchSimilarAsync(
        float[] queryVector,
        int take,
        CancellationToken cancellationToken = default);
}

public interface ISearchSemanticIndexStateRepository
{
    Task<SearchSemanticIndexState?> GetAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default);

    Task AddAsync(SearchSemanticIndexState state, CancellationToken cancellationToken = default);

    Task<KnowledgeCounts> GetCountsAsync(
        string? sourceType = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchSemanticIndexState>> ListRecentByStatusAsync(
        string status,
        int take,
        string? sourceType = null,
        CancellationToken cancellationToken = default);
}

public sealed record SemanticHit(
    Guid ChunkId,
    string Title,
    string Content,
    string SourceType,
    Guid SourceId,
    string? Metadata,
    double Similarity);

public sealed record KnowledgeCounts(
    int IndexedDocuments,
    int TotalChunks,
    int IndexedSources,
    int FailedSources);
