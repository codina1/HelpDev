using System.Net;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.OpenApi;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "OpenApi")]
public sealed class ApiVersioningTests : IntegrationTestClassBase
{
    public ApiVersioningTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Versioned_and_legacy_content_routes_return_same_status_family_for_anonymous_users()
    {
        using var versioned = await Client.GetAsync("/api/v1/content");
        using var legacy = await Client.GetAsync("/api/content");

        Assert.Equal((int)versioned.StatusCode / 100, (int)legacy.StatusCode / 100);
    }

    [PostgreSqlFact]
    public async Task Unsupported_api_version_returns_client_error_not_server_error()
    {
        using var response = await Client.GetAsync("/api/v99/content");

        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Expected 400 or 404 for unsupported version, got {(int)response.StatusCode}.");
    }

    [PostgreSqlFact]
    public async Task Health_live_remains_available()
    {
        using var response = await Client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Legacy_api_health_remains_available()
    {
        using var response = await Client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Public_and_admin_openapi_documents_are_available()
    {
        using var publicDocument = await Client.GetAsync("/openapi/public-v1.json");
        using var adminDocument = await Client.GetAsync("/openapi/admin-v1.json");

        Assert.Equal(HttpStatusCode.OK, publicDocument.StatusCode);
        Assert.Equal(HttpStatusCode.OK, adminDocument.StatusCode);

        var publicPayload = await publicDocument.Content.ReadAsStringAsync();
        Assert.Contains("\"openapi\":", publicPayload, StringComparison.Ordinal);

        var adminPayload = await adminDocument.Content.ReadAsStringAsync();
        Assert.Contains("\"openapi\":", adminPayload, StringComparison.Ordinal);
    }

    [PostgreSqlFact]
    public async Task Admin_route_without_token_returns_401()
    {
        using var response = await Client.GetAsync("/api/v1/admin/analytics/overview");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
