namespace HelpDev.Modules.Toolbox.Application.Tools;

public sealed record ToolDefinitionAdminDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    string Summary,
    string? Description,
    string Type,
    string InputSchema,
    string? ExampleInput,
    bool IsPublished,
    bool IsEnabled,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

public sealed record ToolDefinitionPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<ToolDefinitionAdminDto> Items);

public sealed record CreateToolDefinitionRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string Summary,
    string? Description,
    string Type,
    string InputSchema,
    string? ExampleInput,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder);

public sealed record UpdateToolDefinitionRequest(
    Guid CategoryId,
    string Name,
    string Summary,
    string? Description,
    bool RequiresAuthentication,
    bool AllowHistory,
    int DisplayOrder);

public sealed record UpdateToolSchemaRequest(
    string InputSchema,
    string? ExampleInput);

public sealed record ToolDefinitionFilter(
    Guid? CategoryId,
    string? Type,
    bool? IsPublished,
    bool? IsEnabled,
    int Page,
    int PageSize);

public interface IToolDefinitionQueries
{
    Task<ToolDefinitionAdminDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ToolDefinitionPageDto> GetPageAsync(
        ToolDefinitionFilter filter,
        CancellationToken cancellationToken = default);
}

public interface IToolDefinitionService
{
    Task<ToolDefinitionAdminDto> CreateDraftAsync(
        CreateToolDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> UpdateAsync(
        Guid id,
        UpdateToolDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> UpdateSchemaAsync(
        Guid id,
        UpdateToolSchemaRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> PublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> UnpublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> EnableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> DisableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionPageDto> GetPageAsync(
        ToolDefinitionFilter filter,
        CancellationToken cancellationToken = default);

    Task<ToolDefinitionAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
