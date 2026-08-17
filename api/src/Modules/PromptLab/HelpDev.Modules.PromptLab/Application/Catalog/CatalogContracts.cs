namespace HelpDev.Modules.PromptLab.Application.Catalog;

public sealed record PromptCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder);

public sealed record PromptAiModelDto(
    Guid Id,
    string Name,
    string Slug,
    string Provider);

public sealed record PromptCatalogItemDto(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    Guid CategoryId,
    string CategoryName,
    string Purpose,
    string Visibility,
    bool RequiresAuthentication,
    int DisplayOrder,
    int? PublishedVersionNumber);

public sealed record PromptDetailsCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon);

public sealed record PromptVariableDto(
    string Name,
    string Label,
    string? Description,
    string Type,
    bool IsRequired,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? ValidationPattern,
    IReadOnlyList<string> AllowedValues,
    int DisplayOrder);

public sealed record PromptDetailsDto(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Description,
    string Purpose,
    string Visibility,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder,
    int PublishedVersionNumber,
    string Template,
    IReadOnlyList<PromptVariableDto> Variables,
    PromptDetailsCategoryDto Category);

public sealed record PromptCatalogFilter(
    string? CategorySlug,
    string? Purpose,
    string? Search,
    int Page,
    int PageSize);

public static class PromptLabPaging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public sealed record PromptCatalogPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<PromptCatalogItemDto> Items);

public interface IPromptCatalogQueries
{
    Task<IReadOnlyList<PromptCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptAiModelDto>> GetAiModelsAsync(CancellationToken cancellationToken = default);

    Task<PromptCatalogPageDto> GetPromptsAsync(
        PromptCatalogFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns published+enabled details when publicly accessible.
    /// Throws <see cref="PromptLabException"/> with RenderRequiresAuthentication
    /// when the prompt exists and is published/enabled but requires authentication
    /// and <paramref name="currentUserId"/> is null.
    /// Returns null for unpublished/disabled/inactive-category prompts.
    /// </summary>
    Task<PromptDetailsDto?> GetBySlugAsync(
        string slug,
        Guid? currentUserId = null,
        CancellationToken cancellationToken = default);
}
