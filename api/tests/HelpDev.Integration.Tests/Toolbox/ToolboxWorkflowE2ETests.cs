using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HelpDev.Integration.Tests.Toolbox;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class ToolboxWorkflowE2ETests : IntegrationTestClassBase
{
    private const string PrivateSentinel = "TOOL_INPUT_PRIVATE_SENTINEL";

    public ToolboxWorkflowE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Admin_create_publish_enable_execute_disable_workflow()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var categorySlug = TestIds.Truncate($"tb-wf-{Guid.NewGuid():N}", 18);
        var toolSlug = TestIds.Truncate($"uuid-{Guid.NewGuid():N}", 18);

        using var categoryResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = "Workflow Category",
            slug = categorySlug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await categoryResponse.Content.ReadFromJsonAsync<JsonElement>();
        var categoryId = category.GetProperty("id").GetGuid();

        var inputSchema = """{"type":"object","properties":{"count":{"type":"integer"}}}""";
        using var toolResponse = await admin.PostAsJsonAsync("/api/admin/toolbox/tools", new
        {
            categoryId,
            name = "UUID Generator WF",
            slug = toolSlug,
            summary = "Generates UUIDs",
            description = (string?)null,
            type = "UuidGenerator",
            inputSchema,
            exampleInput = (string?)null,
            requiresAuthentication = false,
            allowHistory = true,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, toolResponse.StatusCode);
        var tool = await toolResponse.Content.ReadFromJsonAsync<JsonElement>();
        var toolId = tool.GetProperty("id").GetGuid();

        using var publish = await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        using var enable = await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/enable", null);
        Assert.Equal(HttpStatusCode.OK, enable.StatusCode);

        using var details = await Client.GetAsync($"/api/tools/{toolSlug}");
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);

        using var execute = await Client.PostAsJsonAsync($"/api/tools/{toolSlug}/execute", new
        {
            input = new { count = 1, secret = PrivateSentinel },
        });
        Assert.Equal(HttpStatusCode.OK, execute.StatusCode);

        using var disable = await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/disable", null);
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);

        using var executeDisabled = await Client.PostAsJsonAsync($"/api/tools/{toolSlug}/execute", new
        {
            input = new { count = 1 },
        });
        Assert.True(
            executeDisabled.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Disabled execute should fail; got {executeDisabled.StatusCode}");

        var createdAudits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.ToolboxToolCreated);
        var publishedAudits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.ToolboxToolPublished);
        Assert.Contains(createdAudits, record => record.SubjectId == toolId);
        Assert.Contains(publishedAudits, record => record.SubjectId == toolId);

        await AssertSentinelAbsentFromAuditTableAsync();
    }

    [PostgreSqlFact]
    public async Task Category_create_writes_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        using var response = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = "Audit Cat",
            slug = TestIds.Truncate($"tb-a-{Guid.NewGuid():N}", 16),
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 1,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.ToolboxCategoryCreated);
        Assert.NotEmpty(audits);
    }

    [PostgreSqlFact]
    public async Task Public_catalog_lists_published_enabled_tool()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var categorySlug = TestIds.Truncate($"tb-list-{Guid.NewGuid():N}", 16);
        var toolSlug = TestIds.Truncate($"list-{Guid.NewGuid():N}", 16);

        var categoryId = await CreateCategoryAsync(admin, categorySlug);
        var toolId = await CreateUuidToolAsync(admin, categoryId, toolSlug);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/publish", null)).StatusCode);

        using var response = await Client.GetAsync("/api/tools");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(toolSlug, body, StringComparison.Ordinal);
    }

    [PostgreSqlFact]
    public async Task Execute_input_sentinel_never_persisted_in_audit_records()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin, TestIds.Truncate($"tb-s-{Guid.NewGuid():N}", 16));
        var toolSlug = TestIds.Truncate($"sent-{Guid.NewGuid():N}", 16);
        var toolId = await CreateUuidToolAsync(admin, categoryId, toolSlug);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/toolbox/tools/{toolId}/publish", null)).StatusCode);

        using var execute = await Client.PostAsJsonAsync($"/api/tools/{toolSlug}/execute", new
        {
            input = new { count = 1, note = PrivateSentinel },
        });
        Assert.Equal(HttpStatusCode.OK, execute.StatusCode);
        await AssertSentinelAbsentFromAuditTableAsync();
    }

    private static async Task<Guid> CreateCategoryAsync(HttpClient admin, string slug)
    {
        using var response = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = slug,
            slug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateUuidToolAsync(HttpClient admin, Guid categoryId, string slug)
    {
        using var response = await admin.PostAsJsonAsync("/api/admin/toolbox/tools", new
        {
            categoryId,
            name = slug,
            slug,
            summary = "uuid",
            description = (string?)null,
            type = "UuidGenerator",
            inputSchema = """{"type":"object","properties":{"count":{"type":"integer"}}}""",
            exampleInput = (string?)null,
            requiresAuthentication = false,
            allowHistory = false,
            displayOrder = 0,
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetGuid();
    }

    private async Task AssertSentinelAbsentFromAuditTableAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)::int
            FROM audit_records
            WHERE metadata::text ILIKE @sentinel
               OR COALESCE(subject_display, '') ILIKE @sentinel
               OR COALESCE(reason_code, '') ILIKE @sentinel
            """;
        command.Parameters.AddWithValue("sentinel", $"%{PrivateSentinel}%");
        var count = (int)(await command.ExecuteScalarAsync())!;
        Assert.Equal(0, count);
    }
}
