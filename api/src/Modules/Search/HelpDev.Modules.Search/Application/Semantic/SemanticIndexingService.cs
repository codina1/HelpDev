using HelpDev.Modules.Search.Application.Chunking;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Search.Application.Semantic;

public interface ISemanticIndexingService
{
    /// <summary>
    /// Idempotent chunk+embed pipeline for a published source. Safe for Outbox retries.
    /// When <paramref name="source"/> is null or unpublished, removes semantic data for the ids.
    /// </summary>
    Task ApplyAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid eventId,
        CancellationToken cancellationToken = default);
}

public sealed class SemanticIndexingService : ISemanticIndexingService
{
    public const string FailureCodeEmbedding = "embedding_failed";
    public const string FailureCodeUnknown = "indexing_failed";

    private readonly IKnowledgeChunker _chunker;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly ISearchChunkRepository _chunkRepository;
    private readonly ISearchVectorRepository _vectorRepository;
    private readonly ISearchSemanticIndexStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<SemanticIndexingService> _logger;

    public SemanticIndexingService(
        IKnowledgeChunker chunker,
        IEmbeddingGenerator embeddingGenerator,
        ISearchChunkRepository chunkRepository,
        ISearchVectorRepository vectorRepository,
        ISearchSemanticIndexStateRepository stateRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<SemanticIndexingService> logger)
    {
        _chunker = chunker;
        _embeddingGenerator = embeddingGenerator;
        _chunkRepository = chunkRepository;
        _vectorRepository = vectorRepository;
        _stateRepository = stateRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task ApplyAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty || sourceId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        var normalizedType = SearchSourceTypes.NormalizeOrThrow(sourceType);
        var state = await GetOrCreateStateAsync(normalizedType, sourceId, cancellationToken);

        if (source is null || !source.IsPublished)
        {
            if (state.LastEventId == eventId && state.Status == SearchSemanticIndexStatuses.Removed)
            {
                return;
            }

            await ClearChunksAsync(normalizedType, sourceId, cancellationToken);
            state.MarkRemoved(eventId, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        if (state.LastEventId == eventId && state.Status == SearchSemanticIndexStatuses.Indexed)
        {
            return;
        }

        try
        {
            await ClearChunksAsync(normalizedType, sourceId, cancellationToken);

            var text = string.IsNullOrWhiteSpace(source.Body) ? source.Summary : source.Body!;
            var chunkDtos = _chunker.Chunk(source.Title, text, source.Url);
            var now = _clock.UtcNow;
            var chunks = new List<SearchChunk>(chunkDtos.Count);
            var vectors = new List<SearchVector>(chunkDtos.Count);

            foreach (var dto in chunkDtos)
            {
                var chunkId = Guid.NewGuid();
                chunks.Add(SearchChunk.Create(
                    chunkId,
                    normalizedType,
                    sourceId,
                    dto.ChunkIndex,
                    dto.Content,
                    dto.Title,
                    dto.Metadata,
                    now,
                    eventId));

                var embedding = await _embeddingGenerator.GenerateAsync(dto.Content, cancellationToken);
                vectors.Add(SearchVector.Create(
                    Guid.NewGuid(),
                    chunkId,
                    embedding.Vector,
                    embedding.Model,
                    now));
            }

            if (chunks.Count > 0)
            {
                await _chunkRepository.AddRangeAsync(chunks, cancellationToken);
                await _vectorRepository.AddRangeAsync(vectors, cancellationToken);
            }

            state.MarkIndexed(chunks.Count, eventId, now);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Never log prompts, chunk bodies, or embedding vectors.
            _logger.LogWarning(
                ex,
                "Semantic indexing failed for {SourceType}/{SourceId}",
                normalizedType,
                sourceId);

            state.MarkFailed(
                ex is InvalidOperationException ? FailureCodeEmbedding : FailureCodeUnknown,
                eventId,
                _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task ClearChunksAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var existing = await _chunkRepository.ListBySourceAsync(sourceType, sourceId, cancellationToken);
        if (existing.Count == 0)
        {
            return;
        }

        await _vectorRepository.RemoveByChunkIdsAsync(existing.Select(c => c.Id), cancellationToken);
        _chunkRepository.RemoveRange(existing);
    }

    private async Task<SearchSemanticIndexState> GetOrCreateStateAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var existing = await _stateRepository.GetAsync(sourceType, sourceId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = SearchSemanticIndexState.Create(Guid.NewGuid(), sourceType, sourceId, _clock.UtcNow);
        await _stateRepository.AddAsync(created, cancellationToken);
        return created;
    }
}
