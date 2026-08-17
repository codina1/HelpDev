using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed class PromptAdminReviewService : IPromptAdminReviewService
{
    private readonly IPromptRepository _prompts;
    private readonly IPromptAdminReviewQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IAuditRequestContext _auditRequestContext;
    private readonly ILogger<PromptAdminReviewService> _logger;

    public PromptAdminReviewService(
        IPromptRepository prompts,
        IPromptAdminReviewQueries queries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAuditRecorder auditRecorder,
        IAuditRequestContext auditRequestContext,
        ILogger<PromptAdminReviewService> logger)
    {
        _prompts = prompts;
        _queries = queries;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _auditRecorder = auditRecorder;
        _auditRequestContext = auditRequestContext;
        _logger = logger;
    }

    public async Task<AdminPromptReviewDetailsDto> ApproveAsync(
        Guid actorUserId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnsureActor(actorUserId);

        try
        {
            var prompt = await GetRequiredAsync(id, cancellationToken);
            var previousState = prompt.Status.ToString();
            prompt.Approve(_clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab admin approved prompt. Operation={Operation} PromptId={PromptId} ActorUserId={ActorUserId} Status={Status}",
                "prompt_approved",
                prompt.Id,
                actorUserId,
                prompt.Status);

            await RecordAuditAsync(
                prompt,
                previousState,
                actorUserId,
                cancellationToken);

            return await GetRequiredDtoAsync(prompt.Id, cancellationToken);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<AdminPromptReviewDetailsDto> RejectAsync(
        Guid actorUserId,
        Guid id,
        RejectAdminPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureActor(actorUserId);
        var reason = NormalizeReason(request.Reason);

        try
        {
            var prompt = await GetRequiredAsync(id, cancellationToken);
            var previousState = prompt.Status.ToString();
            prompt.Reject(_clock.UtcNow, reason);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab admin rejected prompt. Operation={Operation} PromptId={PromptId} ActorUserId={ActorUserId} Status={Status}",
                "prompt_rejected",
                prompt.Id,
                actorUserId,
                prompt.Status);

            await RecordAuditAsync(
                prompt,
                previousState,
                actorUserId,
                cancellationToken);

            return await GetRequiredDtoAsync(prompt.Id, cancellationToken);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    private async Task<Prompt> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        var prompt = await _prompts.GetByIdAsync(id, cancellationToken);
        if (prompt is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return prompt;
    }

    private async Task<AdminPromptReviewDetailsDto> GetRequiredDtoAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dto = await _queries.GetByIdAsync(id, cancellationToken);
        if (dto is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return dto;
    }

    private static void EnsureActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
        {
            throw new PromptLabException(
                "Authentication is required.",
                PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication);
        }
    }

    private static string NormalizeReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new PromptLabException(
                "Rejection reason is required.",
                PromptLabApplicationErrorCodes.PromptRejectionReasonRequired);
        }

        var trimmed = reason.Trim();
        if (trimmed.Length > PromptLabLimits.MaxPromptRejectionReasonLength)
        {
            throw new PromptLabException(
                "Rejection reason is invalid.",
                PromptLabApplicationErrorCodes.PromptRejectionReasonInvalid);
        }

        return trimmed;
    }

    private async Task RecordAuditAsync(
        Prompt prompt,
        string previousState,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        await _auditRecorder.RecordAsync(
            new AuditRecordInput(
                Category: AuditCategories.PromptManagement,
                Action: AuditActions.PromptLabPromptUpdated,
                Outcome: AuditOutcomes.Success,
                ActorUserId: actorUserId,
                ActorType: AuditActorTypes.User,
                SubjectId: prompt.Id,
                SubjectType: "Prompt",
                SubjectDisplay: prompt.Slug.Value,
                CorrelationId: _auditRequestContext.CorrelationId,
                RequestMethod: _auditRequestContext.RequestMethod,
                RequestPathTemplate: _auditRequestContext.RequestPathTemplate,
                Metadata: new Dictionary<string, string>
                {
                    ["promptId"] = prompt.Id.ToString(),
                    ["promptSlug"] = prompt.Slug.Value,
                    ["previousState"] = previousState,
                    ["newState"] = prompt.Status.ToString(),
                }),
            cancellationToken);
    }

    private static PromptLabException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.PromptStatusInvalid, ex);
}
