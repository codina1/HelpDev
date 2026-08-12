using System.Net;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Performance;

/// <summary>
/// Sprint 44 — lightweight performance sanity checks (bounded lists, pagination, no full-table dumps).
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Performance")]
public sealed class PerformanceSanityE2ETests : IntegrationTestClassBase
{
    public PerformanceSanityE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Admin_content_list_respects_page_size_for_large_catalog()
    {
        var (admin, adminId) = await AuthClients.CreateAdminClientWithIdAsync();
        using (admin)
        {
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var content = scope.ServiceProvider.GetRequiredService<IContentService>();
                for (var i = 0; i < 12; i++)
                {
                    await content.CreateAsync(
                        adminId,
                        new CreateContentRequest
                        {
                            Title = $"Perf Article {i}",
                            Slug = $"perf-article-{i}-{Guid.NewGuid():N}"[..40],
                            Body = "Performance sanity body " + i,
                            Type = nameof(ContentType.Article),
                            Status = nameof(ContentStatus.Draft),
                        });
                }
            }

            using var response = await admin.GetAsync("/api/v1/admin/content?page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var items = document.RootElement.GetProperty("items");
            Assert.Equal(JsonValueKind.Array, items.ValueKind);
            Assert.True(items.GetArrayLength() <= 5);
            Assert.True(document.RootElement.GetProperty("totalCount").GetInt32() >= 12);
            Assert.Equal(5, document.RootElement.GetProperty("pageSize").GetInt32());
        }
    }

    [PostgreSqlFact]
    public async Task Search_query_returns_bounded_page()
    {
        using var response = await Client.GetAsync("/api/v1/search?q=helpdev&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        Assert.True(
            document.RootElement.TryGetProperty("items", out var items)
            || document.RootElement.TryGetProperty("results", out items));
        Assert.True(items.GetArrayLength() <= 10);
    }

    [PostgreSqlFact]
    public async Task Admin_dashboard_endpoints_respond_without_unbounded_payload()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();

        using var ops = await admin.GetAsync("/api/v1/admin/operations/status");
        Assert.Equal(HttpStatusCode.OK, ops.StatusCode);

        using var features = await admin.GetAsync("/api/v1/admin/features");
        Assert.Equal(HttpStatusCode.OK, features.StatusCode);
        var payload = await features.Content.ReadAsStringAsync();
        Assert.True(payload.Length < 2_000_000, "Admin features payload unexpectedly large.");
    }

    [PostgreSqlFact]
    public async Task Oversized_page_size_is_clamped_or_rejected()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();

        // Content clamps to MaxPageSize=100 (no 400).
        using var contentResponse = await admin.GetAsync("/api/v1/admin/content?page=1&pageSize=500");
        Assert.Equal(HttpStatusCode.OK, contentResponse.StatusCode);
        await using var stream = await contentResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        Assert.Equal(100, document.RootElement.GetProperty("pageSize").GetInt32());
        Assert.True(document.RootElement.GetProperty("items").GetArrayLength() <= 100);

        // Audit rejects oversized page sizes.
        using var auditResponse = await admin.GetAsync("/api/v1/admin/audit?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, auditResponse.StatusCode);
    }
}
