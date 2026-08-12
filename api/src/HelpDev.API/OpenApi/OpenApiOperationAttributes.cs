namespace HelpDev.API.OpenApi;

[AttributeUsage(AttributeTargets.Method)]
public sealed class OpenApiOperationIdAttribute : Attribute
{
    public OpenApiOperationIdAttribute(string operationId) => OperationId = operationId;

    public string OperationId { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class OpenApiSummaryAttribute : Attribute
{
    public OpenApiSummaryAttribute(string summary, string? description = null)
    {
        Summary = summary;
        Description = description;
    }

    public string Summary { get; }

    public string? Description { get; }
}
