using HelpDev.SharedContracts.Auditing;

namespace HelpDev.Modules.Auditing.Domain.Records;

public sealed class AuditRecord
{
    private AuditRecord()
    {
    }

    public Guid Id { get; private init; }

    public DateTime OccurredAtUtc { get; private init; }

    public string Category { get; private init; } = null!;

    public string Action { get; private init; } = null!;

    public string Outcome { get; private init; } = null!;

    public Guid? ActorUserId { get; private init; }

    public string ActorType { get; private init; } = null!;

    public Guid? SubjectId { get; private init; }

    public string? SubjectType { get; private init; }

    public string? SubjectDisplay { get; private init; }

    public string? ReasonCode { get; private init; }

    public string? CorrelationId { get; private init; }

    public string? RequestMethod { get; private init; }

    public string? RequestPathTemplate { get; private init; }

    public IReadOnlyDictionary<string, string>? Metadata { get; private init; }

    public DateTime CreatedAtUtc { get; private init; }

    public static AuditRecord Create(
        Guid id,
        DateTime occurredAtUtc,
        string category,
        string action,
        string outcome,
        Guid? actorUserId,
        string actorType,
        Guid? subjectId,
        string? subjectType,
        string? subjectDisplay,
        string? reasonCode,
        string? correlationId,
        string? requestMethod,
        string? requestPathTemplate,
        IReadOnlyDictionary<string, string>? metadata,
        DateTime createdAtUtc,
        int maxReasonLength,
        int maxSubjectDisplayLength,
        int maxPathTemplateLength,
        int maxCorrelationIdLength)
    {
        if (id == Guid.Empty)
        {
            throw new AuditException("Audit record id is required.", AuditErrorCodes.RecordInvalid);
        }

        if (!AuditCategories.IsSupported(category))
        {
            throw new AuditException("Audit category is not supported.", AuditErrorCodes.CategoryInvalid);
        }

        if (!AuditActions.IsSupported(action))
        {
            throw new AuditException("Audit action is not supported.", AuditErrorCodes.ActionUnsupported);
        }

        if (!AuditOutcomes.IsSupported(outcome))
        {
            throw new AuditException("Audit outcome is not supported.", AuditErrorCodes.OutcomeInvalid);
        }

        if (string.IsNullOrWhiteSpace(actorType) ||
            actorType is not (AuditActorTypes.User or AuditActorTypes.Anonymous or AuditActorTypes.System))
        {
            throw new AuditException("Audit actor type is invalid.", AuditErrorCodes.RecordInvalid);
        }

        if (reasonCode is not null && reasonCode.Length > maxReasonLength)
        {
            throw new AuditException("Audit reason exceeds maximum length.", AuditErrorCodes.RecordInvalid);
        }

        if (subjectDisplay is not null && subjectDisplay.Length > maxSubjectDisplayLength)
        {
            throw new AuditException("Audit subject display exceeds maximum length.", AuditErrorCodes.RecordInvalid);
        }

        if (requestPathTemplate is not null && requestPathTemplate.Length > maxPathTemplateLength)
        {
            throw new AuditException("Audit request path template exceeds maximum length.", AuditErrorCodes.RecordInvalid);
        }

        if (correlationId is not null && correlationId.Length > maxCorrelationIdLength)
        {
            throw new AuditException("Audit correlation id exceeds maximum length.", AuditErrorCodes.RecordInvalid);
        }

        return new AuditRecord
        {
            Id = id,
            OccurredAtUtc = occurredAtUtc,
            Category = category,
            Action = action,
            Outcome = outcome,
            ActorUserId = actorUserId,
            ActorType = actorType,
            SubjectId = subjectId,
            SubjectType = subjectType,
            SubjectDisplay = subjectDisplay,
            ReasonCode = reasonCode,
            CorrelationId = correlationId,
            RequestMethod = requestMethod,
            RequestPathTemplate = requestPathTemplate,
            Metadata = metadata,
            CreatedAtUtc = createdAtUtc,
        };
    }
}
