using System.Net;
using System.Net.Http.Json;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Auditing;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
public sealed class AuditFailurePolicyE2ETests : IntegrationTestClassBase
{
    public AuditFailurePolicyE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task FailNextWrite_allows_business_success_without_audit_and_logs_failure()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.fail.{Guid.NewGuid():N}", 40);
        Factory.ClearCapturedLogs();
        AuditFailureInjector.FailNextWrite("sprint-23.5-audit-fail");

        using var response = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key,
            isEnabled = true,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var flag = await context.FeatureFlags.AsNoTracking()
                .SingleOrDefaultAsync(item => item.Key == key);
            Assert.NotNull(flag);
        }

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationFeatureFlagCreated);
        Assert.DoesNotContain(audits, record => record.SubjectDisplay == key);

        Assert.Contains(
            CapturedLogs,
            entry => entry.Message.Contains("AuditPersistenceFailed", StringComparison.Ordinal)
                || string.Equals(entry.EventName, "AuditPersistenceFailed", StringComparison.Ordinal));
    }

    [PostgreSqlFact]
    public async Task Reset_allows_subsequent_create_to_audit()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var failingKey = TestIds.Truncate($"ff.fail2.{Guid.NewGuid():N}", 40);
        var successKey = TestIds.Truncate($"ff.ok.{Guid.NewGuid():N}", 40);

        AuditFailureInjector.FailNextWrite();
        using var failResponse = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key = failingKey,
            isEnabled = true,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, failResponse.StatusCode);

        AuditFailureInjector.Reset();

        using var okResponse = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key = successKey,
            isEnabled = true,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, okResponse.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationFeatureFlagCreated);
        Assert.Contains(audits, record => record.SubjectDisplay == successKey);
        Assert.DoesNotContain(audits, record => record.SubjectDisplay == failingKey);
    }

    [PostgreSqlFact]
    public async Task Injected_failure_is_single_shot()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var firstKey = TestIds.Truncate($"ff.once1.{Guid.NewGuid():N}", 40);
        var secondKey = TestIds.Truncate($"ff.once2.{Guid.NewGuid():N}", 40);

        AuditFailureInjector.FailNextWrite();
        using var first = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key = firstKey,
            isEnabled = false,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        using var second = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key = secondKey,
            isEnabled = false,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationFeatureFlagCreated);
        Assert.DoesNotContain(audits, record => record.SubjectDisplay == firstKey);
        Assert.Contains(audits, record => record.SubjectDisplay == secondKey);
    }
}
