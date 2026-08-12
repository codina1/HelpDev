using HelpDev.Modules.Content.Application.InternalLinks;
using HelpDev.Modules.Search.Application.Queries;

namespace HelpDev.Infrastructure.Seo;

/// <summary>
/// Search-backed internal link suggestions. Lives in host infrastructure so Content.Application
/// does not reference the Search module.
/// </summary>
public sealed class InternalLinkSuggestionService : IInternalLinkSuggestionService
{
    private const int PageSize = 8;

    private readonly ISearchQueries _searchQueries;

    public InternalLinkSuggestionService(ISearchQueries searchQueries)
    {
        _searchQueries = searchQueries;
    }

    public async Task<IReadOnlyList<InternalLinkSuggestionDto>> SuggestAsync(
        InternalLinkSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = BuildQuery(request);
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var result = await _searchQueries.SearchAsync(
            query,
            sourceType: "content",
            page: 1,
            pageSize: PageSize,
            cancellationToken);

        return result.Items
            .Where(item => item.SourceId != request.ContentId)
            .Select(item => new InternalLinkSuggestionDto(
                item.SourceId,
                item.Title,
                item.Slug,
                item.Url,
                BuildReason(request, item.Title)))
            .ToList();
    }

    private static string BuildQuery(InternalLinkSuggestionRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.FocusKeyword))
        {
            return request.FocusKeyword.Trim();
        }

        var title = request.Title?.Trim();
        return string.IsNullOrEmpty(title) ? string.Empty : title;
    }

    private static string BuildReason(InternalLinkSuggestionRequest request, string hitTitle)
    {
        if (!string.IsNullOrWhiteSpace(request.FocusKeyword))
        {
            return $"نتیجهٔ جستجو برای کلمهٔ کلیدی «{request.FocusKeyword.Trim()}».";
        }

        return $"نتیجهٔ جستجو مرتبط با عنوان «{request.Title.Trim()}» (مثال: «{hitTitle}»).";
    }
}
