using HelpDev.Modules.Administration.Application.Persistence;

using HelpDev.Modules.Administration.Domain.Settings;

using HelpDev.SharedApplication.Abstractions.Persistence;

using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedKernel.Exceptions;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;



namespace HelpDev.Modules.Administration.Application.Settings;



public sealed class SystemSettingService : ISystemSettingService

{

    private readonly ISystemSettingRepository _repository;

    private readonly ISystemSettingQueries _queries;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _clock;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<SystemSettingService> _logger;



    public SystemSettingService(

        ISystemSettingRepository repository,

        ISystemSettingQueries queries,

        IUnitOfWork unitOfWork,

        IDateTimeProvider clock,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<SystemSettingService> logger)

    {

        _repository = repository;

        _queries = queries;

        _unitOfWork = unitOfWork;

        _clock = clock;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default) =>

        _queries.GetAllAsync(cancellationToken);



    public async Task<SystemSettingDto> GetByKeyAsync(string key, CancellationToken cancellationToken = default)

    {

        var normalized = NormalizeKeyOrThrow(key);

        var dto = await _queries.GetByKeyAsync(normalized, cancellationToken);

        if (dto is null)

        {

            throw new AdministrationException(

                "System setting was not found.",

                AdministrationApplicationErrorCodes.SettingNotFound);

        }



        return dto;

    }



    public async Task<SystemSettingDto> CreateAsync(

        CreateSystemSettingRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var valueType = SystemSettingValueTypeParser.Parse(request.ValueType);

            var normalizedKey = SystemSetting.NormalizeKey(request.Key);

            if (await _repository.ExistsByKeyAsync(normalizedKey, cancellationToken))

            {

                throw new AdministrationException(

                    "System setting key is already in use.",

                    AdministrationApplicationErrorCodes.SettingKeyDuplicate);

            }



            var setting = SystemSetting.Create(

                Guid.NewGuid(),

                normalizedKey,

                request.Value,

                valueType,

                request.Description,

                request.IsPublic,

                _clock.UtcNow);



            await _repository.AddAsync(setting, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);



            _logger.LogInformation(

                "Administration system setting created. Operation={Operation} SettingId={SettingId} Key={Key} AdministratorId={AdministratorId}",

                "setting_created",

                setting.Id,

                setting.Key,

                administratorId);



            await _auditRecorder.RecordAsync(new AuditRecordInput(

                Category: AuditCategories.Administration,

                Action: AuditActions.AdministrationSettingCreated,

                Outcome: AuditOutcomes.Success,

                ActorUserId: administratorId,

                ActorType: ResolveActorType(administratorId),

                SubjectId: setting.Id,

                SubjectType: "SystemSetting",

                SubjectDisplay: setting.Key,

                CorrelationId: _auditRequestContext.CorrelationId,

                RequestMethod: _auditRequestContext.RequestMethod,

                RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

                Metadata: new Dictionary<string, string>

                {

                    ["key"] = setting.Key,

                    ["isPublic"] = setting.IsPublic.ToString().ToLowerInvariant(),

                    ["valueChanged"] = "true",

                }), cancellationToken);



            return ToDto(setting);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<SystemSettingDto> UpdateAsync(

        string key,

        UpdateSystemSettingRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var setting = await GetAggregateAsync(key, cancellationToken);

            var previousValue = setting.Value;

            var previousIsPublic = setting.IsPublic;

            var changed = setting.UpdateValue(request.Value, _clock.UtcNow);

            changed |= setting.UpdateDescription(request.Description, _clock.UtcNow);

            if (request.IsPublic is not null)

            {

                changed |= setting.ChangeVisibility(request.IsPublic.Value, _clock.UtcNow);

            }



            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration system setting updated. Operation={Operation} SettingId={SettingId} Key={Key} AdministratorId={AdministratorId}",

                    "setting_updated",

                    setting.Id,

                    setting.Key,

                    administratorId);



                await _auditRecorder.RecordAsync(new AuditRecordInput(

                    Category: AuditCategories.Administration,

                    Action: AuditActions.AdministrationSettingUpdated,

                    Outcome: AuditOutcomes.Success,

                    ActorUserId: administratorId,

                    ActorType: ResolveActorType(administratorId),

                    SubjectId: setting.Id,

                    SubjectType: "SystemSetting",

                    SubjectDisplay: setting.Key,

                    CorrelationId: _auditRequestContext.CorrelationId,

                    RequestMethod: _auditRequestContext.RequestMethod,

                    RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

                    Metadata: new Dictionary<string, string>

                    {

                        ["key"] = setting.Key,

                        ["isPublic"] = setting.IsPublic.ToString().ToLowerInvariant(),

                        ["valueChanged"] = (!string.Equals(previousValue, setting.Value, StringComparison.Ordinal)

                            || previousIsPublic != setting.IsPublic).ToString().ToLowerInvariant(),

                    }), cancellationToken);

            }



            return ToDto(setting);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    private async Task<SystemSetting> GetAggregateAsync(string key, CancellationToken cancellationToken)

    {

        var normalized = NormalizeKeyOrThrow(key);

        var setting = await _repository.GetByKeyAsync(normalized, cancellationToken);

        if (setting is null)

        {

            throw new AdministrationException(

                "System setting was not found.",

                AdministrationApplicationErrorCodes.SettingNotFound);

        }



        return setting;

    }



    private static string NormalizeKeyOrThrow(string key)

    {

        try

        {

            return SystemSetting.NormalizeKey(key);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    private static string ResolveActorType(Guid? actorUserId) =>

        actorUserId.HasValue ? AuditActorTypes.User : AuditActorTypes.System;



    private static SystemSettingDto ToDto(SystemSetting setting) =>

        new(

            setting.Id,

            setting.Key,

            setting.Value,

            setting.ValueType.ToString(),

            setting.Description,

            setting.IsPublic,

            setting.CreatedAtUtc,

            setting.UpdatedAtUtc);



    private static AdministrationException Wrap(DomainException ex) =>

        new(ex.Message, ex.Code ?? AdministrationApplicationErrorCodes.SettingValueInvalid, ex);

}


