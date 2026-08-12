using HelpDev.Modules.PromptLab.Application.Catalog;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed class PromptDefinitionService : IPromptDefinitionService
{
    private readonly IPromptDefinitionRepository _repository;
    private readonly IPromptCategoryRepository _categoryRepository;
    private readonly IPromptDefinitionQueries _queries;
    private readonly IPromptTemplateParser _templateParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IAuditRequestContext _auditRequestContext;
    private readonly ILogger<PromptDefinitionService> _logger;

    public PromptDefinitionService(
        IPromptDefinitionRepository repository,
        IPromptCategoryRepository categoryRepository,
        IPromptDefinitionQueries queries,
        IPromptTemplateParser templateParser,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAuditRecorder auditRecorder,
        IAuditRequestContext auditRequestContext,
        ILogger<PromptDefinitionService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _queries = queries;
        _templateParser = templateParser;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _auditRecorder = auditRecorder;
        _auditRequestContext = auditRequestContext;
        _logger = logger;
    }

    public async Task<PromptDefinitionAdminDto> CreateDraftAsync(
        CreatePromptDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var purpose = ParsePurpose(request.Purpose);
            var visibility = ParseVisibility(request.Visibility);
            var slug = PromptSlug.Create(
                request.Slug,
                PromptDefinition.SlugMaxLength,
                PromptLabApplicationErrorCodes.PromptSlugRequired,
                PromptLabApplicationErrorCodes.PromptSlugInvalid);

            if (await _repository.ExistsBySlugAsync(slug.Value, cancellationToken))
            {
                throw new PromptLabException(
                    "Prompt slug is already in use.",
                    PromptLabApplicationErrorCodes.PromptSlugDuplicate);
            }

            await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

            var prompt = PromptDefinition.CreateDraft(
                Guid.NewGuid(),
                request.CategoryId,
                request.Name,
                slug.Value,
                request.Summary,
                request.Description,
                purpose,
                visibility,
                request.RequiresAuthentication,
                request.AllowHistory,
                request.DisplayOrder,
                _clock.UtcNow);

            await _repository.AddAsync(prompt, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab prompt draft created. Operation={Operation} PromptId={PromptId} Slug={Slug} AdministratorId={AdministratorId}",
                "prompt_created",
                prompt.Id,
                prompt.Slug.Value,
                administratorId);

            await RecordPromptAuditAsync(
                AuditActions.PromptLabPromptCreated,
                prompt,
                previousState: "draft",
                versionNumber: null,
                administratorId,
                cancellationToken);

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptDefinitionAdminDto> UpdateMetadataAsync(
        Guid id,
        UpdatePromptDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatPromptState(prompt);
            var purpose = ParsePurpose(request.Purpose);
            var visibility = ParseVisibility(request.Visibility);

            var changed = false;
            if (prompt.CategoryId != request.CategoryId)
            {
                await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
                changed |= prompt.ChangeCategory(request.CategoryId, _clock.UtcNow);
            }

            changed |= prompt.UpdateMetadata(
                request.Name,
                request.Summary,
                request.Description,
                _clock.UtcNow);
            changed |= prompt.ChangePurpose(purpose, _clock.UtcNow);
            changed |= prompt.ChangeVisibility(visibility, _clock.UtcNow);
            changed |= prompt.ChangeAuthenticationRequirement(request.RequiresAuthentication, _clock.UtcNow);
            changed |= prompt.ChangeHistoryPolicy(request.AllowHistory, _clock.UtcNow);
            changed |= prompt.ChangeDisplayOrder(request.DisplayOrder, _clock.UtcNow);

            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab prompt updated. Operation={Operation} PromptId={PromptId} AdministratorId={AdministratorId}",
                    "prompt_updated",
                    prompt.Id,
                    administratorId);

                await RecordPromptAuditAsync(
                    AuditActions.PromptLabPromptUpdated,
                    prompt,
                    previousState,
                    versionNumber: null,
                    administratorId,
                    cancellationToken);
            }

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptDefinitionAdminDto> EnableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatPromptState(prompt);
            var changed = prompt.Enable(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab prompt enabled. Operation={Operation} PromptId={PromptId} AdministratorId={AdministratorId}",
                    "prompt_enabled",
                    prompt.Id,
                    administratorId);

                await RecordPromptAuditAsync(
                    AuditActions.PromptLabPromptEnabled,
                    prompt,
                    previousState,
                    versionNumber: null,
                    administratorId,
                    cancellationToken);
            }

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptDefinitionAdminDto> DisableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatPromptState(prompt);
            var changed = prompt.Disable(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab prompt disabled. Operation={Operation} PromptId={PromptId} AdministratorId={AdministratorId}",
                    "prompt_disabled",
                    prompt.Id,
                    administratorId);

                await RecordPromptAuditAsync(
                    AuditActions.PromptLabPromptDisabled,
                    prompt,
                    previousState,
                    versionNumber: null,
                    administratorId,
                    cancellationToken);
            }

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptVersionAdminDto> CreateVersionAsync(
        Guid id,
        CreatePromptVersionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Variables);

        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var placeholders = _templateParser.ExtractPlaceholders(request.Template);
            var versionId = Guid.NewGuid();

            var variables = new List<PromptVariable>(request.Variables.Count);
            foreach (var variableRequest in request.Variables)
            {
                var type = ParseVariableType(variableRequest.Type);
                variables.Add(PromptVariable.Create(
                    Guid.NewGuid(),
                    versionId,
                    variableRequest.Name,
                    variableRequest.Label,
                    variableRequest.Description,
                    type,
                    variableRequest.IsRequired,
                    variableRequest.DefaultValue,
                    variableRequest.MinLength,
                    variableRequest.MaxLength,
                    variableRequest.MinValue,
                    variableRequest.MaxValue,
                    variableRequest.ValidationPattern,
                    variableRequest.AllowedValues,
                    variableRequest.DisplayOrder));
            }

            var version = prompt.RegisterVersion(
                versionId,
                request.Template,
                request.ChangeNotes,
                administratorId,
                variables,
                placeholders,
                _clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "PromptLab version created. Operation={Operation} PromptId={PromptId} VersionId={VersionId} VersionNumber={VersionNumber} AdministratorId={AdministratorId}",
                "version_created",
                prompt.Id,
                version.Id,
                version.VersionNumber,
                administratorId);

            await RecordPromptAuditAsync(
                AuditActions.PromptLabVersionCreated,
                prompt,
                previousState: FormatPromptState(prompt),
                versionNumber: version.VersionNumber,
                administratorId,
                cancellationToken);

            return ToVersionDto(version);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptDefinitionAdminDto> PublishVersionAsync(
        Guid id,
        int versionNumber,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var category = await _categoryRepository.GetByIdAsync(prompt.CategoryId, cancellationToken);
            if (category is null)
            {
                throw new PromptLabException(
                    "Prompt category is invalid.",
                    PromptLabApplicationErrorCodes.PromptCategoryInvalid);
            }

            if (!category.IsActive)
            {
                throw new PromptLabException(
                    "Prompt category is inactive.",
                    PromptLabApplicationErrorCodes.CategoryInactive);
            }

            var previousState = FormatPromptState(prompt);
            var changed = prompt.PublishVersion(versionNumber, _clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab version published. Operation={Operation} PromptId={PromptId} VersionNumber={VersionNumber} AdministratorId={AdministratorId}",
                    "version_published",
                    prompt.Id,
                    versionNumber,
                    administratorId);

                await RecordPromptAuditAsync(
                    AuditActions.PromptLabVersionPublished,
                    prompt,
                    previousState,
                    versionNumber: versionNumber,
                    administratorId,
                    cancellationToken);
            }

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<PromptDefinitionAdminDto> UnpublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatPromptState(prompt);
            var changed = prompt.Unpublish(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "PromptLab prompt unpublished. Operation={Operation} PromptId={PromptId} AdministratorId={AdministratorId}",
                    "prompt_unpublished",
                    prompt.Id,
                    administratorId);

                await RecordPromptAuditAsync(
                    AuditActions.PromptLabPromptUnpublished,
                    prompt,
                    previousState,
                    versionNumber: null,
                    administratorId,
                    cancellationToken);
            }

            return ToAdminDto(prompt);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public Task<PromptDefinitionPageDto> GetPageAsync(
        PromptDefinitionFilter filter,
        CancellationToken cancellationToken = default) =>
        _queries.GetPageAsync(filter, cancellationToken);

    public async Task<PromptDefinitionAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<PromptVersionAdminDto>> GetVersionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _ = await GetByIdAsync(id, cancellationToken);
        return await _queries.GetVersionsAsync(id, cancellationToken);
    }

    public async Task<PromptVersionAdminDto> GetVersionAsync(
        Guid id,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        _ = await GetByIdAsync(id, cancellationToken);
        var version = await _queries.GetVersionAsync(id, versionNumber, cancellationToken);
        if (version is null)
        {
            throw new PromptLabException(
                "Prompt version was not found.",
                PromptLabApplicationErrorCodes.PromptVersionNotFound);
        }

        return version;
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new PromptLabException(
                "Prompt category is invalid.",
                PromptLabApplicationErrorCodes.PromptCategoryInvalid);
        }
    }

    private async Task<PromptDefinition> GetAggregateAsync(Guid id, CancellationToken cancellationToken)
    {
        var prompt = await _repository.GetByIdAsync(id, cancellationToken);
        if (prompt is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        return prompt;
    }

    private static PromptPurpose ParsePurpose(string purpose)
    {
        if (string.IsNullOrWhiteSpace(purpose)
            || !Enum.TryParse<PromptPurpose>(purpose.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new PromptLabException(
                "Prompt purpose is invalid.",
                PromptLabApplicationErrorCodes.PromptNameInvalid);
        }

        return parsed;
    }

    private static PromptVisibility ParseVisibility(string visibility)
    {
        if (string.IsNullOrWhiteSpace(visibility)
            || !Enum.TryParse<PromptVisibility>(visibility.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new PromptLabException(
                "Prompt visibility is invalid.",
                PromptLabApplicationErrorCodes.PromptNameInvalid);
        }

        return parsed;
    }

    private static PromptVariableType ParseVariableType(string type)
    {
        if (string.IsNullOrWhiteSpace(type)
            || !Enum.TryParse<PromptVariableType>(type.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new PromptLabException(
                "Variable type is invalid.",
                PromptLabApplicationErrorCodes.VariableTypeInvalid);
        }

        return parsed;
    }

    private async Task RecordPromptAuditAsync(
        string action,
        PromptDefinition prompt,
        string previousState,
        int? versionNumber,
        Guid? administratorId,
        CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, string>
        {
            ["promptId"] = prompt.Id.ToString(),
            ["promptSlug"] = prompt.Slug.Value,
            ["previousState"] = previousState,
            ["newState"] = FormatPromptState(prompt),
        };

        if (versionNumber.HasValue)
        {
            metadata["versionNumber"] = versionNumber.Value.ToString();
        }

        await _auditRecorder.RecordAsync(new AuditRecordInput(
            Category: AuditCategories.PromptManagement,
            Action: action,
            Outcome: AuditOutcomes.Success,
            ActorUserId: administratorId,
            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,
            SubjectId: prompt.Id,
            SubjectType: "PromptDefinition",
            SubjectDisplay: prompt.Slug.Value,
            CorrelationId: _auditRequestContext.CorrelationId,
            RequestMethod: _auditRequestContext.RequestMethod,
            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,
            Metadata: metadata), cancellationToken);
    }

    private static string FormatPromptState(PromptDefinition prompt)
    {
        if (!prompt.IsEnabled)
        {
            return "disabled";
        }

        return prompt.IsPublished ? "published" : "draft";
    }

    private static PromptDefinitionAdminDto ToAdminDto(PromptDefinition prompt) =>
        new(
            prompt.Id,
            prompt.CategoryId,
            prompt.Name,
            prompt.Slug.Value,
            prompt.Summary,
            prompt.Description,
            prompt.Purpose.ToString(),
            prompt.Visibility.ToString(),
            prompt.IsPublished,
            prompt.IsEnabled,
            prompt.RequiresAuthentication,
            prompt.AllowHistory,
            prompt.DisplayOrder,
            prompt.LatestVersionNumber,
            prompt.PublishedVersionNumber,
            prompt.CreatedAtUtc,
            prompt.UpdatedAtUtc,
            prompt.PublishedAtUtc);

    private static PromptVersionAdminDto ToVersionDto(PromptVersion version) =>
        new(
            version.Id,
            version.VersionNumber,
            version.Template,
            version.ChangeNotes,
            version.CreatedByUserId,
            version.CreatedAtUtc,
            version.Variables
                .OrderBy(variable => variable.DisplayOrder)
                .ThenBy(variable => variable.Name, StringComparer.OrdinalIgnoreCase)
                .Select(variable => new PromptVariableDto(
                    variable.Name,
                    variable.Label,
                    variable.Description,
                    variable.Type.ToString(),
                    variable.IsRequired,
                    variable.DefaultValue,
                    variable.MinLength,
                    variable.MaxLength,
                    variable.MinValue,
                    variable.MaxValue,
                    variable.ValidationPattern,
                    variable.AllowedValues.ToList(),
                    variable.DisplayOrder))
                .ToList());

    private static PromptLabException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.PromptNameInvalid, ex);
}
