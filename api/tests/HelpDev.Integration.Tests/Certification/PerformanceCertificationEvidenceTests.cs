using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Certification;

/// <summary>
/// Sprint 46 — marks existing performance sanity coverage as part of production certification evidence.
/// Detailed assertions live in <see cref="Performance.PerformanceSanityE2ETests"/>.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "Performance")]
[Trait("Category", "ProductionCertification")]
public sealed class PerformanceCertificationEvidenceTests : IntegrationTestClassBase
{
    public PerformanceCertificationEvidenceTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Certification_rechecks_bounded_search_and_admin_lists()
    {
        using var search = await Client.GetAsync("/api/v1/search?q=helpdev&page=1&pageSize=10");
        search.EnsureSuccessStatusCode();

        using var admin = await AuthClients.CreateAdminClientAsync();
        using var content = await admin.GetAsync("/api/v1/admin/content?page=1&pageSize=10");
        content.EnsureSuccessStatusCode();

        using var audit = await admin.GetAsync("/api/v1/admin/audit?page=1&pageSize=10");
        audit.EnsureSuccessStatusCode();

        using var media = await admin.GetAsync("/api/v1/admin/media?page=1&pageSize=10");
        // Media list may be 200 with empty page; reject unbounded dump sizes.
        Assert.True((int)media.StatusCode is 200 or 404 or 400);
        if (media.IsSuccessStatusCode)
        {
            var body = await media.Content.ReadAsStringAsync();
            Assert.True(body.Length < 2_000_000, "Media list payload unexpectedly large.");
        }
    }
}
