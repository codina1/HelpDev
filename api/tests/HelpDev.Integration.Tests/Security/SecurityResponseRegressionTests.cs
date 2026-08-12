using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class SecurityResponseRegressionTests : IntegrationTestClassBase
{
    public SecurityResponseRegressionTests(PostgreSqlFixture fixture)
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
    public async Task Profile_me_anonymous_returns_401_with_correlation_and_security_headers()
    {
        using var response = await Client.GetAsync("/api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertSecurityHeaders(response);
        CorrelationAssertionHelper.AssertPresent(response);
        await AssertNoStackTraceAsync(response);
    }

    [PostgreSqlFact]
    public async Task Admin_audit_as_user_returns_403_json_without_stacktrace()
    {
        using var user = await AuthClients.CreateUserClientAsync();
        using var response = await user.GetAsync("/api/admin/audit");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertSecurityHeaders(response);
        CorrelationAssertionHelper.AssertPresent(response);
        await AssertNoStackTraceAsync(response);
    }

    [PostgreSqlFact]
    public async Task Oversized_body_returns_413_with_message_and_code()
    {
        var padding = new string('x', 17 * 1024);
        using var content = new StringContent(
            $$"""{"mobile":"+98912{{padding}}"}""",
            Encoding.UTF8,
            "application/json");

        using var response = await Client.PostAsync("/api/auth/send-otp", content);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("message", out _));
        Assert.Equal("security_request_too_large", document.RootElement.GetProperty("code").GetString());
        await AssertNoStackTraceAsync(response);
        CorrelationAssertionHelper.AssertPresent(response);
    }

    [PostgreSqlFact]
    public async Task Rate_limit_returns_429_with_message_and_code()
    {
        const string mobile = "+989121223344";
        HttpResponseMessage? rateLimited = null;

        for (var attempt = 0; attempt < 4; attempt++)
        {
            var response = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest { Mobile = mobile });
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rateLimited = response;
                break;
            }
        }

        Assert.NotNull(rateLimited);
        using var document = JsonDocument.Parse(await rateLimited!.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("message", out _));
        Assert.Equal("security_rate_limit_exceeded", document.RootElement.GetProperty("code").GetString());
        await AssertNoStackTraceAsync(rateLimited);
        CorrelationAssertionHelper.AssertPresent(rateLimited);
        AssertSecurityHeaders(rateLimited);
    }

    [PostgreSqlFact]
    public async Task Live_health_includes_security_headers()
    {
        using var response = await Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSecurityHeaders(response);
        CorrelationAssertionHelper.AssertPresent(response);
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
    }

    private static async Task AssertNoStackTraceAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("at HelpDev.", body, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception:", body, StringComparison.Ordinal);
    }
}
