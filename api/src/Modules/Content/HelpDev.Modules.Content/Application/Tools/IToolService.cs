using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Tools.Dtos;

namespace HelpDev.Modules.Content.Application.Tools;

public interface IToolService
{
    Task<ToolDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ToolDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default);

    Task<ToolDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default);

    Task<ToolFeatureDto> AddFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateToolFeatureRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid featureId,
        CancellationToken cancellationToken = default);
}

public interface IToolQueries
{
    Task<IReadOnlyList<ToolListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default);
}
