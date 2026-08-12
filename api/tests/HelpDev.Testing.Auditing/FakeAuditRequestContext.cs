using HelpDev.SharedContracts.Auditing;



namespace HelpDev.Testing.Auditing;



public sealed class FakeAuditRequestContext : IAuditRequestContext

{

    public string? RequestMethod { get; init; } = "POST";



    public string? RequestPathTemplate { get; init; } = "/api/test";



    public string? CorrelationId { get; init; } = "test-correlation-id";

}

