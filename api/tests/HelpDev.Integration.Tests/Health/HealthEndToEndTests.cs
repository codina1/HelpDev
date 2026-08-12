using System.Net;
using System.Text.Json;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Health;

[Collection(PostgreSqlCollection.Name)]
public sealed class HealthEndToEndTests : IntegrationTestClassBase
{
    public HealthEndToEndTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Live_endpoint_does_not_require_database_connectivity()
    {
        var response = await Client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await DeserializeAsync(response);
        Assert.Equal("Healthy", payload.GetProperty("status").GetString());
    }

    [PostgreSqlFact]
    public async Task Ready_endpoint_reports_database_readiness()
    {
        var response = await Client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await DeserializeAsync(response);
        var status = payload.GetProperty("status").GetString();
        Assert.True(status is "Healthy" or "Degraded");
    }

    private static async Task<JsonElement> DeserializeAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }
}
