using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

public sealed class SearchDocumentRepository : ISearchDocumentRepository
{
    private readonly ISearchDbContext _dbContext;

    public SearchDocumentRepository(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SearchDocument?> GetBySourceAsync(
        string sourceType,
        Guid sourceId,
        CancellationToken cancellationToken = default) =>
        _dbContext.SearchDocuments.FirstOrDefaultAsync(
            document => document.SourceType == sourceType && document.SourceId == sourceId,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListSourceIdsByTypeAsync(
        string sourceType,
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var query = _dbContext.SearchDocuments.AsNoTracking()
            .Where(document => document.SourceType == sourceType);

        if (afterSourceId.HasValue)
        {
            var after = afterSourceId.Value;
            query = query.Where(document => document.SourceId > after);
        }

        return await query
            .OrderBy(document => document.SourceId)
            .Take(take)
            .Select(document => document.SourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SearchDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        await _dbContext.SearchDocuments.AddAsync(document, cancellationToken);
    }

    public void Remove(SearchDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        _dbContext.SearchDocuments.Remove(document);
    }
}
