using HelpDev.Modules.Toolbox.Application.Execution;

using HelpDev.Modules.Toolbox.Application.Persistence;

using HelpDev.Modules.Toolbox.Domain.Categories;

using HelpDev.Modules.Toolbox.Domain.Tools;

using HelpDev.SharedApplication.Abstractions.Persistence;

using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedKernel.Exceptions;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;



namespace HelpDev.Modules.Toolbox.Application.Categories;



public sealed class ToolCategoryService : IToolCategoryService

{

    private readonly IToolCategoryRepository _repository;

    private readonly IToolCategoryQueries _queries;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _clock;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<ToolCategoryService> _logger;



    public ToolCategoryService(

        IToolCategoryRepository repository,

        IToolCategoryQueries queries,

        IUnitOfWork unitOfWork,

        IDateTimeProvider clock,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<ToolCategoryService> logger)

    {

        _repository = repository;

        _queries = queries;

        _unitOfWork = unitOfWork;

        _clock = clock;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public async Task<ToolCategoryAdminDto> CreateAsync(

        CreateToolCategoryRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var slug = ToolSlug.Create(

                request.Slug,

                ToolCategory.SlugMaxLength,

                ToolboxApplicationErrorCodes.CategorySlugRequired,

                ToolboxApplicationErrorCodes.CategorySlugInvalid);



            if (await _repository.ExistsBySlugAsync(slug.Value, cancellationToken))

            {

                throw new ToolboxException(

                    "Category slug is already in use.",

                    ToolboxApplicationErrorCodes.CategorySlugDuplicate);

            }



            var category = ToolCategory.Create(

                Guid.NewGuid(),

                request.Name,

                slug.Value,

                request.Description,

                request.Icon,

                request.DisplayOrder,

                _clock.UtcNow);



            await _repository.AddAsync(category, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);



            _logger.LogInformation(

                "Toolbox category created. Operation={Operation} CategoryId={CategoryId} Slug={Slug} AdministratorId={AdministratorId}",

                "category_created",

                category.Id,

                category.Slug.Value,

                administratorId);



            await RecordCategoryAuditAsync(

                AuditActions.ToolboxCategoryCreated,

                category,

                previousState: "none",

                administratorId,

                cancellationToken);



            return ToDto(category);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<ToolCategoryAdminDto> UpdateAsync(

        Guid id,

        UpdateToolCategoryRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var category = await GetAggregateAsync(id, cancellationToken);

            var previousState = FormatActiveState(category.IsActive);

            var changed = category.UpdateDetails(

                request.Name,

                request.Description,

                request.Icon,

                request.DisplayOrder,

                _clock.UtcNow);



            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Toolbox category updated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_updated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.ToolboxCategoryUpdated,

                    category,

                    previousState,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(category);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<ToolCategoryAdminDto> ActivateAsync(

        Guid id,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var category = await GetAggregateAsync(id, cancellationToken);

            var previousState = FormatActiveState(category.IsActive);

            var changed = category.Activate(_clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Toolbox category activated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_activated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.ToolboxCategoryActivated,

                    category,

                    previousState,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(category);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<ToolCategoryAdminDto> DeactivateAsync(

        Guid id,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var category = await GetAggregateAsync(id, cancellationToken);

            var previousState = FormatActiveState(category.IsActive);

            var changed = category.Deactivate(_clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Toolbox category deactivated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_deactivated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.ToolboxCategoryDeactivated,

                    category,

                    previousState,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(category);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public Task<IReadOnlyList<ToolCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default) =>

        _queries.GetAllAsync(cancellationToken);



    public async Task<ToolCategoryAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    {

        var dto = await _queries.GetByIdAsync(id, cancellationToken);

        if (dto is null)

        {

            throw new ToolboxException(

                "Category was not found.",

                ToolboxApplicationErrorCodes.CategoryNotFound);

        }



        return dto;

    }



    private async Task RecordCategoryAuditAsync(

        string action,

        ToolCategory category,

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

            SubjectId: category.Id,

            SubjectType: "ToolCategory",

            SubjectDisplay: category.Slug.Value,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["toolId"] = category.Id.ToString(),

                ["toolSlug"] = category.Slug.Value,

                ["previousState"] = previousState,

                ["newState"] = FormatActiveState(category.IsActive),

            }), cancellationToken);

    }



    private async Task<ToolCategory> GetAggregateAsync(Guid id, CancellationToken cancellationToken)

    {

        var category = await _repository.GetByIdAsync(id, cancellationToken);

        if (category is null)

        {

            throw new ToolboxException(

                "Category was not found.",

                ToolboxApplicationErrorCodes.CategoryNotFound);

        }



        return category;

    }



    private static string FormatActiveState(bool isActive) =>

        isActive ? "active" : "inactive";



    private static ToolCategoryAdminDto ToDto(ToolCategory category) =>

        new(

            category.Id,

            category.Name,

            category.Slug.Value,

            category.Description,

            category.Icon,

            category.DisplayOrder,

            category.IsActive,

            category.CreatedAtUtc,

            category.UpdatedAtUtc);



    private static ToolboxException Wrap(DomainException ex) =>

        new(ex.Message, ex.Code ?? ToolboxApplicationErrorCodes.CategoryNameInvalid, ex);

}


