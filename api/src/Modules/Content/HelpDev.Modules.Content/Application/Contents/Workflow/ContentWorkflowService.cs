using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Entities;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using ContentEntity = HelpDev.Modules.Content.Domain.Entities.Content;

namespace HelpDev.Modules.Content.Application.Contents.Workflow;

public sealed class ContentWorkflowService : IContentWorkflowService
{
    private readonly IContentRepository _contentRepository;
    private readonly IContentWorkflowTransitionRepository _transitionRepository;
    private readonly IContentWorkflowQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ContentWorkflowService(
        IContentRepository contentRepository,
        IContentWorkflowTransitionRepository transitionRepository,
        IContentWorkflowQueries queries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _transitionRepository = transitionRepository;
        _queries = queries;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public Task<WorkflowHistoryDto> GetWorkflowHistoryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        _queries.GetHistoryAsync(actor, contentId, cancellationToken);

    public async Task<AdminContentDetailDto> SubmitForReviewAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var content = await GetManagedContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        return await ApplyTransitionAsync(
                content,
                c => c.SubmitForReview(actor.UserId, _clock.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AdminContentDetailDto> ApproveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var content = await GetManagedContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        EnsureModerator(actor);
        return await ApplyTransitionAsync(
                content,
                c => c.Approve(actor.UserId, _clock.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AdminContentDetailDto> RejectAsync(
        ContentManagementActor actor,
        Guid contentId,
        RejectContentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var content = await GetManagedContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        EnsureModerator(actor);

        try
        {
            return await ApplyTransitionAsync(
                    content,
                    c => c.Reject(request.Comment, actor.UserId, _clock.UtcNow),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.Validation, ex);
        }
    }

    public async Task<AdminContentDetailDto> PublishAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var content = await GetManagedContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        EnsureAdmin(actor);

        if (content.Status == Domain.Enums.ContentStatus.Published)
        {
            return ContentServiceMap.ToAdminDetail(content);
        }

        return await ApplyTransitionAsync(
                content,
                c => c.Publish(actor.UserId, _clock.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<AdminContentDetailDto> ArchiveAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var content = await GetManagedContentAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        EnsureAdmin(actor);
        return await ApplyTransitionAsync(
                content,
                c => c.Archive(actor.UserId, _clock.UtcNow),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task BootstrapPublishAfterCreateAsync(
        Guid contentId,
        Guid authorId,
        CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        var utc = _clock.UtcNow;
        var transitions = new List<ContentWorkflowTransition>
        {
            content.SubmitForReview(authorId, utc),
            content.Approve(authorId, utc),
            content.Publish(authorId, utc),
        };

        await _transitionRepository.AddRangeAsync(transitions, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<AdminContentDetailDto> ApplyTransitionAsync(
        ContentEntity content,
        Func<ContentEntity, ContentWorkflowTransition> transitionFactory,
        CancellationToken cancellationToken)
    {
        try
        {
            var transition = transitionFactory(content);
            await _transitionRepository.AddAsync(transition, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return ContentServiceMap.ToAdminDetail(content);
        }
        catch (DomainException ex)
        {
            throw new ContentException(ex.Message, ContentErrorCodes.OperationInvalid, ex);
        }
    }

    private async Task<ContentEntity> GetManagedContentAsync(
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
        return content;
    }

    private static void EnsureModerator(ContentManagementActor actor)
    {
        if (!actor.CanManageAllContent)
        {
            throw new ContentException("مجوز کافی برای این عملیات وجود ندارد.", ContentErrorCodes.OperationInvalid);
        }
    }

    private static void EnsureAdmin(ContentManagementActor actor) => EnsureModerator(actor);
}
