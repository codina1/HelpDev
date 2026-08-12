using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Toolbox.Application.Tools;

public sealed class ToolDefinitionService : IToolDefinitionService
{
    private readonly IToolDefinitionRepository _repository;
    private readonly IToolCategoryRepository _categoryRepository;
    private readonly IToolDefinitionQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IAuditRequestContext _auditRequestContext;
    private readonly ILogger<ToolDefinitionService> _logger;

    public ToolDefinitionService(
        IToolDefinitionRepository repository,
        IToolCategoryRepository categoryRepository,
        IToolDefinitionQueries queries,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAuditRecorder auditRecorder,
        IAuditRequestContext auditRequestContext,
        ILogger<ToolDefinitionService> logger)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _queries = queries;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _auditRecorder = auditRecorder;
        _auditRequestContext = auditRequestContext;
        _logger = logger;
    }

    public async Task<ToolDefinitionAdminDto> CreateDraftAsync(
        CreateToolDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var type = ParseToolType(request.Type);
            var slug = ToolSlug.Create(
                request.Slug,
                ToolDefinition.SlugMaxLength,
                ToolboxApplicationErrorCodes.ToolSlugRequired,
                ToolboxApplicationErrorCodes.ToolSlugInvalid);

            if (await _repository.ExistsBySlugAsync(slug.Value, cancellationToken))
            {
                throw new ToolboxException(
                    "Tool slug is already in use.",
                    ToolboxApplicationErrorCodes.ToolSlugDuplicate);
            }

            await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

            var tool = ToolDefinition.CreateDraft(
                Guid.NewGuid(),
                request.CategoryId,
                request.Name,
                slug.Value,
                request.Summary,
                request.Description,
                type,
                request.InputSchema,
                request.ExampleInput,
                request.RequiresAuthentication,
                request.AllowHistory,
                request.DisplayOrder,
                _clock.UtcNow);

            await _repository.AddAsync(tool, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Toolbox tool draft created. Operation={Operation} ToolId={ToolId} Slug={Slug} AdministratorId={AdministratorId}",
                "tool_created",
                tool.Id,
                tool.Slug.Value,
                administratorId);

            await RecordToolAuditAsync(
                AuditActions.ToolboxToolCreated,
                tool,
                previousState: "draft",
                administratorId,
                cancellationToken);

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> UpdateAsync(
        Guid id,
        UpdateToolDefinitionRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatToolState(tool);

            var changed = false;
            if (tool.CategoryId != request.CategoryId)
            {
                await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
                changed |= tool.ChangeCategory(request.CategoryId, _clock.UtcNow);
            }

            changed |= tool.UpdateDetails(
                request.Name,
                request.Summary,
                request.Description,
                request.RequiresAuthentication,
                request.AllowHistory,
                request.DisplayOrder,
                _clock.UtcNow);

            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool updated. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_updated",
                    tool.Id,
                    administratorId);

                await RecordToolAuditAsync(
                    AuditActions.ToolboxToolUpdated,
                    tool,
                    previousState,
                    administratorId,
                    cancellationToken);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> UpdateSchemaAsync(
        Guid id,
        UpdateToolSchemaRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var changed = tool.UpdateInputSchema(request.InputSchema, request.ExampleInput, _clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool schema updated. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_schema_updated",
                    tool.Id,
                    administratorId);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> PublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var category = await _categoryRepository.GetByIdAsync(tool.CategoryId, cancellationToken);
            if (category is null)
            {
                throw new ToolboxException(
                    "Tool category is invalid.",
                    ToolboxApplicationErrorCodes.ToolCategoryInvalid);
            }

            if (!category.IsActive)
            {
                throw new ToolboxException(
                    "Tool category is inactive.",
                    ToolboxApplicationErrorCodes.CategoryInactive);
            }

            var previousState = FormatToolState(tool);
            var changed = tool.Publish(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool published. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_published",
                    tool.Id,
                    administratorId);

                await RecordToolAuditAsync(
                    AuditActions.ToolboxToolPublished,
                    tool,
                    previousState,
                    administratorId,
                    cancellationToken);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> UnpublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatToolState(tool);
            var changed = tool.Unpublish(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool unpublished. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_unpublished",
                    tool.Id,
                    administratorId);

                await RecordToolAuditAsync(
                    AuditActions.ToolboxToolUnpublished,
                    tool,
                    previousState,
                    administratorId,
                    cancellationToken);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> EnableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatToolState(tool);
            var changed = tool.Enable(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool enabled. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_enabled",
                    tool.Id,
                    administratorId);

                await RecordToolAuditAsync(
                    AuditActions.ToolboxToolEnabled,
                    tool,
                    previousState,
                    administratorId,
                    cancellationToken);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public async Task<ToolDefinitionAdminDto> DisableAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tool = await GetAggregateAsync(id, cancellationToken);
            var previousState = FormatToolState(tool);
            var changed = tool.Disable(_clock.UtcNow);
            if (changed)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Toolbox tool disabled. Operation={Operation} ToolId={ToolId} AdministratorId={AdministratorId}",
                    "tool_disabled",
                    tool.Id,
                    administratorId);

                await RecordToolAuditAsync(
                    AuditActions.ToolboxToolDisabled,
                    tool,
                    previousState,
                    administratorId,
                    cancellationToken);
            }

            return ToDto(tool);
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    public Task<ToolDefinitionPageDto> GetPageAsync(
        ToolDefinitionFilter filter,
        CancellationToken cancellationToken = default) =>
        _queries.GetPageAsync(filter, cancellationToken);

    public async Task<ToolDefinitionAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _queries.GetByIdAsync(id, cancellationToken);
        if (dto is null)
        {
            throw new ToolboxException(
                "Tool was not found.",
                ToolboxApplicationErrorCodes.ToolNotFound);
        }

        return dto;
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            throw new ToolboxException(
                "Tool category is invalid.",
                ToolboxApplicationErrorCodes.ToolCategoryInvalid);
        }
    }

    private async Task<ToolDefinition> GetAggregateAsync(Guid id, CancellationToken cancellationToken)
    {
        var tool = await _repository.GetByIdAsync(id, cancellationToken);
        if (tool is null)
        {
            throw new ToolboxException(
                "Tool was not found.",
                ToolboxApplicationErrorCodes.ToolNotFound);
        }

        return tool;
    }

    private static ToolType ParseToolType(string type)
    {
        if (string.IsNullOrWhiteSpace(type)
            || !Enum.TryParse<ToolType>(type.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new ToolboxException(
                "Tool type is invalid.",
                ToolboxApplicationErrorCodes.ToolTypeInvalid);
        }

        return parsed;
    }

    private async Task RecordToolAuditAsync(
        string action,
        ToolDefinition tool,
        string previousState,
        Guid? administratorId,
        CancellationToken cancellationToken)
    {
        await _auditRecorder.RecordAsync(new AuditRecordInput(
            Category: AuditCategories.ToolboxManagement,
            Action: action,
            Outcome: AuditOutcomes.Success,
            ActorUserId: administratorId,
            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,
            SubjectId: tool.Id,
            SubjectType: "ToolDefinition",
            SubjectDisplay: tool.Slug.Value,
            CorrelationId: _auditRequestContext.CorrelationId,
            RequestMethod: _auditRequestContext.RequestMethod,
            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,
            Metadata: new Dictionary<string, string>
            {
                ["toolId"] = tool.Id.ToString(),
                ["toolSlug"] = tool.Slug.Value,
                ["previousState"] = previousState,
                ["newState"] = FormatToolState(tool),
            }), cancellationToken);
    }

    private static string FormatToolState(ToolDefinition tool)
    {
        if (!tool.IsEnabled)
        {
            return "disabled";
        }

        return tool.IsPublished ? "published" : "draft";
    }

    private static ToolDefinitionAdminDto ToDto(ToolDefinition tool) =>
        new(
            tool.Id,
            tool.CategoryId,
            tool.Name,
            tool.Slug.Value,
            tool.Summary,
            tool.Description,
            tool.Type.ToString(),
            tool.InputSchema,
            tool.ExampleInput,
            tool.IsPublished,
            tool.IsEnabled,
            tool.RequiresAuthentication,
            tool.AllowHistory,
            tool.DisplayOrder,
            tool.CreatedAtUtc,
            tool.UpdatedAtUtc,
            tool.PublishedAtUtc);

    private static ToolboxException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? ToolboxApplicationErrorCodes.ToolNameInvalid, ex);
}
