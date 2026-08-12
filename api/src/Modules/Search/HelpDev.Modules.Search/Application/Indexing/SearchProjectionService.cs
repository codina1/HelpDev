using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Search.Application.Indexing;

public interface ISearchProjectionService
{
    /// <summary>
    /// Event-driven projection. Commits immediately. Overwrites LastEventId with the event id.
    /// </summary>
    Task ApplyAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid eventId,
        DateTime eventOccurredAtUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reindex/backfill staging. Does not commit. Preserves LastEventId on existing rows.
    /// New rows use <paramref name="reindexRunId"/> as LastEventId.
    /// </summary>
    Task<SearchProjectionOutcome> StageReindexAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid reindexRunId,
        CancellationToken cancellationToken = default);
}

public sealed class SearchProjectionService : ISearchProjectionService
{
    private readonly ISearchDocumentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public SearchProjectionService(
        ISearchDocumentRepository repository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task ApplyAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid eventId,
        DateTime eventOccurredAtUtc,
        CancellationToken cancellationToken = default)
    {
        var outcome = await StageCoreAsync(
            sourceType,
            sourceId,
            source,
            eventId,
            eventOccurredAtUtc,
            preserveLastEventId: false,
            cancellationToken);

        if (outcome is SearchProjectionOutcome.Created
            or SearchProjectionOutcome.Updated
            or SearchProjectionOutcome.Removed)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public Task<SearchProjectionOutcome> StageReindexAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid reindexRunId,
        CancellationToken cancellationToken = default)
    {
        if (reindexRunId == Guid.Empty)
        {
            throw new ArgumentException("Reindex run id must not be empty.", nameof(reindexRunId));
        }

        // Reindex uses source UpdatedAtUtc only (no synthetic event time).
        return StageCoreAsync(
            sourceType,
            sourceId,
            source,
            reindexRunId,
            eventOccurredAtUtc: source?.UpdatedAtUtc ?? DateTime.MinValue,
            preserveLastEventId: true,
            cancellationToken);
    }

    private async Task<SearchProjectionOutcome> StageCoreAsync(
        string sourceType,
        Guid sourceId,
        SearchSourceDocument? source,
        Guid operationId,
        DateTime eventOccurredAtUtc,
        bool preserveLastEventId,
        CancellationToken cancellationToken)
    {
        var normalizedType = SearchSourceTypes.NormalizeOrThrow(sourceType);
        var existing = await _repository.GetBySourceAsync(normalizedType, sourceId, cancellationToken);

        if (!preserveLastEventId
            && existing is not null
            && existing.LastEventId == operationId)
        {
            return SearchProjectionOutcome.Skipped;
        }

        if (source is null || !source.IsPublished)
        {
            if (existing is null)
            {
                return SearchProjectionOutcome.NoOp;
            }

            _repository.Remove(existing);
            return SearchProjectionOutcome.Removed;
        }

        var incomingUpdatedAt = preserveLastEventId
            ? source.UpdatedAtUtc
            : source.UpdatedAtUtc >= eventOccurredAtUtc
                ? source.UpdatedAtUtc
                : eventOccurredAtUtc;

        if (existing is not null && incomingUpdatedAt < existing.SourceUpdatedAtUtc)
        {
            return SearchProjectionOutcome.Skipped;
        }

        if (existing is not null
            && incomingUpdatedAt == existing.SourceUpdatedAtUtc
            && IsSamePublicProjection(existing, source))
        {
            return SearchProjectionOutcome.Skipped;
        }

        var now = _clock.UtcNow;
        if (existing is null)
        {
            await _repository.AddAsync(
                new SearchDocument
                {
                    Id = Guid.NewGuid(),
                    SourceType = normalizedType,
                    SourceId = sourceId,
                    Title = source.Title,
                    Slug = source.Slug,
                    Summary = source.Summary,
                    Url = source.Url,
                    IsPublished = true,
                    SourcePublishedAtUtc = source.PublishedAtUtc,
                    SourceUpdatedAtUtc = incomingUpdatedAt,
                    IndexedAtUtc = now,
                    LastEventId = operationId,
                },
                cancellationToken);
            return SearchProjectionOutcome.Created;
        }

        existing.Title = source.Title;
        existing.Slug = source.Slug;
        existing.Summary = source.Summary;
        existing.Url = source.Url;
        existing.IsPublished = true;
        existing.SourcePublishedAtUtc = source.PublishedAtUtc;
        existing.SourceUpdatedAtUtc = incomingUpdatedAt;
        existing.IndexedAtUtc = now;
        if (!preserveLastEventId)
        {
            existing.LastEventId = operationId;
        }

        return SearchProjectionOutcome.Updated;
    }

    private static bool IsSamePublicProjection(SearchDocument existing, SearchSourceDocument source) =>
        string.Equals(existing.Title, source.Title, StringComparison.Ordinal)
        && string.Equals(existing.Slug, source.Slug, StringComparison.Ordinal)
        && string.Equals(existing.Summary, source.Summary, StringComparison.Ordinal)
        && string.Equals(existing.Url, source.Url, StringComparison.Ordinal)
        && existing.IsPublished == source.IsPublished
        && existing.SourcePublishedAtUtc == source.PublishedAtUtc;
}
