using HelpDev.Modules.Administration.Application.Persistence;

using HelpDev.Modules.Administration.Domain.Announcements;

using HelpDev.SharedApplication.Abstractions.Persistence;

using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedKernel.Exceptions;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;



namespace HelpDev.Modules.Administration.Application.Announcements;



public sealed class AnnouncementService : IAnnouncementService

{

    private readonly IAnnouncementRepository _repository;

    private readonly IAnnouncementQueries _queries;

    private readonly IUnitOfWork _unitOfWork;

    private readonly IDateTimeProvider _clock;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<AnnouncementService> _logger;



    public AnnouncementService(

        IAnnouncementRepository repository,

        IAnnouncementQueries queries,

        IUnitOfWork unitOfWork,

        IDateTimeProvider clock,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<AnnouncementService> logger)

    {

        _repository = repository;

        _queries = queries;

        _unitOfWork = unitOfWork;

        _clock = clock;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public async Task<AnnouncementDto> CreateAsync(

        CreateAnnouncementRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var type = AnnouncementEnumParser.ParseType(request.Type);

            var announcement = Announcement.CreateDraft(

                Guid.NewGuid(),

                request.Title,

                request.Body,

                type,

                request.StartsAtUtc,

                request.EndsAtUtc,

                _clock.UtcNow);



            await _repository.AddAsync(announcement, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);



            _logger.LogInformation(

                "Administration announcement created. Operation={Operation} AnnouncementId={AnnouncementId} AdministratorId={AdministratorId}",

                "announcement_created",

                announcement.Id,

                administratorId);



            await RecordAnnouncementAuditAsync(

                AuditActions.AdministrationAnnouncementCreated,

                announcement,

                previousStatus: "none",

                administratorId,

                cancellationToken);



            return ToDto(announcement);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<AnnouncementDto> UpdateAsync(

        Guid id,

        UpdateAnnouncementRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        try

        {

            var announcement = await GetAggregateAsync(id, cancellationToken);

            var previousStatus = announcement.Status.ToString().ToLowerInvariant();

            var type = AnnouncementEnumParser.ParseType(request.Type);

            var changed = announcement.UpdateDetails(request.Title, request.Body, type, _clock.UtcNow);

            changed |= announcement.UpdateSchedule(request.StartsAtUtc, request.EndsAtUtc, _clock.UtcNow);



            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration announcement updated. Operation={Operation} AnnouncementId={AnnouncementId} AdministratorId={AdministratorId}",

                    "announcement_updated",

                    announcement.Id,

                    administratorId);



                await RecordAnnouncementAuditAsync(

                    AuditActions.AdministrationAnnouncementUpdated,

                    announcement,

                    previousStatus,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(announcement);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<AnnouncementDto> PublishAsync(

        Guid id,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var announcement = await GetAggregateAsync(id, cancellationToken);

            var previousStatus = announcement.Status.ToString().ToLowerInvariant();

            var changed = announcement.Publish(_clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration announcement published. Operation={Operation} AnnouncementId={AnnouncementId} AdministratorId={AdministratorId}",

                    "announcement_published",

                    announcement.Id,

                    administratorId);



                await RecordAnnouncementAuditAsync(

                    AuditActions.AdministrationAnnouncementPublished,

                    announcement,

                    previousStatus,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(announcement);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<AnnouncementDto> ArchiveAsync(

        Guid id,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var announcement = await GetAggregateAsync(id, cancellationToken);

            var previousStatus = announcement.Status.ToString().ToLowerInvariant();

            var changed = announcement.Archive(_clock.UtcNow);

            if (changed)

            {

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(

                    "Administration announcement archived. Operation={Operation} AnnouncementId={AnnouncementId} AdministratorId={AdministratorId}",

                    "announcement_archived",

                    announcement.Id,

                    administratorId);



                await RecordAnnouncementAuditAsync(

                    AuditActions.AdministrationAnnouncementArchived,

                    announcement,

                    previousStatus,

                    administratorId,

                    cancellationToken);

            }



            return ToDto(announcement);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task DeleteAsync(

        Guid id,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        try

        {

            var announcement = await GetAggregateAsync(id, cancellationToken);

            announcement.EnsureCanHardDelete();

            _repository.Remove(announcement);

            await _unitOfWork.SaveChangesAsync(cancellationToken);



            _logger.LogInformation(

                "Administration announcement deleted. Operation={Operation} AnnouncementId={AnnouncementId} AdministratorId={AdministratorId}",

                "announcement_deleted",

                announcement.Id,

                administratorId);

        }

        catch (DomainException ex)

        {

            throw Wrap(ex);

        }

    }



    public async Task<AnnouncementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)

    {

        var dto = await _queries.GetByIdAsync(id, cancellationToken);

        if (dto is null)

        {

            throw new AdministrationException(

                "Announcement was not found.",

                AdministrationApplicationErrorCodes.AnnouncementNotFound);

        }



        return dto;

    }



    public Task<AnnouncementPageDto> GetPageAsync(

        AnnouncementFilter filter,

        CancellationToken cancellationToken = default) =>

        _queries.GetPageAsync(filter, cancellationToken);



    private async Task RecordAnnouncementAuditAsync(

        string action,

        Announcement announcement,

        string previousStatus,

        Guid? administratorId,

        CancellationToken cancellationToken)

    {

        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.Administration,

            Action: action,

            Outcome: AuditOutcomes.Success,

            ActorUserId: administratorId,

            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,

            SubjectId: announcement.Id,

            SubjectType: "Announcement",

            SubjectDisplay: announcement.Title,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["announcementId"] = announcement.Id.ToString(),

                ["previousStatus"] = previousStatus,

                ["newStatus"] = announcement.Status.ToString().ToLowerInvariant(),

            }), cancellationToken);

    }



    private async Task<Announcement> GetAggregateAsync(Guid id, CancellationToken cancellationToken)

    {

        var announcement = await _repository.GetByIdAsync(id, cancellationToken);

        if (announcement is null)

        {

            throw new AdministrationException(

                "Announcement was not found.",

                AdministrationApplicationErrorCodes.AnnouncementNotFound);

        }



        return announcement;

    }



    private static AnnouncementDto ToDto(Announcement announcement) =>

        new(

            announcement.Id,

            announcement.Title,

            announcement.Body,

            announcement.Type.ToString(),

            announcement.Status.ToString(),

            announcement.StartsAtUtc,

            announcement.EndsAtUtc,

            announcement.CreatedAtUtc,

            announcement.UpdatedAtUtc,

            announcement.PublishedAtUtc);



    private static AdministrationException Wrap(DomainException ex) =>

        new(ex.Message, ex.Code ?? AdministrationApplicationErrorCodes.AnnouncementStatusInvalid, ex);

}


