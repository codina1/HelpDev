using System.Net;
using System.Text.Json;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Smoke;

/// <summary>
/// Read-only smoke suite that exercises the deployed contract surface without performing
/// destructive mutations. Safe to run against a production-like host.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "Smoke")]
[Trait("Category", "Deployment")]
public sealed class ProductionSmokeTests : IntegrationTestClassBase
{
    public ProductionSmokeTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Liveness_probe_returns_ok()
    {
        using var response = await Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Readiness_probe_returns_expected_state()
    {
        using var response = await Client.GetAsync("/health/ready");
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected readiness status {(int)response.StatusCode}.");
    }

    [PostgreSqlFact]
    public async Task Legacy_health_endpoint_is_available()
    {
        using var response = await Client.GetAsync("/api/health");
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.ServiceUnavailable,
            $"Unexpected legacy health status {(int)response.StatusCode}.");
    }

    [PostgreSqlFact]
    public async Task Public_responses_include_correlation_and_security_headers()
    {
        using var response = await Client.GetAsync("/health/live");

        Assert.True(response.Headers.Contains("X-Correlation-ID"), "Correlation header missing.");
        Assert.True(response.Headers.Contains("X-Content-Type-Options"), "Security header missing.");
    }

    [PostgreSqlFact]
    public async Task Public_health_response_is_status_only()
    {
        using var response = await Client.GetAsync("/health/live");
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var properties = document.RootElement.EnumerateObject().Select(p => p.Name).ToList();
        Assert.Equal(["status"], properties);
    }

    [PostgreSqlFact]
    public async Task Otp_request_returns_safe_response()
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(new { mobile = "09120000009" }),
            System.Text.Encoding.UTF8,
            "application/json");

        using var response = await Client.PostAsync("/api/v1/auth/send-otp", content);

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.TooManyRequests,
            $"Unexpected OTP response status {(int)response.StatusCode}.");
    }

    [PostgreSqlFact]
    public async Task OpenApi_document_is_available_outside_production()
    {
        using var response = await Client.GetAsync("/openapi/public-v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
