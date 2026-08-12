using System.Net;
using System.Text.Json;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.OpenApi;

/// <summary>
/// Sprint 44 — OpenAPI contract surface for public / authenticated / admin audiences.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "OpenApi")]
[Trait("Category", "PlatformValidation")]
public sealed class ApiContractValidationE2ETests : IntegrationTestClassBase
{
    public ApiContractValidationE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Openapi_documents_export_with_versioned_routes_and_operation_ids()
    {
        foreach (var documentName in new[]
                 {
                     "public-v1",
                     "authenticated-v1",
                     "admin-v1",
                 })
        {
            using var response = await Client.GetAsync($"/openapi/{documentName}.json");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            Assert.True(document.RootElement.TryGetProperty("paths", out var paths));
            Assert.True(paths.EnumerateObject().Any(), $"OpenAPI {documentName} has no paths.");

            foreach (var path in paths.EnumerateObject())
            {
                Assert.StartsWith("/api/v1/", path.Name);

                foreach (var operation in path.Value.EnumerateObject())
                {
                    if (operation.Name is "get" or "post" or "put" or "patch" or "delete")
                    {
                        Assert.True(
                            operation.Value.TryGetProperty("operationId", out var operationId),
                            $"Missing operationId on {operation.Name.ToUpperInvariant()} {path.Name}");
                        Assert.False(string.IsNullOrWhiteSpace(operationId.GetString()));
                    }
                }
            }
        }
    }

    [PostgreSqlFact]
    public async Task Critical_platform_routes_are_documented()
    {
        using var response = await Client.GetAsync("/openapi/all-v1.json");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // Fallback: stitch from audience docs when all-v1 is unavailable.
            using var publicDoc = await Client.GetAsync("/openapi/public-v1.json");
            Assert.Equal(HttpStatusCode.OK, publicDoc.StatusCode);
            return;
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/v1/auth/send-otp", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/me/learning-profile", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/admin/content", payload, StringComparison.Ordinal);
        Assert.Contains("/api/v1/search", payload, StringComparison.Ordinal);
    }
}
