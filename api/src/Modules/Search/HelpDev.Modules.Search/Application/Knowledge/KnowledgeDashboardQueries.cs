using HelpDev.Modules.Search.Application.Knowledge;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;

namespace HelpDev.Modules.Search.Application.Knowledge;

public sealed class KnowledgeDashboardQueries : IKnowledgeDashboardQueries
{
    private readonly ISearchSemanticIndexStateRepository _stateRepository;

    public KnowledgeDashboardQueries(ISearchSemanticIndexStateRepository stateRepository)
    {
        _stateRepository = stateRepository;
    }

    public async Task<KnowledgeDashboardDto> GetAsync(
        string? sourceType = null,
        CancellationToken cancellationToken = default)
    {
        string? filter = null;
        if (!string.IsNullOrWhiteSpace(sourceType)
            && !string.Equals(sourceType.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            filter = SearchSourceTypes.NormalizeOrThrow(sourceType);
        }

        var counts = await _stateRepository.GetCountsAsync(filter, cancellationToken);
        var failures = await _stateRepository.ListRecentByStatusAsync(
            SearchSemanticIndexStatuses.Failed,
            take: 20,
            filter,
            cancellationToken);
        var indexed = await _stateRepository.ListRecentByStatusAsync(
            SearchSemanticIndexStatuses.Indexed,
            take: 20,
            filter,
            cancellationToken);

        return new KnowledgeDashboardDto(
            counts.IndexedDocuments,
            counts.TotalChunks,
            counts.IndexedSources,
            counts.FailedSources,
            filter,
            failures.Select(Map).ToList(),
            indexed.Select(Map).ToList());
    }

    private static KnowledgeSourceStatusDto Map(SearchSemanticIndexState state) =>
        new(
            state.SourceType,
            state.SourceId,
            state.Status,
            state.ChunkCount,
            state.UpdatedAtUtc,
            state.FailureCode);
}
