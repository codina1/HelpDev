using HelpDev.Modules.PromptLab.Application.Catalog;

namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed record PromptDefinitionAdminDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    string Summary,
    string? Description,
    string Purpose,
    string Visibility,
    bool IsPublished,
    bool IsEnabled,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder,
    int LatestVersionNumber,
    int? PublishedVersionNumber,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

public sealed record PromptDefinitionPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<PromptDefinitionAdminDto> Items);

public sealed record PromptVersionAdminDto(
    Guid Id,
    int VersionNumber,
    string Template,
    string? ChangeNotes,
    Guid? CreatedByUserId,
    DateTime CreatedAtUtc,
    IReadOnlyList<PromptVariableDto> Variables);

public sealed record CreatePromptDefinitionRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string Summary,
    string? Description,
    string Purpose,
    string Visibility,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder);

public sealed record UpdatePromptDefinitionRequest(
    Guid CategoryId,
    string Name,
    string Summary,
    string? Description,
    string Purpose,
    string Visibility,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder);

public sealed record CreatePromptVariableRequest(
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
    IReadOnlyList<string>? AllowedValues,
    int DisplayOrder);

public sealed record CreatePromptVersionRequest(
    string Template,
    string? ChangeNotes,
    IReadOnlyList<CreatePromptVariableRequest> Variables);

public sealed record PromptDefinitionFilter(
    Guid? CategoryId,
    string? Purpose,
    string? Visibility,
    bool? IsPublished,
    bool? IsEnabled,
    int Page,
    int PageSize);

public interface IPromptDefinitionQueries
{
    Task<PromptDefinitionAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromptDefinitionPageDto> GetPageAsync(
        PromptDefinitionFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptVersionAdminDto>> GetVersionsAsync(
        Guid promptId,
        CancellationToken cancellationToken = default);

    Task<PromptVersionAdminDto?> GetVersionAsync(
        Guid promptId,
        int versionNumber,
        CancellationToken cancellationToken = default);
}

public interface IPromptDefinitionService
{
    Task<PromptDefinitionAdminDto> CreateDraftAsync(
        CreatePromptDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> UpdateMetadataAsync(
        Guid id,
        UpdatePromptDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> EnableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> DisableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptVersionAdminDto> CreateVersionAsync(
        Guid id,
        CreatePromptVersionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> PublishVersionAsync(
        Guid id,
        int versionNumber,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> UnpublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionPageDto> GetPageAsync(
        PromptDefinitionFilter filter,
        CancellationToken cancellationToken = default);

    Task<PromptDefinitionAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromptVersionAdminDto>> GetVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PromptVersionAdminDto> GetVersionAsync(
        Guid id,
        int versionNumber,
        CancellationToken cancellationToken = default);
}
