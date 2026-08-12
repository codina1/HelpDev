using System.Net;
using System.Net.Http.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class CorrelationPropagationE2ETests : IntegrationTestClassBase
{
    public CorrelationPropagationE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Valid_correlation_id_is_echoed_and_stored_in_audit()
    {
        const string correlationId = "CORR-SPRINT23-5-VALID";
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.corr.{Guid.NewGuid():N}", 40);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/features")
        {
            Content = JsonContent.Create(new
            {
                key,
                isEnabled = true,
                description = (string?)null,
            }),
        };
        CorrelationAssertionHelper.SetCorrelationId(request, correlationId);

        using var response = await admin.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        CorrelationAssertionHelper.AssertEchoed(response, correlationId);

        var audits = await AuditAssertionHelper.GetAuditRecordsFromDbAsync(
            Factory,
            AuditActions.AdministrationFeatureFlagCreated);
        var match = Assert.Single(audits.Where(record => record.SubjectDisplay == key));
        Assert.Equal(correlationId, match.CorrelationId);
    }

    [PostgreSqlFact]
    public async Task Invalid_correlation_id_with_newline_is_replaced_and_original_absent_from_logs()
    {
        const string invalid = "bad\ncorr-id";
        Factory.ClearCapturedLogs();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        CorrelationAssertionHelper.SetCorrelationId(request, invalid);

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var echoed = CorrelationAssertionHelper.GetCorrelationId(response);
        Assert.False(string.IsNullOrWhiteSpace(echoed));
        Assert.True(echoed != invalid);
        Assert.DoesNotContain('\n', echoed!);
        SensitiveLogAssertionHelper.AssertSentinelsAbsent(CapturedLogs, "bad\ncorr-id");
    }

    [PostgreSqlFact]
    public async Task Missing_correlation_id_generates_one()
    {
        using var response = await Client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        CorrelationAssertionHelper.AssertPresent(response);
        var generated = CorrelationAssertionHelper.GetCorrelationId(response)!;
        Assert.Matches("^[A-Za-z0-9._-]{1,100}$", generated);
    }
}
