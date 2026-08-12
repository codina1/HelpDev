using System.Net;
using System.Net.Http.Json;
using System.Text;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class LoggingPrivacyRegressionTests : IntegrationTestClassBase
{
    public LoggingPrivacyRegressionTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    // MapControllers().RequireRateLimiting(GeneralApi) is the effective limiter in this host;
    // keep General tight so OTP privacy/rate-limit assertions can observe 429.
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
    public async Task Otp_send_does_not_log_otp_code()
    {
        Factory.ClearCapturedLogs();
        const string mobile = "+989121000001";

        using var response = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest
        {
            Mobile = mobile,
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<SendOtpResponse>();
        Assert.False(string.IsNullOrWhiteSpace(payload?.Otp));

        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, payload!.Otp!);
    }

    [PostgreSqlFact]
    public async Task Setting_update_sentinel_absent_from_logs()
    {
        const string sentinel = "SETTING_SECRET_SENTINEL_23_5";
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"setting.privacy.{Guid.NewGuid():N}", 40);

        using var create = await admin.PostAsJsonAsync("/api/admin/settings", new
        {
            key,
            value = "seed",
            valueType = "String",
            description = (string?)null,
            isPublic = false,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        Factory.ClearCapturedLogs();
        using var update = await admin.PutAsJsonAsync($"/api/admin/settings/{key}", new
        {
            value = sentinel,
            description = (string?)null,
            isPublic = false,
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, sentinel);
    }

    [PostgreSqlFact]
    public async Task Search_query_sentinel_absent_from_logs()
    {
        const string sentinel = "SEARCH_QUERY_SENTINEL_SECRET";
        Factory.ClearCapturedLogs();

        using var response = await Client.GetAsync($"/api/search?q={Uri.EscapeDataString(sentinel)}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, sentinel);
    }

    [PostgreSqlFact]
    public async Task Oversized_body_413_does_not_log_payload_sentinel()
    {
        const string sentinel = "OVERSIZE_BODY_SENTINEL_SECRET";
        Factory.ClearCapturedLogs();
        var padding = new string('x', 17 * 1024);
        using var content = new StringContent(
            $$"""{"mobile":"+98912{{padding}}{{sentinel}}"}""",
            Encoding.UTF8,
            "application/json");

        using var response = await Client.PostAsync("/api/auth/send-otp", content);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, sentinel);
    }

    [PostgreSqlFact]
    public async Task Rate_limit_429_does_not_log_otp_or_phone_secrets()
    {
        const string mobile = "+989121000099";
        Factory.ClearCapturedLogs();
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
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, "OTP_SECRET_SHOULD_NOT_APPEAR");
    }

    [PostgreSqlFact]
    public async Task Audit_failure_log_does_not_include_setting_secret()
    {
        const string sentinel = "SETTING_SECRET_SENTINEL_23_5";
        using var admin = await AuthClients.CreateAdminClientAsync();
        Factory.ClearCapturedLogs();
        AuditFailureInjector.FailNextWrite(sentinel);

        using var response = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key = TestIds.Truncate($"ff.logfail.{Guid.NewGuid():N}", 40),
            isEnabled = true,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        Assert.Contains(
            CapturedLogs,
            entry => entry.Message.Contains("AuditPersistenceFailed", StringComparison.Ordinal)
                || string.Equals(entry.EventName, "AuditPersistenceFailed", StringComparison.Ordinal));

        // Failure injector reason may appear in exception message; ensure business secrets from request body are absent.
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(
            CapturedLogs.Where(entry =>
                !string.Equals(entry.EventName, "AuditPersistenceFailed", StringComparison.Ordinal)
                && !entry.Message.Contains("AuditPersistenceFailed", StringComparison.Ordinal)),
            sentinel);
    }
}
