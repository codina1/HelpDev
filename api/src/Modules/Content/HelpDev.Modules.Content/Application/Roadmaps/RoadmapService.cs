using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Roadmaps.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Roadmaps;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.Roadmaps;

public sealed class RoadmapService : IRoadmapService
{
    private readonly IContentRepository _contentRepository;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public RoadmapService(
        IContentRepository contentRepository,
        IRoadmapRepository roadmapRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _roadmapRepository = roadmapRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<RoadmapDetailDto?> GetByContentIdAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        return roadmap is null ? null : RoadmapMapper.ToDetail(roadmap);
    }

    public async Task<RoadmapDetailDto> CreateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var existing = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            throw new ContentException("متادیتای نقشه راه قبلاً ایجاد شده است.", ContentErrorCodes.OperationInvalid);
        }

        try
        {
            var roadmap = RoadmapMetadata.Create(
                Guid.NewGuid(),
                content.Id,
                ParseLevel(request.Level),
                request.EstimatedDuration,
                request.Goal,
                request.Prerequisites,
                _clock.UtcNow);

            await _roadmapRepository.AddAsync(roadmap, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RoadmapMapper.ToDetail(roadmap);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<RoadmapDetailDto> UpdateAsync(
        ContentManagementActor actor,
        Guid contentId,
        UpdateRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (roadmap is null)
        {
            throw new ContentException("متادیتای نقشه راه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            roadmap.Update(
                ParseLevel(request.Level),
                request.EstimatedDuration,
                request.Goal,
                request.Prerequisites,
                _clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RoadmapMapper.ToDetail(roadmap);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<RoadmapStepDto> AddStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        CreateRoadmapStepRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (roadmap is null)
        {
            throw new ContentException("متادیتای نقشه راه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            var order = request.Order
                ?? (roadmap.Steps.Count == 0 ? 0 : roadmap.Steps.Max(s => s.Order) + 1);

            var step = RoadmapStep.Create(
                Guid.NewGuid(),
                roadmap.Id,
                request.Title,
                request.Description,
                order,
                request.EstimatedHours,
                request.ProjectTitle,
                request.ProjectDescription);

            ApplyTopics(step, request.Topics);
            ApplyResources(step, request.Resources);

            await _roadmapRepository.AddStepAsync(step, cancellationToken).ConfigureAwait(false);
            roadmap.Touch(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RoadmapMapper.ToStep(step);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<RoadmapStepDto> UpdateStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        UpdateRoadmapStepRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (roadmap is null)
        {
            throw new ContentException("متادیتای نقشه راه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            var step = roadmap.GetRequiredStep(stepId);
            step.Update(
                request.Title,
                request.Description,
                request.Order,
                request.EstimatedHours,
                request.ProjectTitle,
                request.ProjectDescription);

            if (request.Topics is not null)
            {
                ApplyTopics(step, request.Topics);
            }

            if (request.Resources is not null)
            {
                ApplyResources(step, request.Resources);
            }

            roadmap.Touch(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RoadmapMapper.ToStep(step);
        }
        catch (DomainException ex)
        {
            var code = ex.Message.Contains("یافت نشد", StringComparison.Ordinal)
                ? ContentErrorCodes.NotFound
                : ContentErrorCodes.Validation;
            throw new ContentException(ex.Message, code, ex);
        }
    }

    public async Task RemoveStepAsync(
        ContentManagementActor actor,
        Guid contentId,
        Guid stepId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (roadmap is null)
        {
            throw new ContentException("متادیتای نقشه راه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            roadmap.RemoveStep(stepId, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.NotFound, ex);
        }
    }

    public async Task ReorderStepsAsync(
        ContentManagementActor actor,
        Guid contentId,
        ReorderRoadmapStepsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);

        var content = await LoadRoadmapContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        if (roadmap is null)
        {
            throw new ContentException("متادیتای نقشه راه یافت نشد.", ContentErrorCodes.NotFound);
        }

        try
        {
            roadmap.ReorderSteps(request.StepIds, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    private async Task<Domain.Entities.Content> LoadRoadmapContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);

        if (content.Type != ContentType.Roadmap)
        {
            throw new ContentException("این محتوا از نوع نقشه راه نیست.", ContentErrorCodes.Validation);
        }

        return content;
    }

    private static void ApplyTopics(RoadmapStep step, IReadOnlyList<UpsertRoadmapTopicItem>? items)
    {
        var topics = (items ?? [])
            .Select(item => RoadmapTopic.Create(
                item.Id is { } id && id != Guid.Empty ? id : Guid.NewGuid(),
                step.Id,
                item.Title,
                item.Description,
                item.Order))
            .ToList();
        step.ReplaceTopics(topics);
    }

    private static void ApplyResources(RoadmapStep step, IReadOnlyList<UpsertRoadmapResourceItem>? items)
    {
        var resources = (items ?? [])
            .Select(item => RoadmapResource.Create(
                item.Id is { } id && id != Guid.Empty ? id : Guid.NewGuid(),
                step.Id,
                item.Title,
                item.Url,
                ParseResourceType(item.ResourceType),
                item.Order))
            .ToList();
        step.ReplaceResources(resources);
    }

    private static RoadmapLevel ParseLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out RoadmapLevel level)
            || !Enum.IsDefined(level))
        {
            throw new DomainException("سطح نقشه راه معتبر نیست.");
        }

        return level;
    }

    private static RoadmapResourceType ParseResourceType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value.Trim(), ignoreCase: true, out RoadmapResourceType type)
            || !Enum.IsDefined(type))
        {
            throw new DomainException("نوع منبع معتبر نیست.");
        }

        return type;
    }
}
