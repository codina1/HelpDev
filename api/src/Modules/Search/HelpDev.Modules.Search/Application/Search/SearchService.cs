using HelpDev.Modules.Search.Application.Dtos;
using HelpDev.Modules.Search.Application.Queries;
using HelpDev.Modules.Search.Domain;

namespace HelpDev.Modules.Search.Application.Search;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(
        string? query,
        string? sourceType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public sealed class SearchException : Exception
{
    public SearchException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class SearchErrorCodes
{
    public const string QueryRequired = "search_query_required";
    public const string QueryTooLong = "search_query_too_long";
    public const string PageInvalid = "search_page_invalid";
    public const string PageSizeInvalid = "search_page_size_invalid";
    public const string TypeInvalid = "search_type_invalid";
}

public sealed class SearchService : ISearchService
{
    public const int MaxQueryLength = 200;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 50;

    private readonly ISearchQueries _searchQueries;

    public SearchService(ISearchQueries searchQueries)
    {
        _searchQueries = searchQueries;
    }

    public Task<SearchResultDto> SearchAsync(
        string? query,
        string? sourceType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new SearchException("Search query is required.", SearchErrorCodes.QueryRequired);
        }

        var trimmed = query.Trim();
        if (trimmed.Length > MaxQueryLength)
        {
            throw new SearchException(
                $"Search query must be at most {MaxQueryLength} characters.",
                SearchErrorCodes.QueryTooLong);
        }

        if (page < 1)
        {
            throw new SearchException("Page must be greater than or equal to 1.", SearchErrorCodes.PageInvalid);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new SearchException(
                $"Page size must be between 1 and {MaxPageSize}.",
                SearchErrorCodes.PageSizeInvalid);
        }

        string? normalizedType = null;
        if (!string.IsNullOrWhiteSpace(sourceType))
        {
            normalizedType = sourceType.Trim().ToLowerInvariant();
            if (!SearchSourceTypes.IsKnown(normalizedType))
            {
                throw new SearchException(
                    $"Unsupported search type '{sourceType}'.",
                    SearchErrorCodes.TypeInvalid);
            }
        }

        return _searchQueries.SearchAsync(trimmed, normalizedType, page, pageSize, cancellationToken);
    }
}
