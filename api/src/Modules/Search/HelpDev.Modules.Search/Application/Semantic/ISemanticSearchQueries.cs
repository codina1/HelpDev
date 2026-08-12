namespace HelpDev.Modules.Search.Application.Semantic;

public sealed record SearchContextItemDto(
    string Title,
    string Snippet,
    string SourceUrl,
    string SourceType,
    Guid SourceId,
    double Similarity);

public sealed record SearchContextDto(
    string Query,
    IReadOnlyList<SearchContextItemDto> Items);

public interface ISemanticSearchQueries
{
    Task<SearchContextDto> SearchSimilarAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default);

    Task<SearchContextDto> RetrieveContextAsync(
        string query,
        int take = 6,
        CancellationToken cancellationToken = default);

    Task<SearchContextDto> SearchRelatedToSourceAsync(
        string sourceType,
        Guid sourceId,
        int take = 6,
        CancellationToken cancellationToken = default);
}
