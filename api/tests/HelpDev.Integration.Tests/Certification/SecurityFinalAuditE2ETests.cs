using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Certification;

/// <summary>
/// Sprint 46 — final security audit: authz matrix smoke + no JWT/secret/prompt/vector leakage in responses/logs.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "Security")]
[Trait("Category", "ProductionCertification")]
public sealed class SecurityFinalAuditE2ETests : IntegrationTestClassBase
{
    public SecurityFinalAuditE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Otp_flow_issues_jwt_without_leaking_secrets_in_logs()
    {
        var mobile = $"+98913{Random.Shared.Next(1000000, 9999999)}";
        var send = await Client.PostAsJsonAsync("/api/v1/auth/send-otp", new SendOtpRequest { Mobile = mobile });
        Assert.Equal(HttpStatusCode.OK, send.StatusCode);
        var sendPayload = await send.Content.ReadFromJsonAsync<SendOtpResponse>();
        Assert.False(string.IsNullOrWhiteSpace(sendPayload?.Otp));

        var verify = await Client.PostAsJsonAsync("/api/v1/auth/verify-otp", new VerifyOtpRequest
        {
            Mobile = mobile,
            Code = sendPayload!.Otp!,
        });
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
        var body = await verify.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var token = doc.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        Assert.DoesNotContain("sk-", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PartitionHashKey", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);

        SensitiveLogAssertionHelper.AssertSentinelsAbsent(
            CapturedLogs,
            "sk-",
            "Bearer " + token!,
            "ApiKey",
            "PartitionHashKey");
    }

    [Theory]
    [InlineData("Anonymous", "GET", "/api/v1/admin/audit", HttpStatusCode.Unauthorized)]
    [InlineData("User", "GET", "/api/v1/admin/audit", HttpStatusCode.Forbidden)]
    [InlineData("Writer", "GET", "/api/v1/admin/audit", HttpStatusCode.Forbidden)]
    [InlineData("Admin", "GET", "/api/v1/admin/audit", HttpStatusCode.OK)]
    [InlineData("Anonymous", "GET", "/api/v1/admin/content", HttpStatusCode.Unauthorized)]
    [InlineData("User", "GET", "/api/v1/admin/content", HttpStatusCode.Forbidden)]
    [InlineData("Writer", "GET", "/api/v1/admin/content", HttpStatusCode.OK)]
    [InlineData("Admin", "GET", "/api/v1/admin/content", HttpStatusCode.OK)]
    [InlineData("Anonymous", "GET", "/api/v1/profile/me", HttpStatusCode.Unauthorized)]
    [InlineData("User", "GET", "/api/v1/profile/me", HttpStatusCode.OK)]
    public async Task Authorization_matrix_holds_for_critical_surfaces(
        string role,
        string method,
        string path,
        HttpStatusCode expected)
    {
        using var client = role switch
        {
            "Anonymous" => AuthClients.CreateAnonymousClient(),
            "User" => await AuthClients.CreateUserClientAsync(),
            "Writer" => await AuthClients.CreateWriterClientAsync(),
            "Admin" => await AuthClients.CreateAdminClientAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };

        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"accessToken\"", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer ", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"embedding\"", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"vector\"", payload, StringComparison.OrdinalIgnoreCase);
    }

    [PostgreSqlFact]
    public async Task Public_search_and_content_do_not_expose_vectors_or_prompts()
    {
        var (writer, writerId) = await AuthClients.CreateWriterClientWithIdAsync();
        using (writer)
        {
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var content = scope.ServiceProvider.GetRequiredService<IContentService>();
                await content.CreateAsync(
                    writerId,
                    new CreateContentRequest
                    {
                        Title = "Security Audit Draft",
                        Slug = $"sec-audit-{Guid.NewGuid():N}"[..40],
                        Body = "Public surface must not leak embeddings or system prompts.",
                        Type = nameof(ContentType.Article),
                        Status = nameof(ContentStatus.Draft),
                    });
            }

            using var search = await Client.GetAsync("/api/v1/search?q=postgresql&page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, search.StatusCode);
            var searchBody = await search.Content.ReadAsStringAsync();
            AssertNoSensitivePayload(searchBody);

            using var contentList = await Client.GetAsync("/api/v1/content?page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, contentList.StatusCode);
            AssertNoSensitivePayload(await contentList.Content.ReadAsStringAsync());

            using var health = await Client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, health.StatusCode);
            AssertNoSensitivePayload(await health.Content.ReadAsStringAsync());
        }
    }

    private static void AssertNoSensitivePayload(string payload)
    {
        Assert.DoesNotContain("sk-", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Bearer ", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("system prompt", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"embedding\"", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"vector\"", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PartitionHashKey", payload, StringComparison.OrdinalIgnoreCase);

        // Raw float arrays typical of vector dumps should not appear.
        Assert.False(
            payload.Contains("[0.", StringComparison.Ordinal) && payload.Contains(",0.", StringComparison.Ordinal)
            && Encoding.UTF8.GetByteCount(payload) > 50_000,
            "Response resembles an unbounded vector dump.");
    }
}
