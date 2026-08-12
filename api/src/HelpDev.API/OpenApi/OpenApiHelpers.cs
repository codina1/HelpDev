using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace HelpDev.API.OpenApi;

public static class SchemaIdSelector
{
    public static string GetSchemaId(Type type)
    {
        if (type.IsGenericType)
        {
            var typeName = type.Name;
            var tickIndex = typeName.IndexOf('`');
            if (tickIndex >= 0)
            {
                typeName = typeName[..tickIndex];
            }

            var args = type.GetGenericArguments().Select(GetSchemaId);
            return $"{typeName}Of{string.Join("And", args)}";
        }

        if (type.IsArray)
        {
            return $"ArrayOf{GetSchemaId(type.GetElementType()!)}";
        }

        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying is not null)
        {
            return GetSchemaId(nullableUnderlying);
        }

        return type.Name
            .Replace("+", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal);
    }
}

public sealed class ApiDescriptionMetadata
{
    public required string? Audience { get; init; }
    public required string? OperationId { get; init; }
    public required string? Summary { get; init; }
    public required string? Description { get; init; }
}

public static class ApiDescriptionMetadataReader
{
    public static ApiDescriptionMetadata Read(ApiDescription description)
    {
        string? audience = null;
        string? operationId = null;
        string? summary = null;
        string? descriptionText = null;

        if (description.ActionDescriptor is ControllerActionDescriptor controllerAction)
        {
            audience = controllerAction.MethodInfo.GetCustomAttribute<ApiAudienceAttribute>(inherit: true)?.Audience
                ?? controllerAction.ControllerTypeInfo.GetCustomAttribute<ApiAudienceAttribute>(inherit: true)?.Audience;

            operationId = controllerAction.MethodInfo.GetCustomAttribute<OpenApiOperationIdAttribute>(inherit: true)?.OperationId;

            var summaryAttribute = controllerAction.MethodInfo.GetCustomAttribute<OpenApiSummaryAttribute>(inherit: true);
            summary = summaryAttribute?.Summary;
            descriptionText = summaryAttribute?.Description;
        }

        return new ApiDescriptionMetadata
        {
            Audience = audience,
            OperationId = operationId,
            Summary = summary,
            Description = descriptionText,
        };
    }
}

public static class OpenApiErrorExamples
{
    public static Microsoft.OpenApi.Any.OpenApiObject Create(string message, string code) =>
        new()
        {
            ["message"] = new Microsoft.OpenApi.Any.OpenApiString(message),
            ["code"] = new Microsoft.OpenApi.Any.OpenApiString(code),
        };
}

public static class OpenApiPathHelpers
{
    public static bool IsCanonicalVersionedApiPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        return relativePath.StartsWith("api/v", StringComparison.OrdinalIgnoreCase);
    }
}
