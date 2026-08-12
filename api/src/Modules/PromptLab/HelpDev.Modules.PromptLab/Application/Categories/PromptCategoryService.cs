using HelpDev.Modules.PromptLab.Application.Persistence;

using HelpDev.Modules.PromptLab.Domain.Categories;

using HelpDev.Modules.PromptLab.Domain.Prompts;

using HelpDev.SharedApplication.Abstractions.Persistence;

using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedKernel.Exceptions;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;



namespace HelpDev.Modules.PromptLab.Application.Categories;



public sealed class PromptCategoryService : IPromptCategoryService

{

    private readonly IPromptCategoryRepository _repository;

    private readonly IPromptCategoryQueries _queries;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _clock;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<PromptCategoryService> _logger;



    public PromptCategoryService(

        IPromptCategoryRepository repository,

        IPromptCategoryQueries queries,

        IUnitOfWork unitOfWork,

        IDateTimeProvider clock,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<PromptCategoryService> logger)

    {

        _repository = repository;

        _queries = queries;

        _unitOfWork = unitOfWork;

        _clock = clock;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public async Task<PromptCategoryAdminDto> CreateAsync(

        CreatePromptCategoryRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var slug = PromptSlug.Create(

                request.Slug,

                PromptCategory.SlugMaxLength,

                PromptLabApplicationErrorCodes.CategorySlugRequired,

                PromptLabApplicationErrorCodes.CategorySlugInvalid);



            if (await _repository.ExistsBySlugAsync(slug.Value, cancellationToken))

            {

                throw new PromptLabException(

                    "Category slug is already in use.",

                    PromptLabApplicationErrorCodes.CategorySlugDuplicate);

            }



            var category = PromptCategory.Create(

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

                "PromptLab category created. Operation={Operation} CategoryId={CategoryId} Slug={Slug} AdministratorId={AdministratorId}",

                "category_created",

                category.Id,

                category.Slug.Value,

                administratorId);



            await RecordCategoryAuditAsync(

                AuditActions.PromptLabCategoryCreated,

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



    public async Task<PromptCategoryAdminDto> UpdateAsync(

        Guid id,

        UpdatePromptCategoryRequest request,

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

                    "PromptLab category updated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_updated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.PromptLabCategoryUpdated,

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



    public async Task<PromptCategoryAdminDto> ActivateAsync(

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

                    "PromptLab category activated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_activated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.PromptLabCategoryActivated,

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



    public async Task<PromptCategoryAdminDto> DeactivateAsync(

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

                    "PromptLab category deactivated. Operation={Operation} CategoryId={CategoryId} AdministratorId={AdministratorId}",

                    "category_deactivated",

                    category.Id,

                    administratorId);



                await RecordCategoryAuditAsync(

                    AuditActions.PromptLabCategoryDeactivated,

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



    public Task<IReadOnlyList<PromptCategoryAdminDto>> GetAllAsync(CancellationToken cancellationToken = default) =>

        _queries.GetAllAsync(cancellationToken);



    public async Task<PromptCategoryAdminDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    {

        var dto = await _queries.GetByIdAsync(id, cancellationToken);

        if (dto is null)

        {

            throw new PromptLabException(

                "Category was not found.",

                PromptLabApplicationErrorCodes.CategoryNotFound);

        }



        return dto;

    }



    private async Task RecordCategoryAuditAsync(

        string action,

        PromptCategory category,

        string previousState,

        Guid? administratorId,

        CancellationToken cancellationToken)

    {

        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.PromptManagement,

            Action: action,

            Outcome: AuditOutcomes.Success,

            ActorUserId: administratorId,

            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,

            SubjectId: category.Id,

            SubjectType: "PromptCategory",

            SubjectDisplay: category.Slug.Value,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["promptId"] = category.Id.ToString(),

                ["promptSlug"] = category.Slug.Value,

                ["previousState"] = previousState,

                ["newState"] = FormatActiveState(category.IsActive),

            }), cancellationToken);

    }



    private async Task<PromptCategory> GetAggregateAsync(Guid id, CancellationToken cancellationToken)

    {

        var category = await _repository.GetByIdAsync(id, cancellationToken);

        if (category is null)

        {

            throw new PromptLabException(

                "Category was not found.",

                PromptLabApplicationErrorCodes.CategoryNotFound);

        }



        return category;

    }



    private static string FormatActiveState(bool isActive) =>

        isActive ? "active" : "inactive";



    private static PromptCategoryAdminDto ToDto(PromptCategory category) =>

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



    private static PromptLabException Wrap(DomainException ex) =>

        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.CategoryNameInvalid, ex);

}


