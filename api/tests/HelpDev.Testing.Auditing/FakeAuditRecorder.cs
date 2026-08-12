using HelpDev.SharedContracts.Auditing;



namespace HelpDev.Testing.Auditing;



public sealed class FakeAuditRecorder : IAuditRecorder

{

    public List<AuditRecordInput> Recorded { get; } = [];



    public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default)

    {

        Recorded.Add(input);

        return Task.CompletedTask;

    }

}

