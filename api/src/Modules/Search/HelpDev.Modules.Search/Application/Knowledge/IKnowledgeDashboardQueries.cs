namespace HelpDev.Modules.Search.Application.Knowledge;

public sealed record KnowledgeDashboardDto(
    int IndexedDocuments,
    int TotalChunks,
    int IndexedSources,
    int FailedSources,
    string? SourceFilter,
    IReadOnlyList<KnowledgeSourceStatusDto> RecentFailures,
    IReadOnlyList<KnowledgeSourceStatusDto> RecentIndexed);

public sealed record KnowledgeSourceStatusDto(
    string SourceType,
    Guid SourceId,
    string Status,
    int ChunkCount,
    DateTime UpdatedAtUtc,
    string? FailureCode);

public interface IKnowledgeDashboardQueries
{
    Task<KnowledgeDashboardDto> GetAsync(
        string? sourceType = null,
        CancellationToken cancellationToken = default);
}
