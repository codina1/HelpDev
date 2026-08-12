using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Roadmaps;
using HelpDev.Modules.Content.Application.Roadmaps.Ai;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeRoadmapService : IRoadmapService
{
    public RoadmapDetailDto? RoadmapToReturn { get; set; }

    public string? LastOperation { get; private set; }

    public Guid? LastContentId { get; private set; }

    public UpdateRoadmapRequest? LastRequest { get; private set; }

    public Task<RoadmapDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        LastContentId = contentId;
        LastOperation = nameof(GetByContentIdAsync);
        return Task.FromResult(RoadmapToReturn);
    }

    public Task<RoadmapDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(CreateAsync);
        RoadmapToReturn = Sample(contentId, request);
        return Task.FromResult(RoadmapToReturn);
    }

    public Task<RoadmapDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(UpdateAsync);
        RoadmapToReturn = Sample(contentId, request);
        return Task.FromResult(RoadmapToReturn!);
    }

    public Task<RoadmapStepDto> AddStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateRoadmapStepRequest request,
        CancellationToken cancellationToken = default)
    {
        LastOperation = nameof(AddStepAsync);
        return Task.FromResult(new RoadmapStepDto(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Order ?? 0,
            request.EstimatedHours,
            request.ProjectTitle,
            request.ProjectDescription,
            [],
            []));
    }

    public Task<RoadmapStepDto> UpdateStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        UpdateRoadmapStepRequest request,
        CancellationToken cancellationToken = default)
    {
        LastOperation = nameof(UpdateStepAsync);
        return Task.FromResult(new RoadmapStepDto(
            stepId,
            request.Title,
            request.Description,
            request.Order,
            request.EstimatedHours,
            request.ProjectTitle,
            request.ProjectDescription,
            [],
            []));
    }

    public Task RemoveStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        CancellationToken cancellationToken = default)
    {
        LastOperation = nameof(RemoveStepAsync);
        return Task.CompletedTask;
    }

    public Task ReorderStepsAsync(
        ContentManagementActor actor,
        Guid contentId,
        ReorderRoadmapStepsRequest request,
        CancellationToken cancellationToken = default)
    {
        LastOperation = nameof(ReorderStepsAsync);
        return Task.CompletedTask;
    }

    private static RoadmapDetailDto Sample(Guid contentId, UpdateRoadmapRequest request) =>
        new(
            Guid.NewGuid(),
            contentId,
            request.Level,
            request.EstimatedDuration,
            request.Goal,
            request.Prerequisites,
            [],
            DateTime.UtcNow,
            DateTime.UtcNow);
}

internal sealed class FakeRoadmapAiAssistantService : IRoadmapAiAssistantService
{
    public Task<RoadmapAiSuggestionDto> SuggestOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new RoadmapAiSuggestionDto("outline", "t", "b", [], true));

    public Task<RoadmapAiSuggestionDto> SuggestPhasesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new RoadmapAiSuggestionDto("phases", "t", "b", [], true));

    public Task<RoadmapAiSuggestionDto> SuggestTopicsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new RoadmapAiSuggestionDto("topics", "t", "b", [], true));
}
