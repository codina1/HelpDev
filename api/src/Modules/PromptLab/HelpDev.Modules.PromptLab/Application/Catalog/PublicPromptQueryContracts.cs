using HelpDev.Modules.PromptLab.Domain.Prompts;

namespace HelpDev.Modules.PromptLab.Application.Catalog;

public sealed record PublicPromptCategoryRefDto(
    Guid Id,
    string Name,
    string Slug);

public sealed record PublicPromptAiModelRefDto(
    Guid Id,
    string Name,
    string Slug,
    string Provider);

public sealed record PublicPromptListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string? CoverImage,
    string MediaType,
    PublicPromptCategoryRefDto Category,
    PublicPromptAiModelRefDto AiModel,
    int Views,
    int CopyCount,
    DateTime? PublishedAt);

public sealed record PublicPromptDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string Content,
    string? CoverImage,
    string MediaType,
    PublicPromptCategoryRefDto Category,
    PublicPromptAiModelRefDto AiModel,
    int Views,
    int CopyCount,
    DateTime? PublishedAt);

public sealed record PublicPromptFilter(
    string? Category,
    string? AiModel,
    string? MediaType,
    string? Search,
    bool Popular,
    int Page,
    int PageSize);

public sealed record PublicPromptPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<PublicPromptListItemDto> Items);

public interface IPromptPublicQueries
{
    Task<PublicPromptPageDto> GetPromptsAsync(
        PublicPromptFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an approved prompt that is publicly readable.
    /// Returns null for missing slugs and for Draft, Submitted, or Rejected prompts
    /// so unpublished items are indistinguishable from not found.
    /// </summary>
    Task<PublicPromptDetailsDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}

public static class PublicPromptMediaTypes
{
    public static bool TryParse(string? value, out PromptMediaType mediaType)
    {
        mediaType = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out mediaType)
            && Enum.IsDefined(mediaType);
    }
}
