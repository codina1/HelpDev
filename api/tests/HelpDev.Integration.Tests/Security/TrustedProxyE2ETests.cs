using System.Net;
using HelpDev.API.Security.RateLimiting;
using HelpDev.Testing.PostgreSQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Security;

/// <summary>
/// TestServer does not fully emulate reverse-proxy connection features.
/// These tests document that limitation and assert honest RemoteIp / partition behavior.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class TrustedProxyE2ETests : IntegrationTestClassBase
{
    public TrustedProxyE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    protected override IReadOnlyDictionary<string, string?>? ConfigurationOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["Security:TrustedProxyAddresses:0"] = "127.0.0.1",
            ["Security:TrustedProxyNetworks:0"] = "10.0.0.0/8",
        };

    [PostgreSqlFact]
    public void Forwarded_headers_configuration_sets_forward_limit_two()
    {
        // Documented contract in ForwardedHeadersConfiguration.Configure.
        Assert.Equal(2, GetConfiguredForwardLimit());
    }

    [PostgreSqlFact]
    public void Forwarded_headers_configuration_source_registers_trusted_proxies()
    {
        var source = File.ReadAllText(
            Path.Combine(
                FindRepoRoot(),
                "src",
                "HelpDev.API",
                "Security",
                "RateLimiting",
                "RateLimitPartitionKeyProvider.cs"));

        Assert.Contains("options.ForwardLimit = 2", source, StringComparison.Ordinal);
        Assert.Contains("TrustedProxyAddresses", source, StringComparison.Ordinal);
        Assert.Contains("TrustedProxyNetworks", source, StringComparison.Ordinal);
        Assert.Contains("KnownProxies", source, StringComparison.Ordinal);
    }

    [PostgreSqlFact]
    public void Without_mutating_remote_ip_x_forwarded_for_does_not_change_anonymous_partition()
    {
        // Partition key provider uses Connection.RemoteIpAddress only; X-Forwarded-For alone does not change it.
        // TestServer limitation: forwarded headers middleware will not rewrite RemoteIp unless the immediate
        // peer is in KnownProxies/KnownNetworks.
        using var scope = Factory.Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IRateLimitPartitionKeyProvider>();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.10");
        var baseline = provider.GetAnonymousNetworkPartition(context);

        context.Request.Headers["X-Forwarded-For"] = "198.51.100.20";
        var withHeader = provider.GetAnonymousNetworkPartition(context);

        Assert.Equal(baseline, withHeader);
    }

    [PostgreSqlFact]
    public async Task Rate_limit_still_succeeds_with_x_forwarded_for_header_present()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.50");

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static int GetConfiguredForwardLimit()
    {
        // Keep assertion honest without depending on ForwardedHeadersOptions visibility in the test TFM.
        var source = File.ReadAllText(
            Path.Combine(
                FindRepoRoot(),
                "src",
                "HelpDev.API",
                "Security",
                "RateLimiting",
                "RateLimitPartitionKeyProvider.cs"));
        const string marker = "options.ForwardLimit = ";
        var index = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(index >= 0);
        var start = index + marker.Length;
        var end = source.IndexOf(';', start);
        return int.Parse(source.AsSpan(start, end - start));
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "HelpDev.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate HelpDev.sln.");
    }
}
