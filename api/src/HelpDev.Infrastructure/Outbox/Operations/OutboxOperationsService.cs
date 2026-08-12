using HelpDev.SharedContracts.Auditing;

using HelpDev.SharedInfrastructure.Outbox;

using HelpDev.SharedKernel.Time;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;



namespace HelpDev.Infrastructure.Outbox.Operations;



public sealed class OutboxOperationsService : IOutboxOperationsService

{

    public const int DefaultRetryFailedLimit = 50;

    public const int MinRetryFailedLimit = 1;

    public const int MaxRetryFailedLimit = 100;



    /// <summary>

    /// Documented PostgreSQL claim/reset shape for failed outbox rows (FOR UPDATE SKIP LOCKED).

    /// </summary>

    public const string ResetFailedBatchSql = """

        WITH failed AS (

            SELECT "Id"

            FROM outbox_messages

            WHERE processed_at_utc IS NULL

              AND attempt_count >= {0}

              AND (locked_until_utc IS NULL OR locked_until_utc < {1})

              AND ({2}::text IS NULL OR type = {2})

            ORDER BY occurred_at_utc, "Id"

            LIMIT {3}

            FOR UPDATE SKIP LOCKED

        )

        UPDATE outbox_messages AS o

        SET

            attempt_count = 0,

            last_attempt_at_utc = NULL,

            error = NULL,

            locked_until_utc = NULL,

            lock_id = NULL

        FROM failed

        WHERE o."Id" = failed."Id"

        """;



    private readonly IOutboxRetryStore _retryStore;

    private readonly IDateTimeProvider _clock;

    private readonly OutboxOptions _options;

    private readonly IAuditRecorder _auditRecorder;

    private readonly IAuditRequestContext _auditRequestContext;

    private readonly ILogger<OutboxOperationsService> _logger;



    public OutboxOperationsService(

        IOutboxRetryStore retryStore,

        IDateTimeProvider clock,

        IOptions<OutboxOptions> options,

        IAuditRecorder auditRecorder,

        IAuditRequestContext auditRequestContext,

        ILogger<OutboxOperationsService> logger)

    {

        _retryStore = retryStore;

        _clock = clock;

        _options = options.Value;

        _auditRecorder = auditRecorder;

        _auditRequestContext = auditRequestContext;

        _logger = logger;

    }



    public async Task<OutboxMessageDetailDto> RetryAsync(

        Guid messageId,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        if (messageId == Guid.Empty)

        {

            throw new OutboxOperationsException(

                "Outbox message id is invalid.",

                OutboxOperationsErrorCodes.OperationInvalid);

        }



        var now = _clock.UtcNow;

        var message = await _retryStore.GetTrackedByIdAsync(messageId, cancellationToken);



        if (message is null)

        {

            throw new OutboxOperationsException(

                "Outbox message was not found.",

                OutboxOperationsErrorCodes.MessageNotFound);

        }



        if (message.ProcessedAtUtc is not null)

        {

            throw new OutboxOperationsException(

                "Processed outbox messages cannot be retried.",

                OutboxOperationsErrorCodes.MessageAlreadyProcessed);

        }



        if (OutboxMessageStatuses.IsActivelyLocked(message.LockedUntilUtc, now))

        {

            throw new OutboxOperationsException(

                "Outbox message is currently being processed.",

                OutboxOperationsErrorCodes.MessageCurrentlyProcessing);

        }



        var previousStatus = OutboxMessageStatuses.Derive(

            message.ProcessedAtUtc,

            message.AttemptCount,

            message.LockedUntilUtc,

            now,

            _options.MaxAttempts);

        var wasDeadLetter = message.AttemptCount >= _options.MaxAttempts;



        var originalType = message.Type;

        var originalPayload = message.Payload;

        var originalOccurred = message.OccurredAtUtc;

        var originalId = message.Id;



        try

        {

            message.ResetForRetry(now);

        }

        catch (InvalidOperationException ex)

        {

            throw new OutboxOperationsException(ex.Message, OutboxOperationsErrorCodes.OperationInvalid, ex);

        }



        await _retryStore.SaveChangesAsync(cancellationToken);



        _logger.LogInformation(

            "Admin {AdministratorId} reset outbox message {MessageId} of type {EventType} for retry.",

            administratorId,

            message.Id,

            message.Type);



        if (message.Id != originalId

            || message.Type != originalType

            || message.Payload != originalPayload

            || message.OccurredAtUtc != originalOccurred)

        {

            throw new InvalidOperationException("Outbox retry mutated immutable message fields.");

        }



        var detail = ToDetail(message, now);



        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.OutboxOperations,

            Action: wasDeadLetter

                ? AuditActions.OutboxMessageReprocessed

                : AuditActions.OutboxRetryRequested,

            Outcome: AuditOutcomes.Success,

            ActorUserId: administratorId,

            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,

            SubjectId: message.Id,

            SubjectType: "OutboxMessage",

            SubjectDisplay: message.Type,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["messageId"] = message.Id.ToString(),

                ["operation"] = wasDeadLetter ? "reprocess" : "retry",

                ["previousStatus"] = previousStatus,

                ["newStatus"] = detail.Status,

            }), cancellationToken);



        return detail;

    }



    public async Task<RetryFailedOutboxResultDto> RetryFailedAsync(

        RetryFailedOutboxRequest request,

        Guid? administratorId = null,

        CancellationToken cancellationToken = default)

    {

        ArgumentNullException.ThrowIfNull(request);



        if (request.Limit < MinRetryFailedLimit || request.Limit > MaxRetryFailedLimit)

        {

            throw new OutboxOperationsException(

                $"Retry limit must be between {MinRetryFailedLimit} and {MaxRetryFailedLimit}.",

                OutboxOperationsErrorCodes.RetryLimitInvalid);

        }



        var typeFilter = string.IsNullOrWhiteSpace(request.Type) ? null : request.Type.Trim();

        var now = _clock.UtcNow;

        var resetCount = await _retryStore.ResetFailedBatchAsync(

            request.Limit,

            typeFilter,

            now,

            _options.MaxAttempts,

            cancellationToken);



        _logger.LogInformation(

            "Admin {AdministratorId} reset {ResetCount} failed outbox message(s) for retry (limit {Limit}, type {EventType}).",

            administratorId,

            resetCount,

            request.Limit,

            typeFilter ?? "(all)");



        await _auditRecorder.RecordAsync(new AuditRecordInput(

            Category: AuditCategories.OutboxOperations,

            Action: AuditActions.OutboxDeadletterRecoveryRequested,

            Outcome: AuditOutcomes.Success,

            ActorUserId: administratorId,

            ActorType: administratorId.HasValue ? AuditActorTypes.User : AuditActorTypes.System,

            CorrelationId: _auditRequestContext.CorrelationId,

            RequestMethod: _auditRequestContext.RequestMethod,

            RequestPathTemplate: _auditRequestContext.RequestPathTemplate,

            Metadata: new Dictionary<string, string>

            {

                ["messageId"] = Guid.Empty.ToString(),

                ["operation"] = "recovery",

                ["previousStatus"] = OutboxMessageStatuses.Failed,

                ["newStatus"] = OutboxMessageStatuses.Pending,

            }), cancellationToken);



        return new RetryFailedOutboxResultDto(

            request.Limit,

            resetCount,

            typeFilter,

            now);

    }



    private OutboxMessageDetailDto ToDetail(OutboxMessage message, DateTime nowUtc) =>

        new(

            message.Id,

            message.Type,

            message.OccurredAtUtc,

            message.ProcessedAtUtc,

            message.AttemptCount,

            message.LastAttemptAtUtc,

            message.Error,

            message.LockedUntilUtc,

            OutboxMessageStatuses.Derive(

                message.ProcessedAtUtc,

                message.AttemptCount,

                message.LockedUntilUtc,

                nowUtc,

                _options.MaxAttempts));

}


