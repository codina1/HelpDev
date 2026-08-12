using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
using HelpDev.Modules.Content.Domain.Roadmaps;

namespace HelpDev.Modules.Content.Application.Roadmaps;

public interface IRoadmapRepository
{
    Task<RoadmapMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task<RoadmapMetadata?> GetByIdAsync(Guid roadmapId, CancellationToken cancellationToken = default);

    Task AddAsync(RoadmapMetadata metadata, CancellationToken cancellationToken = default);

    Task AddStepAsync(RoadmapStep step, CancellationToken cancellationToken = default);
}

public interface IRoadmapService
{
    Task<RoadmapDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<RoadmapDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default);

    Task<RoadmapDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default);

    Task<RoadmapStepDto> AddStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateRoadmapStepRequest request,
        CancellationToken cancellationToken = default);

    Task<RoadmapStepDto> UpdateStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        UpdateRoadmapStepRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        CancellationToken cancellationToken = default);

    Task ReorderStepsAsync(
        ContentManagementActor actor,
        Guid contentId,
        ReorderRoadmapStepsRequest request,
        CancellationToken cancellationToken = default);
}

public interface IRoadmapQueries
{
    Task<IReadOnlyList<RoadmapListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default);
}
