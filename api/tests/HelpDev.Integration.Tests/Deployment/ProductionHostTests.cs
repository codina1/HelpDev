using System.Net;
using HelpDev.API.Deployment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Deployment;

/// <summary>
/// Boots the real application under a Production environment to verify fail-fast behavior,
/// safe startup, and Production OpenAPI gating. Uses MigrationMode=None so no database is required.
/// </summary>
internal sealed class ProductionHostFactory : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _settings;

    public ProductionHostFactory(IReadOnlyDictionary<string, string?> settings)
    {
        _settings = settings;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(_settings);
        });
    }
}

[Trait("Category", "Integration")]
[Trait("Category", "ProductionSafety")]
[Trait("Category", "Deployment")]
public sealed class ProductionHostTests
{
    private const string StrongSecret = "Prod_Jwt_Signing_Key_0123456789abcdefghij";
    private const string StrongPartition = "Prod_Partition_Hmac_Key_0123456789zyxwvuts";

    private static Dictionary<string, string?> SafeSettings() => new()
    {
        ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Port=5432;Database=helpdev;Username=app;Password=StrongPw123",
        ["Database:MigrationMode"] = "None",
        ["Database:SeedMode"] = "None",
        ["Outbox:Enabled"] = "false",
        ["Jwt:Secret"] = StrongSecret,
        ["Security:PartitionHashKey"] = StrongPartition,
        ["Security:RequireHttpsMetadata"] = "true",
        ["Security:AllowedCorsOrigins:0"] = "https://app.example.com",
        ["Https:RedirectToHttps"] = "false",
        ["OpenApi:Enabled"] = "false",
        ["OpenApi:EnableUi"] = "false",
        ["OpenApi:EnableInProduction"] = "false",
    };

    [Fact]
    public void Unsafe_production_configuration_fails_startup()
    {
        // Both secrets are individually valid (strong, non-placeholder) so per-option validators pass,
        // but the centralized production safety validator rejects identical JWT and partition secrets.
        var settings = SafeSettings();
        settings["Security:PartitionHashKey"] = StrongSecret;
        settings["Jwt:Secret"] = StrongSecret;

        using var factory = new ProductionHostFactory(settings);

        var exception = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(exception);
        Assert.True(ContainsProductionSafetyFailure(exception!), exception!.ToString());
    }

    [Fact]
    public async Task Safe_production_configuration_starts_and_serves_liveness()
    {
        using var factory = new ProductionHostFactory(SafeSettings());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Production_openapi_json_is_unavailable_by_default()
    {
        using var factory = new ProductionHostFactory(SafeSettings());
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/openapi/public-v1.json");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public void Production_readiness_state_is_ready_after_startup()
    {
        using var factory = new ProductionHostFactory(SafeSettings());
        _ = factory.CreateClient();

        var readiness = factory.Services.GetRequiredService<HelpDev.SharedContracts.Observability.IApplicationReadinessState>();

        Assert.Equal(HelpDev.SharedContracts.Observability.ApplicationReadinessStatus.Ready, readiness.Status);
    }

    [Fact]
    public void Readiness_becomes_non_ready_during_shutdown()
    {
        using var factory = new ProductionHostFactory(SafeSettings());
        _ = factory.CreateClient();

        var readiness = factory.Services.GetRequiredService<HelpDev.SharedContracts.Observability.IApplicationReadinessState>();
        Assert.Equal(HelpDev.SharedContracts.Observability.ApplicationReadinessStatus.Ready, readiness.Status);

        var lifetime = factory.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
        lifetime.StopApplication();

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (readiness.IsAcceptingTraffic && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(25);
        }

        Assert.False(readiness.IsAcceptingTraffic);
        Assert.Equal(HelpDev.SharedContracts.Observability.ApplicationReadinessStatus.Stopping, readiness.Status);
    }

    private static bool ContainsProductionSafetyFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is ProductionSafetyValidationException)
            {
                return true;
            }
        }

        return false;
    }
}
