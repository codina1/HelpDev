using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed class PromptWriterService : IPromptWriterService
{
    private readonly IPromptRepository _prompts;
    private readonly IPromptCategoryRepository _categories;
    private readonly IAiModelRepository _aiModels;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IAuditRequestContext _auditRequestContext;
    private readonly ILogger<PromptWriterService> _logger;

    public PromptWriterService(
        IPromptRepository prompts,
        IPromptCategoryRepository categories,
        IAiModelRepository aiModels,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAuditRecorder auditRecorder,
        IAuditRequestContext auditRequestContext,
        ILogger<PromptWriterService> logger)
    {
        _prompts = prompts;
        _categories = categories;
        _aiModels = aiModels;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _auditRecorder = auditRecorder;
        _auditRequestContext = auditRequestContext;
        _logger = logger;
    }

    public async Task<WriterPromptDetailsDto> CreateAsync(
        Guid authorId,
        CreateWriterPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAuthor(authorId);

        try
        {
            var mediaType = ParseMediaType(request.MediaType);
            var category = await GetActiveCategoryAsync(request.CategoryId, cancellationToken);
            var aiModel = await GetActiveAiModelAsync(request.AiModelId, cancellationToken);
            await EnsureSlugAvailableAsync(request.Slug, excludingId: null, cancellationToken);

            var prompt = Prompt.Create(
                Guid.NewGuid(),
                request.Title,
                request.Slug,
                request.Description,
                request.Content,
                request.CoverImage,
                mediaType,
                aiModel,
                category,
                authorId,
                _clock.UtcNow);

            await _prompts.AddAsync(prompt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab writer prompt created as draft. Operation={Operation} PromptId={PromptId} AuthorId={AuthorId} Status={Status}",
                "prompt_created",
                prompt.Id,
                authorId,
                prompt.Status);

            await RecordAuditAsync(
                AuditActions.PromptLabPromptCreated,
                prompt,
                previousState: PromptStatus.Draft.ToString(),
                authorId,
                cancellationToken);

            return ToDetails(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<WriterPromptDetailsDto> UpdateAsync(
        Guid authorId,
        Guid id,
        UpdateWriterPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAuthor(authorId);

        try
        {
            var prompt = await GetOwnedAsync(authorId, id, cancellationToken);
            var previousState = prompt.Status.ToString();
            var mediaType = ParseMediaType(request.MediaType);

            if (!string.Equals(prompt.Slug.Value, request.Slug.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                await EnsureSlugAvailableAsync(request.Slug, prompt.Id, cancellationToken);
            }

            var changed = prompt.Update(
                authorId,
                request.Title,
                request.Slug,
                request.Description,
                request.Content,
                request.CoverImage,
                mediaType,
                _clock.UtcNow);

            if (prompt.CategoryId != request.CategoryId)
            {
                var category = await GetActiveCategoryAsync(request.CategoryId, cancellationToken);
                changed |= prompt.ChangeCategory(authorId, category, _clock.UtcNow);
            }

            if (prompt.AiModelId != request.AiModelId)
            {
                var aiModel = await GetActiveAiModelAsync(request.AiModelId, cancellationToken);
                changed |= prompt.ChangeAiModel(authorId, aiModel, _clock.UtcNow);
            }

            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab writer prompt updated. Operation={Operation} PromptId={PromptId} AuthorId={AuthorId} Status={Status}",
                    "prompt_updated",
                    prompt.Id,
                    authorId,
                    prompt.Status);

                await RecordAuditAsync(
                    AuditActions.PromptLabPromptUpdated,
                    prompt,
                    previousState,
                    authorId,
                    cancellationToken);
            }

            return ToDetails(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<WriterPromptDetailsDto> SubmitAsync(
        Guid authorId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        EnsureAuthor(authorId);

        try
        {
            var prompt = await GetOwnedAsync(authorId, id, cancellationToken);
            var previousState = prompt.Status.ToString();

            prompt.Submit(authorId, _clock.UtcNow);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab writer prompt submitted. Operation={Operation} PromptId={PromptId} AuthorId={AuthorId} Status={Status}",
                "prompt_submitted",
                prompt.Id,
                authorId,
                prompt.Status);

            await RecordAuditAsync(
                AuditActions.PromptLabPromptUpdated,
                prompt,
                previousState,
                authorId,
                cancellationToken);

            return ToDetails(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    private async Task<Prompt> GetOwnedAsync(Guid authorId, Guid id, CancellationToken cancellationToken)
    {
        var prompt = await _prompts.GetByIdAsync(id, cancellationToken);
        if (prompt is null || prompt.AuthorId != authorId)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return prompt;
    }

    private async Task<PromptCategory> GetActiveCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new PromptLabException(
                "Prompt category was not found.",
                PromptLabApplicationErrorCodes.CategoryNotFound);
        }

        category.EnsureActive();
        return category;
    }

    private async Task<AiModel> GetActiveAiModelAsync(Guid aiModelId, CancellationToken cancellationToken)
    {
        var aiModel = await _aiModels.GetByIdAsync(aiModelId, cancellationToken);
        if (aiModel is null)
        {
            throw new PromptLabException(
                "AI model was not found.",
                PromptLabApplicationErrorCodes.AiModelNotFound);
        }

        aiModel.EnsureActive();
        return aiModel;
    }

    private async Task EnsureSlugAvailableAsync(
        string? slug,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var normalized = PromptSlug.Create(
            slug,
            Prompt.SlugMaxLength,
            PromptLabApplicationErrorCodes.PromptSlugRequired,
            PromptLabApplicationErrorCodes.PromptSlugInvalid);

        if (await _prompts.ExistsBySlugAsync(normalized.Value, excludingId, cancellationToken))
        {
            throw new PromptLabException(
                "Prompt slug is already in use.",
                PromptLabApplicationErrorCodes.PromptSlugDuplicate);
        }
    }

    private static void EnsureAuthor(Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new PromptLabException(
                "Author id is required.",
                PromptLabApplicationErrorCodes.PromptAuthorInvalid);
        }
    }

    private static PromptMediaType ParseMediaType(string mediaType)
    {
        if (!PublicPromptMediaTypes.TryParse(mediaType, out var parsed))
        {
            throw new PromptLabException(
                "Media type is invalid.",
                PromptLabApplicationErrorCodes.PromptMediaTypeInvalid);
        }

        return parsed;
    }

    private async Task RecordAuditAsync(
        string action,
        Prompt prompt,
        string previousState,
        Guid authorId,
        CancellationToken cancellationToken)
    {
        await _auditRecorder.RecordAsync(
            new AuditRecordInput(
                Category: AuditCategories.PromptManagement,
                Action: action,
                Outcome: AuditOutcomes.Success,
                ActorUserId: authorId,
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

    internal static WriterPromptDetailsDto ToDetails(Prompt prompt) =>
        new(
            prompt.Id,
            prompt.Title,
            prompt.Slug.Value,
            prompt.Description,
            prompt.Content,
            prompt.CoverImage,
            prompt.MediaType.ToString(),
            prompt.CategoryId,
            prompt.AiModelId,
            prompt.Status.ToString(),
            prompt.Views,
            prompt.CopyCount,
            prompt.CreatedAt,
            prompt.UpdatedAt,
            prompt.PublishedAt);

    private static PromptLabException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.PromptTitleInvalid, ex);
}
