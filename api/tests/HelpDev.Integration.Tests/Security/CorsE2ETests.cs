using System.Net;
using System.Net.Http.Headers;
using HelpDev.API.Security;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class CorsE2ETests : IntegrationTestClassBase
{
    private const string AllowedOrigin = "https://app.helpdev.test";

    public CorsE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    protected override IReadOnlyDictionary<string, string?>? ConfigurationOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["Security:AllowedCorsOrigins:0"] = AllowedOrigin,
            ["Cors:FrontendOrigins:0"] = AllowedOrigin,
        };

    [PostgreSqlFact]
    public async Task Allowed_origin_receives_access_control_allow_origin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains(AllowedOrigin, values);
    }

    [PostgreSqlFact]
    public async Task Evil_origin_does_not_receive_acao()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.TryAddWithoutValidation("Origin", "https://evil.example");

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [PostgreSqlFact]
    public async Task Options_preflight_for_allowed_origin_succeeds()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/content");
        request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Headers", "content-type");

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Contains(AllowedOrigin, values);
    }

    [PostgreSqlFact]
    public async Task Allowed_origin_on_api_tools_returns_acao()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/tools");
        request.Headers.TryAddWithoutValidation("Origin", AllowedOrigin);

        using var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            AllowedOrigin,
            response.Headers.GetValues("Access-Control-Allow-Origin"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Production_wildcard_cors_origin_fails_validation()
    {
        var validator = new SecurityOptionsValidator(new FakeHostEnvironment
        {
            EnvironmentName = Environments.Production,
        });

        var result = validator.Validate(
            null,
            new SecurityOptions
            {
                DefaultRequestBodyLimitBytes = 1024,
                MaxJsonRequestBodyLimitBytes = 1024,
                PartitionHashKey = "HelpDev_Integration_Partition_Hash_Key_32+",
                AllowedCorsOrigins = ["*"],
                RequireHttpsMetadata = true,
            });

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("Wildcard", StringComparison.OrdinalIgnoreCase)
            || failure.Contains("not a valid absolute URI", StringComparison.OrdinalIgnoreCase)
            || failure.Contains("CORS", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "HelpDev.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
