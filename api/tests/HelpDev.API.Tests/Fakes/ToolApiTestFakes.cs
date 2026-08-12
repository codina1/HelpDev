using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Application.Tools.Dtos;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeToolService : IToolService
{
    public ToolDetailDto? ToolToReturn { get; set; }

    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public UpdateToolRequest? LastRequest { get; private set; }

    public string? LastOperation { get; private set; }

    public Task<ToolDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = nameof(GetByContentIdAsync);
        return Task.FromResult(ToolToReturn);
    }

    public Task<ToolDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(CreateAsync);
        ToolToReturn = Sample(contentId, request);
        return Task.FromResult(ToolToReturn);
    }

    public Task<ToolDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateToolRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastRequest = request;
        LastOperation = nameof(UpdateAsync);
        ToolToReturn = Sample(contentId, request, ToolToReturn?.Id ?? Guid.NewGuid());
        return Task.FromResult(ToolToReturn);
    }

    public Task<ToolFeatureDto> AddFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateToolFeatureRequest request,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = nameof(AddFeatureAsync);
        return Task.FromResult(new ToolFeatureDto(Guid.NewGuid(), request.Title, request.Description, request.Order ?? 0));
    }

    public Task RemoveFeatureAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid featureId,
        CancellationToken cancellationToken = default)
    {
        LastActor = actor;
        LastContentId = contentId;
        LastOperation = nameof(RemoveFeatureAsync);
        return Task.CompletedTask;
    }

    private static ToolDetailDto Sample(Guid contentId, UpdateToolRequest request, Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            contentId,
            request.ToolName,
            request.OfficialWebsiteUrl,
            request.GithubUrl,
            request.LogoMediaId,
            request.CompanyName,
            request.PricingModel,
            request.ToolCategory,
            request.Platforms,
            request.LicenseType,
            [],
            [],
            DateTime.UtcNow,
            DateTime.UtcNow);
}
