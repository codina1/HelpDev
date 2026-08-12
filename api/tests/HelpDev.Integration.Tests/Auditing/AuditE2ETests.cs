using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Auditing;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class AuditE2ETests : IntegrationTestClassBase
{
    private const string CorrelationId = "CORR-SPRINT23-5-VALID";

    public AuditE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Feature_flag_create_writes_single_audit_with_correlation()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.sprint235.{Guid.NewGuid():N}", 40);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/features")
        {
            Content = JsonContent.Create(new
            {
                key,
                isEnabled = true,
                description = "sprint 23.5",
            }),
        };
        CorrelationAssertionHelper.SetCorrelationId(request, CorrelationId);

        using var response = await admin.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        CorrelationAssertionHelper.AssertEchoed(response, CorrelationId);

        var page = await AuditAssertionHelper.GetAuditPageAsync(
            admin,
            AuditActions.AdministrationFeatureFlagCreated);
        var item = AuditAssertionHelper.SingleItemByAction(
            page,
            AuditActions.AdministrationFeatureFlagCreated);

        Assert.Equal(CorrelationId, item.GetProperty("correlationId").GetString());
        Assert.Equal("POST", item.GetProperty("requestMethod").GetString());
        Assert.False(string.IsNullOrWhiteSpace(item.GetProperty("requestPathTemplate").GetString()));
        AuditAssertionHelper.AssertHasMetadataKeys(item, "key", "previousState", "newState");
        AuditAssertionHelper.AssertMetadataLacksSensitive(item, "09", "+98");
    }

    [PostgreSqlFact]
    public async Task Setting_create_audits_key_and_valueChanged_not_secret_value()
    {
        const string sentinel = "SETTING_SECRET_SENTINEL_23_5";
        using var admin = await AuthClients.CreateAdminClientAsync();
        Factory.ClearCapturedLogs();

        var key = TestIds.Truncate($"setting.sprint235.{Guid.NewGuid():N}", 40);
        using var response = await admin.PostAsJsonAsync("/api/admin/settings", new
        {
            key,
            value = sentinel,
            valueType = "String",
            description = "secret setting",
            isPublic = false,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var page = await AuditAssertionHelper.GetAuditPageAsync(
            admin,
            AuditActions.AdministrationSettingCreated);
        var item = AuditAssertionHelper.SingleItemByAction(
            page,
            AuditActions.AdministrationSettingCreated);

        AuditAssertionHelper.AssertHasMetadataKeys(item, "key", "valueChanged", "isPublic");
        Assert.Equal("true", item.GetProperty("metadata").GetProperty("valueChanged").GetString());
        Assert.DoesNotContain(sentinel, item.GetRawText(), StringComparison.Ordinal);
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, sentinel);
    }

    [PostgreSqlFact]
    public async Task Announcement_create_and_publish_write_audits()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();

        using var createResponse = await admin.PostAsJsonAsync("/api/admin/announcements", new
        {
            title = "Sprint 23.5 announcement",
            body = "Body",
            type = "Information",
            startsAtUtc = (DateTime?)null,
            endsAtUtc = (DateTime?)null,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        using var publishResponse = await admin.PostAsync($"/api/admin/announcements/{id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        var createdAudits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationAnnouncementCreated);
        var publishedAudits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationAnnouncementPublished);

        Assert.Single(createdAudits);
        Assert.Single(publishedAudits);
        Assert.Equal("POST", createdAudits[0].RequestMethod);
        Assert.False(string.IsNullOrWhiteSpace(createdAudits[0].RequestPathTemplate));
    }

    [PostgreSqlFact]
    public async Task Toolbox_category_create_writes_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var slug = TestIds.Truncate($"tb-cat-{Guid.NewGuid():N}", 20);

        using var response = await admin.PostAsJsonAsync("/api/admin/toolbox/categories", new
        {
            name = "Sprint Category",
            slug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.ToolboxCategoryCreated);
        Assert.Single(audits);
        Assert.Equal("POST", audits[0].RequestMethod);
        Assert.False(string.IsNullOrWhiteSpace(audits[0].RequestPathTemplate));
        AuditAssertionHelper.AssertNoSensitiveInRecord(audits[0]);
    }

    [PostgreSqlFact]
    public async Task PromptLab_category_create_writes_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var slug = TestIds.Truncate($"pl-cat-{Guid.NewGuid():N}", 20);

        using var response = await admin.PostAsJsonAsync("/api/admin/prompt-lab/categories", new
        {
            name = "Sprint Prompt Category",
            slug,
            description = (string?)null,
            icon = (string?)null,
            displayOrder = 0,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.PromptLabCategoryCreated);
        Assert.Single(audits);
        Assert.Equal("POST", audits[0].RequestMethod);
        Assert.False(string.IsNullOrWhiteSpace(audits[0].RequestPathTemplate));
    }

    [PostgreSqlFact]
    public async Task Feature_flag_audit_metadata_excludes_phone_jwt_otp()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.privacy.{Guid.NewGuid():N}", 40);

        using var response = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key,
            isEnabled = false,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var page = await AuditAssertionHelper.GetAuditPageAsync(
            admin,
            AuditActions.AdministrationFeatureFlagCreated);
        var item = AuditAssertionHelper.SingleItemByAction(
            page,
            AuditActions.AdministrationFeatureFlagCreated);

        var raw = item.GetRawText();
        Assert.DoesNotContain("otp", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("eyJ", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("+98", raw, StringComparison.Ordinal);
    }

    [PostgreSqlFact]
    public async Task Exactly_one_audit_per_feature_flag_create_action()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.FeatureKey("ff.once");

        using var response = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key,
            isEnabled = true,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationFeatureFlagCreated);
        Assert.Single(audits.Where(record => record.SubjectDisplay == key));
    }

    [PostgreSqlFact]
    public async Task Setting_update_marks_valueChanged_without_storing_value()
    {
        const string sentinel = "SETTING_SECRET_SENTINEL_23_5";
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"setting.update.{Guid.NewGuid():N}", 40);

        using var create = await admin.PostAsJsonAsync("/api/admin/settings", new
        {
            key,
            value = "initial",
            valueType = "String",
            description = (string?)null,
            isPublic = false,
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var update = await admin.PutAsJsonAsync($"/api/admin/settings/{key}", new
        {
            value = sentinel,
            description = (string?)null,
            isPublic = false,
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var page = await AuditAssertionHelper.GetAuditPageAsync(
            admin,
            AuditActions.AdministrationSettingUpdated);
        var item = AuditAssertionHelper.SingleItemByAction(
            page,
            AuditActions.AdministrationSettingUpdated);
        AuditAssertionHelper.AssertHasMetadataKeys(item, "key", "valueChanged");
        Assert.DoesNotContain(sentinel, item.GetRawText(), StringComparison.Ordinal);
    }
}
