using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Auth;

[Collection(PostgreSqlCollection.Name)]
public sealed class AuthenticationEndToEndTests : IntegrationTestClassBase
{
    public AuthenticationEndToEndTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Otp_flow_via_http_registers_user_and_returns_jwt()
    {
        const string mobile = "+989121234567";

        var sendResponse = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest
        {
            Mobile = mobile,
        });

        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);

        var sendPayload = await sendResponse.Content.ReadFromJsonAsync<SendOtpResponse>();
        Assert.NotNull(sendPayload);
        Assert.False(string.IsNullOrWhiteSpace(sendPayload!.Otp));
        Assert.Equal(6, sendPayload.Otp!.Length);

        var verifyResponse = await Client.PostAsJsonAsync("/api/auth/verify-otp", new VerifyOtpRequest
        {
            Mobile = mobile,
            Code = sendPayload.Otp,
        });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        using var document = JsonDocument.Parse(await verifyResponse.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("accessToken").GetString()));
        Assert.True(root.GetProperty("expiresIn").GetInt32() > 0);
        Assert.Equal("09121234567", root.GetProperty("user").GetProperty("mobile").GetString());
    }

    [PostgreSqlFact]
    public async Task Send_otp_with_invalid_mobile_returns_bad_request()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/send-otp", new SendOtpRequest
        {
            Mobile = "invalid",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
