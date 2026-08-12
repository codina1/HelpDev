using System.Net;
using System.Text.Json;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;

namespace HelpDev.Integration.Tests.Smoke;

/// <summary>
/// Sprint 44 — production readiness smoke suite (health, headers, CORS, correlation, OpenAPI, migrations).
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "Smoke")]
[Trait("Category", "Deployment")]
[Trait("Category", "ProductionReadiness")]
public sealed class ProductionReadinessSmokeTests : IntegrationTestClassBase
{
    private const string AllowedOrigin = "https://app.helpdev.smoke.test";

    public ProductionReadinessSmokeTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    protected override IReadOnlyDictionary<string, string?>? ConfigurationOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["Security:AllowedCorsOrigins:0"] = AllowedOrigin,
            ["Cors:FrontendOrigins:0"] = AllowedOrigin,
        };

    [PostgreSqlFact]
    public async Task Health_live_and_ready_are_available()
    {
        using var live = await Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);

        using var ready = await Client.GetAsync("/health/ready");
        Assert.True(
            ready.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected readiness status {(int)ready.StatusCode}.");
    }

    [PostgreSqlFact]
    public async Task Responses_include_security_and_correlation_headers()
    {
        using var response = await Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }

    [PostgreSqlFact]
    public async Task Allowed_cors_origin_receives_acao()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains(AllowedOrigin, values);
    }

    [PostgreSqlFact]
    public async Task Openapi_is_available_in_non_production_test_host()
    {
        using var response = await Client.GetAsync("/openapi/public-v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        Assert.True(document.RootElement.TryGetProperty("paths", out var paths));
        Assert.True(paths.TryGetProperty("/api/v1/auth/send-otp", out _));
    }

    [PostgreSqlFact]
    public async Task Migrations_apply_to_expected_count_with_module_tables()
    {
        var count = await PostgreSqlDatabaseHelper.GetAppliedMigrationCountAsync(ConnectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, count);

        var tables = await PostgreSqlDatabaseHelper.GetExistingModuleTablesAsync(ConnectionString);
        Assert.Equal(PostgreSqlDatabaseHelper.ExpectedModuleTables.Count, tables.Count);
        Assert.Contains("outbox_messages", tables);
        Assert.Contains("search_vectors", tables);
        Assert.Contains("learning_profiles", tables);
        Assert.Contains("ai_usage_records", tables);
        Assert.Contains("media_assets", tables);
    }
}
