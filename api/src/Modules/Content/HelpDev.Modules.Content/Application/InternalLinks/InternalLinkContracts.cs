namespace HelpDev.Modules.Content.Application.InternalLinks;

/// <summary>
/// Suggests internal links using the existing search index only (no AI, no embeddings).
/// </summary>
public interface IInternalLinkSuggestionService
{
    Task<IReadOnlyList<InternalLinkSuggestionDto>> SuggestAsync(
        InternalLinkSuggestionRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record InternalLinkSuggestionRequest(
    Guid ContentId,
    string ContentType,
    string Title,
    string? FocusKeyword);

public sealed record InternalLinkSuggestionDto(
    Guid TargetContentId,
    string Title,
    string Slug,
    string Url,
    string Reason);
