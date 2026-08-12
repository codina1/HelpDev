using System.Text.Json;

namespace HelpDev.API.Tests.OpenApi;

[Trait("Category", "OpenApi")]
public sealed class OpenApiDocumentRegressionTests
{
    [Fact]
    public void All_v1_document_matches_contract_expectations()
    {
        var directory = OpenApiArtifactLocator.RequireArtifactsDirectory();
        var path = Path.Combine(directory, "helpdev-all-v1.json");

        Assert.True(File.Exists(path), $"Expected complete OpenAPI artifact at {path}.");

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("openapi", out var openApiVersion));
        Assert.StartsWith("3.", openApiVersion.GetString(), StringComparison.Ordinal);

        Assert.True(root.TryGetProperty("info", out var info));
        Assert.Equal("v1", info.GetProperty("version").GetString());

        Assert.True(root.TryGetProperty("components", out var components));
        Assert.True(components.TryGetProperty("schemas", out var schemas));
        Assert.True(schemas.TryGetProperty("ApiErrorResponse", out var errorSchema));
        Assert.True(errorSchema.GetProperty("properties").TryGetProperty("message", out _));
        Assert.True(errorSchema.GetProperty("properties").TryGetProperty("code", out _));

        Assert.True(components.TryGetProperty("securitySchemes", out var securitySchemes));
        var bearer = securitySchemes.GetProperty("Bearer");
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());

        Assert.True(root.TryGetProperty("paths", out var paths));

        var operationIds = new HashSet<string>(StringComparer.Ordinal);
        var pathMethodKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var pathEntry in paths.EnumerateObject())
        {
            foreach (var operationEntry in pathEntry.Value.EnumerateObject())
            {
                if (operationEntry.NameEquals("parameters") || operationEntry.NameEquals("servers"))
                {
                    continue;
                }

                var key = $"{pathEntry.Name}:{operationEntry.Name}";
                Assert.True(pathMethodKeys.Add(key), $"Duplicate path+method documented: {key}.");

                var operation = operationEntry.Value;
                Assert.True(operation.TryGetProperty("operationId", out var operationIdElement));
                var operationId = operationIdElement.GetString();
                Assert.False(string.IsNullOrWhiteSpace(operationId));
                Assert.True(operationIds.Add(operationId!), $"Duplicate operationId: {operationId}.");
            }
        }

        Assert.True(paths.TryGetProperty("/api/v1/auth/send-otp", out var requestOtpPath));
        var requestOtp = requestOtpPath.GetProperty("post");
        Assert.Equal("Auth_RequestOtp", requestOtp.GetProperty("operationId").GetString());
        Assert.False(RequiresBearer(requestOtp));

        Assert.True(paths.TryGetProperty("/api/v1/admin/analytics/overview", out var adminOverviewPath));
        var adminOverview = adminOverviewPath.GetProperty("get");
        Assert.Equal("AnalyticsAdmin_GetOverview", adminOverview.GetProperty("operationId").GetString());
        Assert.True(RequiresBearer(adminOverview));

        Assert.True(paths.TryGetProperty("/api/health", out var legacyHealthPath));
        var legacyHealth = legacyHealthPath.GetProperty("get");
        Assert.True(legacyHealth.GetProperty("deprecated").GetBoolean());

        Assert.True(paths.TryGetProperty("/health/live", out _));
        Assert.True(paths.TryGetProperty("/health/ready", out _));

        var sampleOperation = adminOverview;
        Assert.True(HasCorrelationIdParameter(sampleOperation));
    }

    private static bool RequiresBearer(JsonElement operation)
    {
        if (!operation.TryGetProperty("security", out var security) || security.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        if (security.GetArrayLength() == 0)
        {
            return false;
        }

        foreach (var requirement in security.EnumerateArray())
        {
            if (requirement.TryGetProperty("Bearer", out _))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasCorrelationIdParameter(JsonElement operation)
    {
        if (!operation.TryGetProperty("parameters", out var parameters))
        {
            return false;
        }

        foreach (var parameter in parameters.EnumerateArray())
        {
            if (parameter.TryGetProperty("name", out var name)
                && string.Equals(name.GetString(), "X-Correlation-ID", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
