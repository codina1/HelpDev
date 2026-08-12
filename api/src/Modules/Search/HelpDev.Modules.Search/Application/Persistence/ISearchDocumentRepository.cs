using HelpDev.Modules.Search.Domain;

namespace HelpDev.Modules.Search.Application.Persistence;

public interface ISearchDocumentRepository
{
    Task<SearchDocument?> GetBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Keyset page of SourceIds for a source type ordered by SourceId ascending.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListSourceIdsByTypeAsync(
        string sourceType,
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default);

    Task AddAsync(SearchDocument document, CancellationToken cancellationToken = default);

    void Remove(SearchDocument document);
}
