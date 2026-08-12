using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Pagination;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class PaginationConsistencyE2ETests : IntegrationTestClassBase
{
    public PaginationConsistencyE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Admin_toolbox_tools_pages_have_no_duplicate_ids()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var categoryId = await SeedToolboxToolsAsync(admin, count: 5);

        using var page1Response = await admin.GetAsync("/api/admin/toolbox/tools?page=1&pageSize=2");
        using var page2Response = await admin.GetAsync("/api/admin/toolbox/tools?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        var page1 = await page1Response.Content.ReadFromJsonAsync<JsonElement>();
        var page2 = await page2Response.Content.ReadFromJsonAsync<JsonElement>();
        PaginationAssertionHelper.AssertNoDuplicateIdsAcrossPages(page1, page2);
        Assert.True(categoryId != Guid.Empty);
    }

    [PostgreSqlFact]
    public async Task Admin_announcements_pages_have_no_duplicate_ids()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        await SeedAnnouncementsAsync(admin, count: 5);

        using var page1Response = await admin.GetAsync("/api/admin/announcements?page=1&pageSize=2");
        using var page2Response = await admin.GetAsync("/api/admin/announcements?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        var page1 = await page1Response.Content.ReadFromJsonAsync<JsonElement>();
        var page2 = await page2Response.Content.ReadFromJsonAsync<JsonElement>();
        PaginationAssertionHelper.AssertNoDuplicateIdsAcrossPages(page1, page2);
    }

    [PostgreSqlFact]
    public async Task Admin_audit_pages_have_no_duplicate_ids()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        for (var i = 0; i < 5; i++)
        {
            using var create = await admin.PostAsJsonAsync("/api/admin/features", new
            {
                key = TestIds.Truncate($"ff.page.{Guid.NewGuid():N}", 40),
                isEnabled = i % 2 == 0,
                description = (string?)null,
            });
            create.EnsureSuccessStatusCode();
        }

        using var page1Response = await admin.GetAsync("/api/admin/audit?page=1&pageSize=2");
        using var page2Response = await admin.GetAsync("/api/admin/audit?page=2&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, page1Response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, page2Response.StatusCode);

        var page1 = await page1Response.Content.ReadFromJsonAsync<JsonElement>();
        var page2 = await page2Response.Content.ReadFromJsonAsync<JsonElement>();
        PaginationAssertionHelper.AssertNoDuplicateIdsAcrossPages(page1, page2);
    }

    [PostgreSqlFact]
    public async Task Invalid_page_size_101_returns_400_for_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        using var response = await admin.GetAsync("/api/admin/audit?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Invalid_page_size_101_returns_400_for_toolbox_tools()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        using var response = await admin.GetAsync("/api/admin/toolbox/tools?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Content_list_is_non_paginated_array()
    {
        // Documented: GET /api/content returns a flat list, not a page DTO.
        using var response = await Client.GetAsync("/api/content");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    private static async Task<Guid> SeedToolboxToolsAsync(HttpClient admin, int count)
    {
        var categorySlug = TestIds.Truncate($"pg-cat-{Guid.NewGuid():N}", 16);
        using var categoryResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = "Pagination Category",
            slug = categorySlug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        categoryResponse.EnsureSuccessStatusCode();
        var categoryId = (await categoryResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        for (var i = 0; i < count; i++)
        {
            var slug = TestIds.Truncate($"pg-tool-{i}-{Guid.NewGuid():N}", 20);
            using var toolResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/tools", new
            {
                categoryId,
                name = $"Tool {i}",
                slug,
                summary = "summary",
                description = (string?)null,
                type = "UuidGenerator",
                inputSchema = """{"type":"object","properties":{"count":{"type":"integer"}}}""",
                exampleInput = (string?)null,
                requiresAuthentication = false,
                allowHistory = false,
                displayOrder = i,
            });
            toolResponse.EnsureSuccessStatusCode();
        }

        return categoryId;
    }

    private static async Task SeedAnnouncementsAsync(HttpClient admin, int count)
    {
        for (var i = 0; i < count; i++)
        {
            using var response = await admin.PostAsJsonAsync("/api/admin/announcements", new
            {
                title = $"Announcement {i} {Guid.NewGuid():N}",
                body = "Body",
                type = "Information",
                startsAtUtc = (DateTime?)null,
                endsAtUtc = (DateTime?)null,
            });
            response.EnsureSuccessStatusCode();
        }
    }
}
