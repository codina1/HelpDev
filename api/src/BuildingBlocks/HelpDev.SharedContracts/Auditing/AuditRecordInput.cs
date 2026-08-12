namespace HelpDev.SharedContracts.Auditing;

public sealed record AuditRecordInput(
    string Category,
    string Action,
    string Outcome,
    Guid? ActorUserId,
    string ActorType,
    Guid? SubjectId = null,
    string? SubjectType = null,
    string? SubjectDisplay = null,
    string? ReasonCode = null,
    string? CorrelationId = null,
    string? RequestMethod = null,
    string? RequestPathTemplate = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public interface IAuditRecorder
{
    Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default);
}

public interface IAuditRequestContext
{
    string? RequestMethod { get; }

    string? RequestPathTemplate { get; }

    string? CorrelationId { get; }
}

public interface ICorrelationContext
{
    string CorrelationId { get; }
}
