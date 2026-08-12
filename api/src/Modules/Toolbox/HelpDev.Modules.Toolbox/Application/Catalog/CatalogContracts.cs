namespace HelpDev.Modules.Toolbox.Application.Catalog;

public sealed record ToolCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder);

public sealed record ToolCatalogItemDto(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string Type,
    string CategorySlug,
    string CategoryName,
    bool RequiresAuthentication,
    int DisplayOrder);

public sealed record ToolDetailsCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon);

public sealed record ToolDetailsDto(
    Guid Id,
    string Slug,
    string Name,
    string Summary,
    string? Description,
    string Type,
    string InputSchema,
    string? ExampleInput,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder,
    ToolDetailsCategoryDto Category);

public sealed record ToolCatalogFilter(
    string? CategorySlug,
    string? Search,
    int Page,
    int PageSize);

public static class ToolboxPaging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public sealed record ToolCatalogPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<ToolCatalogItemDto> Items);

public interface IToolCatalogQueries
{
    Task<IReadOnlyList<ToolCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<ToolCatalogPageDto> GetToolsAsync(
        ToolCatalogFilter filter,
        CancellationToken cancellationToken = default);

    Task<ToolDetailsDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
