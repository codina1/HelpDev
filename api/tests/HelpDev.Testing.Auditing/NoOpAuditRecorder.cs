using HelpDev.SharedContracts.Auditing;



namespace HelpDev.Testing.Auditing;



public sealed class NoOpAuditRecorder : IAuditRecorder

{

    public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default) =>

        Task.CompletedTask;

}

