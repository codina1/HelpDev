namespace HelpDev.Modules.Toolbox.Application.Categories;

public sealed record ToolCategoryAdminDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateToolCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder);

public sealed record UpdateToolCategoryRequest(
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder);

public interface IToolCategoryQueries
{
    Task<IReadOnlyList<ToolCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ToolCategoryAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IToolCategoryService
{
    Task<ToolCategoryAdminDto> CreateAsync(
        CreateToolCategoryRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolCategoryAdminDto> UpdateAsync(
        Guid id,
        UpdateToolCategoryRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolCategoryAdminDto> ActivateAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolCategoryAdminDto> DeactivateAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ToolCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ToolCategoryAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
