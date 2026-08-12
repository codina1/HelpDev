using System.Net;
using System.Net.Http.Json;
using System.Text;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class SecurityPipelineTests : IntegrationTestClassBase
{
    public SecurityPipelineTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    protected override IReadOnlyDictionary<string, string?>? ConfigurationOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["RateLimiting:General:PermitLimit"] = "2",
            ["RateLimiting:General:WindowSeconds"] = "10",
            ["RateLimiting:OtpRequest:PermitLimit"] = "2",
            ["RateLimiting:OtpRequest:WindowSeconds"] = "10",
            ["RateLimiting:OtpRequestNetwork:PermitLimit"] = "2",
            ["RateLimiting:OtpRequestNetwork:WindowSeconds"] = "10",
        };

    [PostgreSqlFact]
    public async Task Security_headers_are_applied_to_api_responses()
    {
        var response = await Client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [PostgreSqlFact]
    public async Task Oversized_request_body_returns_413()
    {
        var padding = new string('x', 17 * 1024);
        using var content = new StringContent(
            $$"""{"mobile":"+98912{{padding}}"}""",
            Encoding.UTF8,
            "application/json");

        var response = await Client.PostAsync("/api/auth/send-otp", content);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Otp_request_rate_limit_returns_429_with_short_window()
    {
        const string mobile = "+989121112233";
        HttpResponseMessage? rateLimitedResponse = null;

        for (var attempt = 0; attempt < 4; attempt++)
        {
            var response = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest { Mobile = mobile });
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimitedResponse = response;
                break;
            }
        }

        Assert.NotNull(rateLimitedResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse!.StatusCode);

        var body = await rateLimitedResponse.Content.ReadAsStringAsync();
        Assert.Contains("security_rate_limit_exceeded", body, StringComparison.Ordinal);
    }
}
