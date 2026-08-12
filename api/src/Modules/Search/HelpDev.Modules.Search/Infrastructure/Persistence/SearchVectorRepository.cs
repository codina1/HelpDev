using System.Data.Common;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchChunkRepository : ISearchChunkRepository
{
    private readonly ISearchDbContext _dbContext;

    public SearchChunkRepository(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SearchChunk>> ListBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.SearchChunks
            .Where(chunk => chunk.SourceType == sourceType && chunk.SourceId == sourceId)
            .OrderBy(chunk => chunk.ChunkIndex)
            .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<SearchChunk> chunks, CancellationToken cancellationToken = default) =>
        await _dbContext.SearchChunks.AddRangeAsync(chunks, cancellationToken);

    public void RemoveRange(IEnumerable<SearchChunk> chunks) =>
        _dbContext.SearchChunks.RemoveRange(chunks);
}

public sealed class SearchVectorRepository : ISearchVectorRepository
{
    private readonly ISearchDbContext _dbContext;

    public SearchVectorRepository(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(IEnumerable<SearchVector> vectors, CancellationToken cancellationToken = default) =>
        await _dbContext.SearchVectors.AddRangeAsync(vectors, cancellationToken);

    public async Task RemoveByChunkIdsAsync(IEnumerable<Guid> chunkIds, CancellationToken cancellationToken = default)
    {
        var ids = chunkIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return;
        }

        var existing = await _dbContext.SearchVectors
            .Where(vector => ids.Contains(vector.ChunkId))
            .ToListAsync(cancellationToken);
        _dbContext.SearchVectors.RemoveRange(existing);
    }

    public async Task<IReadOnlyList<SemanticHit>> SearchSimilarAsync(
        float[] queryVector,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        ArgumentNullException.ThrowIfNull(queryVector);

        if (_dbContext is not DbContext ef)
        {
            throw new InvalidOperationException("Search vector queries require an EF DbContext.");
        }

        var connection = ef.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        // PK columns are stored as quoted "Id" (EF default); other columns use snake_case.
        command.CommandText = """
            SELECT c."Id", c.title, c.content, c.source_type, c.source_id, c.metadata,
                   1 - (v.embedding <=> @q) AS similarity
            FROM search_vectors v
            INNER JOIN search_chunks c ON c."Id" = v.chunk_id
            ORDER BY v.embedding <=> @q
            LIMIT @take
            """;

        AddParameter(command, "q", new Vector(queryVector));
        AddParameter(command, "take", take);

        var results = new List<SemanticHit>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new SemanticHit(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetGuid(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetDouble(6)));
        }

        return results;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}

public sealed class SearchSemanticIndexStateRepository : ISearchSemanticIndexStateRepository
{
    private readonly ISearchDbContext _dbContext;

    public SearchSemanticIndexStateRepository(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SearchSemanticIndexState?> GetAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default) =>
        _dbContext.SearchSemanticIndexStates
            .FirstOrDefaultAsync(
                state => state.SourceType == sourceType && state.SourceId == sourceId,
                cancellationToken);

    public async Task AddAsync(SearchSemanticIndexState state, CancellationToken cancellationToken = default) =>
        await _dbContext.SearchSemanticIndexStates.AddAsync(state, cancellationToken);

    public async Task<KnowledgeCounts> GetCountsAsync(
        string? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        var indexedDocuments = await _dbContext.SearchDocuments.CountAsync(
            document => document.IsPublished
                && (sourceType == null || document.SourceType == sourceType),
            cancellationToken);
        var totalChunks = await _dbContext.SearchChunks.CountAsync(
            chunk => sourceType == null || chunk.SourceType == sourceType,
            cancellationToken);
        var indexedSources = await _dbContext.SearchSemanticIndexStates.CountAsync(
            state => state.Status == SearchSemanticIndexStatuses.Indexed
                && (sourceType == null || state.SourceType == sourceType),
            cancellationToken);
        var failedSources = await _dbContext.SearchSemanticIndexStates.CountAsync(
            state => state.Status == SearchSemanticIndexStatuses.Failed
                && (sourceType == null || state.SourceType == sourceType),
            cancellationToken);

        return new KnowledgeCounts(indexedDocuments, totalChunks, indexedSources, failedSources);
    }

    public async Task<IReadOnlyList<SearchSemanticIndexState>> ListRecentByStatusAsync(
        string status,
        int take,
        string? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SearchSemanticIndexStates
            .Where(state => state.Status == status);

        if (sourceType is not null)
        {
            query = query.Where(state => state.SourceType == sourceType);
        }

        return await query
            .OrderByDescending(state => state.UpdatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
