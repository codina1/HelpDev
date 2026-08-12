using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;
using Npgsql;

namespace HelpDev.Integration.Tests.PromptLab;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class PromptLabWorkflowE2ETests : IntegrationTestClassBase
{
    private const string PrivacySentinel = "PROMPT_VALUE_PRIVATE_SENTINEL";

    public PromptLabWorkflowE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Create_version_publish_enable_render_disable_workflow()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var categorySlug = TestIds.Truncate($"pl-wf-{Guid.NewGuid():N}", 16);
        var promptSlug = TestIds.Truncate($"hello-{Guid.NewGuid():N}", 16);

        using var categoryResponse = await admin.PostAsJsonAsync("/api/admin/prompt-lab/categories", new
        {
            name = "Prompt WF",
            slug = categorySlug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var categoryId = (await categoryResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        using var promptResponse = await admin.PostAsJsonAsync("/api/admin/prompt-lab/prompts", new
        {
            categoryId,
            name = "Hello Prompt",
            slug = promptSlug,
            summary = "Greets",
            description = (string?)null,
            purpose = "General",
            visibility = "Public",
            requiresAuthentication = false,
            allowHistory = true,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, promptResponse.StatusCode);
        var promptId = (await promptResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        using var versionResponse = await admin.PostAsJsonAsync($"/api/admin/prompt-lab/prompts/{promptId}/versions", new
        {
            template = "Hello {{name}}",
            changeNotes = (string?)null,
            variables = new[]
            {
                new
                {
                    name = "name",
                    label = "Name",
                    description = (string?)null,
                    type = "Text",
                    isRequired = true,
                    defaultValue = (string?)null,
                    minLength = (int?)null,
                    maxLength = (int?)null,
                    minValue = (decimal?)null,
                    maxValue = (decimal?)null,
                    validationPattern = (string?)null,
                    allowedValues = (string[]?)null,
                    displayOrder = 0,
                },
            },
        });
        Assert.Equal(HttpStatusCode.Created, versionResponse.StatusCode);
        var versionNumber = (await versionResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("versionNumber").GetInt32();

        using var publish = await admin.PostAsync(
            $"/api/admin/prompt-lab/prompts/{promptId}/versions/{versionNumber}/publish",
            null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        using var enable = await admin.PostAsync($"/api/admin/prompt-lab/prompts/{promptId}/enable", null);
        Assert.Equal(HttpStatusCode.OK, enable.StatusCode);

        using var render = await Client.PostAsJsonAsync($"/api/prompts/{promptSlug}/render", new
        {
            values = new Dictionary<string, string> { ["name"] = "HelpDev" },
        });
        Assert.Equal(HttpStatusCode.OK, render.StatusCode);

        using var disable = await admin.PostAsync($"/api/admin/prompt-lab/prompts/{promptId}/disable", null);
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);

        using var renderDisabled = await Client.PostAsJsonAsync($"/api/prompts/{promptSlug}/render", new
        {
            values = new Dictionary<string, string> { ["name"] = "HelpDev" },
        });
        Assert.True(
            renderDisabled.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound,
            $"Disabled render should fail; got {renderDisabled.StatusCode}");

        var created = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.PromptLabPromptCreated);
        var published = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.PromptLabVersionPublished);
        Assert.Contains(created, record => record.SubjectId == promptId);
        Assert.Contains(published, record => record.SubjectId == promptId);
    }

    [PostgreSqlFact]
    public async Task Category_create_writes_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        using var response = await admin.PostAsJsonAsync("/api/admin/prompt-lab/categories", new
        {
            name = "PL Audit",
            slug = TestIds.Truncate($"pl-a-{Guid.NewGuid():N}", 16),
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.PromptLabCategoryCreated);
        Assert.NotEmpty(audits);
    }

    [PostgreSqlFact]
    public async Task Render_privacy_sentinel_absent_from_audit_table()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var (promptSlug, _) = await SeedPublishedPromptAsync(admin);

        using var render = await Client.PostAsJsonAsync($"/api/prompts/{promptSlug}/render", new
        {
            values = new Dictionary<string, string> { ["name"] = PrivacySentinel },
        });
        Assert.Equal(HttpStatusCode.OK, render.StatusCode);

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)::int
            FROM audit_records
            WHERE metadata::text ILIKE @sentinel
               OR COALESCE(subject_display, '') ILIKE @sentinel
            """;
        command.Parameters.AddWithValue("sentinel", $"%{PrivacySentinel}%");
        var count = (int)(await command.ExecuteScalarAsync())!;
        Assert.Equal(0, count);
    }

    [PostgreSqlFact]
    public async Task Version_publish_produces_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var (_, promptId) = await SeedPublishedPromptAsync(admin);

        var published = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.PromptLabVersionPublished);
        Assert.Contains(published, record => record.SubjectId == promptId);
    }

    private static async Task<(string Slug, Guid PromptId)> SeedPublishedPromptAsync(HttpClient admin)
    {
        var categorySlug = TestIds.Truncate($"pl-s-{Guid.NewGuid():N}", 16);
        var promptSlug = TestIds.Truncate($"prm-{Guid.NewGuid():N}", 16);

        using var categoryResponse = await admin.PostAsJsonAsync("/api/admin/prompt-lab/categories", new
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

        using var promptResponse = await admin.PostAsJsonAsync("/api/admin/prompt-lab/prompts", new
        {
            categoryId,
            name = promptSlug,
            slug = promptSlug,
            summary = "summary",
            description = (string?)null,
            purpose = "General",
            visibility = "Public",
            requiresAuthentication = false,
            allowHistory = false,
            displayOrder = 0,
        });
        promptResponse.EnsureSuccessStatusCode();
        var promptId = (await promptResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        using var versionResponse = await admin.PostAsJsonAsync($"/api/admin/prompt-lab/prompts/{promptId}/versions", new
        {
            template = "Hello {{name}}",
            changeNotes = (string?)null,
            variables = new[]
            {
                new
                {
                    name = "name",
                    label = "Name",
                    description = (string?)null,
                    type = "Text",
                    isRequired = true,
                    defaultValue = (string?)null,
                    minLength = (int?)null,
                    maxLength = (int?)null,
                    minValue = (decimal?)null,
                    maxValue = (decimal?)null,
                    validationPattern = (string?)null,
                    allowedValues = (string[]?)null,
                    displayOrder = 0,
                },
            },
        });
        versionResponse.EnsureSuccessStatusCode();
        var versionNumber = (await versionResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("versionNumber").GetInt32();

        (await admin.PostAsync(
            $"/api/admin/prompt-lab/prompts/{promptId}/versions/{versionNumber}/publish",
            null)).EnsureSuccessStatusCode();
        (await admin.PostAsync($"/api/admin/prompt-lab/prompts/{promptId}/enable", null)).EnsureSuccessStatusCode();

        return (promptSlug, promptId);
    }
}
