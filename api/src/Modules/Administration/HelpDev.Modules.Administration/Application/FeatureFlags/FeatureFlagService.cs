using HelpDev.Modules.Administration.Application.Persistence;

using HelpDev.Modules.Administration.Domain.FeatureFlags;

using HelpDev.SharedApplication.Abstractions.Persistence;

using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedKernel.Exceptions;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;



namespace HelpDev.Modules.Administration.Application.FeatureFlags;



public sealed class FeatureFlagService : IFeatureFlagService

{

    private readonly IFeatureFlagRepository _repository;

    private readonly IFeatureFlagQueries _queries;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _clock;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<FeatureFlagService> _logger;



    public FeatureFlagService(

        IFeatureFlagRepository repository,

        IFeatureFlagQueries queries,

        IUnitOfWork unitOfWork,

        IDateTimeProvider clock,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<FeatureFlagService> logger)

    {

        _repository = repository;

        _queries = queries;

        _unitOfWork = unitOfWork;

        _clock = clock;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default) =>

        _queries.GetAllAsync(cancellationToken);



    public async Task<FeatureFlagDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default)

    {

        var normalized = NormalizeKeyOrThrow(key);

        var dto = await _queries.GetByKeyAsync(normalized, cancellationToken);

        if (dto is null)

        {

            throw new AdministrationException(

                "Feature flag was not found.",

                AdministrationApplicationErrorCodes.FeatureNotFound);

        }



        return dto;

    }



    public async Task<FeatureFlagDto> CreateAsync(

        CreateFeatureFlagRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var normalizedKey = FeatureFlag.NormalizeKey(request.Key);

            if (await _repository.ExistsByKeyAsync(normalizedKey, cancellationToken))

            {

                throw new AdministrationException(

                    "Feature flag key is already in use.",

                    AdministrationApplicationErrorCodes.FeatureKeyDuplicate);

            }



            var flag = FeatureFlag.Create(

                Guid.NewGuid(),

                normalizedKey,

                request.IsEnabled,

                request.Description,

                _clock.UtcNow);



            await _repository.AddAsync(flag, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);



            _logger.LogInformation(

                "Administration feature flag created. Operation={Operation} FeatureFlagId={FeatureFlagId} Key={Key} AdministratorId={AdministratorId}",

                "feature_created",

                flag.Id,

                flag.Key,

                administratorId);



            await _auditRecorder.RecordAsync(new AuditRecordInput(

                Category: AuditCategories.Administration,

                Action: AuditActions.AdministrationFeatureFlagCreated,

                Outcome: AuditOutcomes.Success,

                ActorUserId: administratorId,

                ActorType: ResolveActorType(administratorId),

                SubjectId: flag.Id,

                SubjectType: "FeatureFlag",

                SubjectDisplay: flag.Key,

                CorrelationId: _auditRequestContext.CorrelationId,

                RequestMethod: _auditRequestContext.RequestMethod,

                RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

                Metadata: new Dictionary<string, string>

                {

                    ["key"] = flag.Key,

                    ["previousState"] = "none",

                    ["newState"] = FormatEnabledState(flag.IsEnabled),

                }), cancellationToken);



            return ToDto(flag);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<FeatureFlagDto> UpdateAsync(

        string key,

        UpdateFeatureFlagRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var flag = await GetAggregateAsync(key, cancellationToken);

            var previousState = FormatEnabledState(flag.IsEnabled);

            var changed = flag.UpdateDescription(request.Description, _clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration feature flag updated. Operation={Operation} FeatureFlagId={FeatureFlagId} Key={Key} AdministratorId={AdministratorId}",

                    "feature_updated",

                    flag.Id,

                    flag.Key,

                    administratorId);



                await _auditRecorder.RecordAsync(new AuditRecordInput(

                    Category: AuditCategories.Administration,

                    Action: AuditActions.AdministrationFeatureFlagUpdated,

                    Outcome: AuditOutcomes.Success,

                    ActorUserId: administratorId,

                    ActorType: ResolveActorType(administratorId),

                    SubjectId: flag.Id,

                    SubjectType: "FeatureFlag",

                    SubjectDisplay: flag.Key,

                    CorrelationId: _auditRequestContext.CorrelationId,

                    RequestMethod: _auditRequestContext.RequestMethod,

                    RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

                    Metadata: new Dictionary<string, string>

                    {

                        ["key"] = flag.Key,

                        ["previousState"] = previousState,

                        ["newState"] = FormatEnabledState(flag.IsEnabled),

                    }), cancellationToken);

            }



            return ToDto(flag);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<FeatureFlagDto> SetEnabledAsync(

        string key,

        bool isEnabled,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var flag = await GetAggregateAsync(key, cancellationToken);

            var previousState = FormatEnabledState(flag.IsEnabled);

            var changed = flag.UpdateState(isEnabled, _clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration feature flag state changed. Operation={Operation} FeatureFlagId={FeatureFlagId} Key={Key} IsEnabled={IsEnabled} AdministratorId={AdministratorId}",

                    "feature_state_changed",

                    flag.Id,

                    flag.Key,

                    flag.IsEnabled,

                    administratorId);



                await _auditRecorder.RecordAsync(new AuditRecordInput(

                    Category: AuditCategories.Administration,

                    Action: isEnabled

                        ? AuditActions.AdministrationFeatureFlagEnabled

                        : AuditActions.AdministrationFeatureFlagDisabled,

                    Outcome: AuditOutcomes.Success,

                    ActorUserId: administratorId,

                    ActorType: ResolveActorType(administratorId),

                    SubjectId: flag.Id,

                    SubjectType: "FeatureFlag",

                    SubjectDisplay: flag.Key,

                    CorrelationId: _auditRequestContext.CorrelationId,

                    RequestMethod: _auditRequestContext.RequestMethod,

                    RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

                    Metadata: new Dictionary<string, string>

                    {

                        ["key"] = flag.Key,

                        ["previousState"] = previousState,

                        ["newState"] = FormatEnabledState(flag.IsEnabled),

                    }), cancellationToken);

            }



            return ToDto(flag);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    private async Task<FeatureFlag> GetAggregateAsync(string key, CancellationToken cancellationToken)

    {

        var normalized = NormalizeKeyOrThrow(key);

        var flag = await _repository.GetByKeyAsync(normalized, cancellationToken);

        if (flag is null)

        {

            throw new AdministrationException(

                "Feature flag was not found.",

                AdministrationApplicationErrorCodes.FeatureNotFound);

        }



        return flag;

    }



    private static string NormalizeKeyOrThrow(string key)

    {

        try

        {

            return FeatureFlag.NormalizeKey(key);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    private static string ResolveActorType(Guid? actorUserId) =>

        actorUserId.HasValue ? AuditActorTypes.User : AuditActorTypes.System;



    private static string FormatEnabledState(bool isEnabled) =>

        isEnabled ? "enabled" : "disabled";



    private static FeatureFlagDto ToDto(FeatureFlag flag) =>

        new(flag.Id, flag.Key, flag.IsEnabled, flag.Description, flag.CreatedAtUtc, flag.UpdatedAtUtc);



    private static AdministrationException Wrap(DomainException ex) =>

        new(ex.Message, ex.Code ?? AdministrationApplicationErrorCodes.FeatureKeyInvalid, ex);

}


