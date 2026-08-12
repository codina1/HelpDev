using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.SharedContracts.Analytics;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.FailureIsolation;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class AnalyticsFailureIsolationE2ETests : IntegrationTestClassBase
{
    public AnalyticsFailureIsolationE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task FailNextIngestion_does_not_fail_otp_verify()
    {
        const string mobile = "+989121555001";
        using var send = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest { Mobile = mobile });
        send.EnsureSuccessStatusCode();
        var otp = (await send.Content.ReadFromJsonAsync<SendOtpResponse>())!.Otp!;

        AnalyticsFailureInjector.FailNextIngestion(AnalyticsEventTypes.IdentityUserRegistered);

        using var verify = await Client.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpRequest
        {
            Mobile = mobile,
            Code = otp,
        });
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
    }

    [PostgreSqlFact]
    public async Task FailNextIngestion_does_not_fail_toolbox_execute()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var toolSlug = await SeedUuidToolAsync(admin);

        AnalyticsFailureInjector.FailNextIngestion(AnalyticsEventTypes.ToolboxExecutionSucceeded);

        using var execute = await Client.PostAsJsonAsync($"/api/tools/{toolSlug}/execute", new
        {
            input = new { count = 1 },
        });
        Assert.Equal(HttpStatusCode.OK, execute.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Reset_clears_pending_analytics_failure()
    {
        AnalyticsFailureInjector.FailNextIngestion();
        AnalyticsFailureInjector.Reset();

        const string mobile = "+989121555002";
        using var send = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest { Mobile = mobile });
        send.EnsureSuccessStatusCode();
        var otp = (await send.Content.ReadFromJsonAsync<SendOtpResponse>())!.Otp!;

        using var verify = await Client.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpRequest
        {
            Mobile = mobile,
            Code = otp,
        });
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
    }

    private static async Task<string> SeedUuidToolAsync(HttpClient admin)
    {
        var categorySlug = TestIds.Truncate($"an-cat-{Guid.NewGuid():N}", 16);
        var toolSlug = TestIds.Truncate($"an-tool-{Guid.NewGuid():N}", 16);

        using var categoryResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = categorySlug,
            slug = categorySlug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        categoryResponse.EnsureSuccessStatusCode();
        var categoryId = (await categoryResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        using var toolResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/tools", new
        {
            categoryId,
            name = toolSlug,
            slug = toolSlug,
            summary = "uuid",
            description = (string?)null,
            type = "UuidGenerator",
            inputSchema = """{"type":"object","properties":{"count":{"type":"integer"}}}""",
            exampleInput = (string?)null,
            requiresAuthentication = false,
            allowHistory = false,
            displayOrder = 0,
        });
        toolResponse.EnsureSuccessStatusCode();
        var toolId = (await toolResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        (await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/publish", null)).EnsureSuccessStatusCode();
        return toolSlug;
    }
}
