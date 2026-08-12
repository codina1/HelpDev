namespace HelpDev.Modules.PromptLab.Application.Categories;

public sealed record PromptCategoryAdminDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreatePromptCategoryRequest(
    string Name,
    string Slug,
    string? Description,
    string? Icon,
    int DisplayOrder);

public sealed record UpdatePromptCategoryRequest(
    string Name,
    string? Description,
    string? Icon,
    int DisplayOrder);

public interface IPromptCategoryQueries
{
    Task<IReadOnlyList<PromptCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PromptCategoryAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IPromptCategoryService
{
    Task<PromptCategoryAdminDto> CreateAsync(
        CreatePromptCategoryRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptCategoryAdminDto> UpdateAsync(
        Guid id,
        UpdatePromptCategoryRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptCategoryAdminDto> ActivateAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptCategoryAdminDto> DeactivateAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PromptCategoryAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
