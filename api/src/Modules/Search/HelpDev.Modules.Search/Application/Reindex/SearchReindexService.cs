using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Search.Application.Reindex;

public sealed class SearchReindexService : ISearchReindexService
{
    public const int DefaultBatchSize = 100;
    public const int MinBatchSize = 10;
    public const int MaxBatchSize = 500;

    private readonly IContentSearchSource _contentSearchSource;
    private readonly ICourseSearchSource _courseSearchSource;
    private readonly ISearchProjectionService _projectionService;
    private readonly ISearchDocumentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISearchReindexLock _reindexLock;
    private readonly IDateTimeProvider _clock;

    public SearchReindexService(
        IContentSearchSource contentSearchSource,
        ICourseSearchSource courseSearchSource,
        ISearchProjectionService projectionService,
        ISearchDocumentRepository repository,
        IUnitOfWork unitOfWork,
        ISearchReindexLock reindexLock,
        IDateTimeProvider clock)
    {
        _contentSearchSource = contentSearchSource;
        _courseSearchSource = courseSearchSource;
        _projectionService = projectionService;
        _repository = repository;
        _unitOfWork = unitOfWork;
        _reindexLock = reindexLock;
        _clock = clock;
    }

    public async Task<SearchReindexResultDto> ReindexAsync(
        SearchReindexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var sourceTypes = NormalizeSourceTypes(request.SourceType);
        var batchSize = NormalizeBatchSize(request.BatchSize);

        await using var lease = await _reindexLock.TryAcquireAsync(cancellationToken);
        if (lease is null)
        {
            throw new SearchReindexException(
                "Search reindex is already running.",
                SearchReindexErrorCodes.AlreadyRunning);
        }

        var startedAt = _clock.UtcNow;
        var reindexRunId = Guid.NewGuid();
        var totals = new Counters();

        foreach (var sourceType in sourceTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await UpsertPublishedSourcesAsync(sourceType, batchSize, reindexRunId, totals, cancellationToken);
            await RemoveOrphansAsync(sourceType, batchSize, reindexRunId, totals, cancellationToken);
        }

        return new SearchReindexResultDto(
            totals.Scanned,
            totals.Created,
            totals.Updated,
            totals.Removed,
            totals.Skipped,
            startedAt,
            _clock.UtcNow);
    }

    private async Task UpsertPublishedSourcesAsync(
        string sourceType,
        int batchSize,
        Guid reindexRunId,
        Counters totals,
        CancellationToken cancellationToken)
    {
        Guid? afterSourceId = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batch = await GetPublishedBatchAsync(sourceType, afterSourceId, batchSize, cancellationToken);
            if (batch.Count == 0)
            {
                break;
            }

            var mutated = false;
            foreach (var source in batch)
            {
                totals.Scanned++;
                var outcome = await _projectionService.StageReindexAsync(
                    sourceType,
                    source.SourceId,
                    source,
                    reindexRunId,
                    cancellationToken);
                ApplyOutcome(totals, outcome, ref mutated);
            }

            if (mutated)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            afterSourceId = batch[^1].SourceId;
            if (batch.Count < batchSize)
            {
                break;
            }
        }
    }

    private async Task RemoveOrphansAsync(
        string sourceType,
        int batchSize,
        Guid reindexRunId,
        Counters totals,
        CancellationToken cancellationToken)
    {
        Guid? afterSourceId = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sourceIds = await _repository.ListSourceIdsByTypeAsync(
                sourceType,
                afterSourceId,
                batchSize,
                cancellationToken);
            if (sourceIds.Count == 0)
            {
                break;
            }

            var mutated = false;
            foreach (var sourceId in sourceIds)
            {
                var source = await GetByIdAsync(sourceType, sourceId, cancellationToken);
                if (source is not null)
                {
                    continue;
                }

                var outcome = await _projectionService.StageReindexAsync(
                    sourceType,
                    sourceId,
                    source: null,
                    reindexRunId,
                    cancellationToken);
                ApplyOutcome(totals, outcome, ref mutated);
            }

            if (mutated)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            afterSourceId = sourceIds[^1];
            if (sourceIds.Count < batchSize)
            {
                break;
            }
        }
    }

    private Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        string sourceType,
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken) =>
        sourceType switch
        {
            SearchSourceTypes.Content => _contentSearchSource.GetPublishedBatchAsync(
                afterSourceId,
                take,
                cancellationToken),
            SearchSourceTypes.Course => _courseSearchSource.GetPublishedBatchAsync(
                afterSourceId,
                take,
                cancellationToken),
            _ => throw new SearchReindexException(
                $"Unsupported search source type '{sourceType}'.",
                SearchReindexErrorCodes.SourceInvalid),
        };

    private Task<SearchSourceDocument?> GetByIdAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken) =>
        sourceType switch
        {
            SearchSourceTypes.Content => _contentSearchSource.GetByIdAsync(sourceId, cancellationToken),
            SearchSourceTypes.Course => _courseSearchSource.GetByIdAsync(sourceId, cancellationToken),
            _ => throw new SearchReindexException(
                $"Unsupported search source type '{sourceType}'.",
                SearchReindexErrorCodes.SourceInvalid),
        };

    private static IReadOnlyList<string> NormalizeSourceTypes(string? sourceType)
    {
        if (string.IsNullOrWhiteSpace(sourceType))
        {
            return [SearchSourceTypes.Content, SearchSourceTypes.Course];
        }

        var normalized = sourceType.Trim().ToLowerInvariant();
        if (normalized is not (SearchSourceTypes.Content or SearchSourceTypes.Course))
        {
            throw new SearchReindexException(
                $"Unsupported reindex source type '{sourceType}'. Lexical reindex supports content and course only.",
                SearchReindexErrorCodes.SourceInvalid);
        }

        return [normalized];
    }

    private static int NormalizeBatchSize(int batchSize)
    {
        if (batchSize < MinBatchSize || batchSize > MaxBatchSize)
        {
            throw new SearchReindexException(
                $"Batch size must be between {MinBatchSize} and {MaxBatchSize}.",
                SearchReindexErrorCodes.BatchSizeInvalid);
        }

        return batchSize;
    }

    private static void ApplyOutcome(Counters totals, SearchProjectionOutcome outcome, ref bool mutated)
    {
        switch (outcome)
        {
            case SearchProjectionOutcome.Created:
                totals.Created++;
                mutated = true;
                break;
            case SearchProjectionOutcome.Updated:
                totals.Updated++;
                mutated = true;
                break;
            case SearchProjectionOutcome.Removed:
                totals.Removed++;
                mutated = true;
                break;
            case SearchProjectionOutcome.Skipped:
            case SearchProjectionOutcome.NoOp:
                totals.Skipped++;
                break;
        }
    }

    private sealed class Counters
    {
        public int Scanned { get; set; }

        public int Created { get; set; }

        public int Updated { get; set; }

        public int Removed { get; set; }

        public int Skipped { get; set; }
    }
}
