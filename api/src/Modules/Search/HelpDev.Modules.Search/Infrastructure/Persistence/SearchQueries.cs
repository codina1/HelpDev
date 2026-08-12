using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Search.Infrastructure.Persistence;

/// <summary>
/// v1 search uses PostgreSQL ILIKE with deterministic CASE ranking.
/// Limitation: leading-wildcard contains matches cannot use a simple B-tree optimally.
/// </summary>
public sealed class SearchQueries : ISearchQueries
{
    private readonly ISearchDbContext _dbContext;

    public SearchQueries(ISearchDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchResultDto> SearchAsync(
        string query,
        string? sourceType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalized = query.Trim();
        var escaped = EscapeLike(normalized);
        var exact = escaped;
        var prefix = $"{escaped}%";
        var contains = $"%{escaped}%";

        var documents = _dbContext.SearchDocuments.AsNoTracking()
            .Where(document => document.IsPublished);

        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            documents = documents.Where(document => document.SourceType == sourceType);
        }

        const string likeEscape = "\\";
        documents = documents.Where(document =>
            EF.Functions.ILike(document.Title, exact, likeEscape)
            || EF.Functions.ILike(document.Title, prefix, likeEscape)
            || EF.Functions.ILike(document.Title, contains, likeEscape)
            || EF.Functions.ILike(document.Summary, contains, likeEscape));

        var total = await documents.CountAsync(cancellationToken);

        var items = await documents
            .OrderBy(document =>
                EF.Functions.ILike(document.Title, exact, likeEscape) ? 0
                : EF.Functions.ILike(document.Title, prefix, likeEscape) ? 1
                : EF.Functions.ILike(document.Title, contains, likeEscape) ? 2
                : 3)
            .ThenByDescending(document => document.SourceUpdatedAtUtc)
            .ThenBy(document => document.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(document => new SearchItemDto(
                document.SourceType,
                document.SourceId,
                document.Title,
                document.Slug,
                document.Summary,
                document.Url,
                document.SourcePublishedAtUtc,
                document.SourceUpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new SearchResultDto(normalized, page, pageSize, total, items);
    }

    private static string EscapeLike(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
