using HelpDev.API.Deployment;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests.Deployment;

internal sealed class FakeHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = Environments.Production;
    public string ApplicationName { get; set; } = "HelpDev.API";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

/// <summary>
/// Builds a <see cref="ProductionSafetyValidator"/> with safe Production defaults so individual
/// tests can flip one setting to unsafe and assert the specific failure.
/// </summary>
internal sealed class ProductionSafetyValidatorBuilder
{
    public const string StrongJwtSecret = "S3cure_Prod_Jwt_Signing_Key_9x8y7z6w5v4u3t2s";
    public const string StrongPartitionKey = "P4rt1t10n_Hmac_Key_ab12cd34ef56gh78ij90klmn";

    public string Environment { get; set; } = Environments.Production;
    public string? ConnectionString { get; set; } =
        "Host=db;Port=5432;Database=helpdev;Username=app;Password=StrongDbPassword123";

    public JwtSettings Jwt { get; set; } = new()
    {
        Secret = StrongJwtSecret,
        Issuer = "HelpDev",
        Audience = "HelpDev.Client",
        ExpirationMinutes = 60,
    };

    public AuthSettings Auth { get; set; } = new() { ExposeOtpInResponse = false };

    public SecurityOptions Security { get; set; } = new()
    {
        PartitionHashKey = StrongPartitionKey,
        RequireHttpsMetadata = true,
        AllowedCorsOrigins = ["https://app.example.com"],
        DefaultRequestBodyLimitBytes = 256 * 1024,
        MaxJsonRequestBodyLimitBytes = 256 * 1024,
    };

    public OpenApiOptions OpenApi { get; set; } = new()
    {
        Enabled = false,
        EnableUi = false,
        EnableInProduction = false,
    };

    public ReverseProxyOptions ReverseProxy { get; set; } = new() { Enabled = false };

    public HttpsPolicyOptions Https { get; set; } = new();

    public DatabaseStartupOptions Database { get; set; } = new()
    {
        MigrationMode = DatabaseMigrationMode.Validate,
        SeedMode = DatabaseSeedMode.None,
    };

    public Dictionary<string, string?> ExtraConfiguration { get; } = new()
    {
        ["Logging:EnableSensitiveDataLogging"] = "false",
        ["Logging:EnableDetailedErrors"] = "false",
        ["Logging:LogLevel:Default"] = "Information",
    };

    public ProductionSafetyValidator Build()
    {
        var config = new Dictionary<string, string?>(ExtraConfiguration);
        if (ConnectionString is not null)
        {
            config["ConnectionStrings:DefaultConnection"] = ConnectionString;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        var environment = new FakeHostEnvironment { EnvironmentName = Environment };

        return new ProductionSafetyValidator(
            environment,
            configuration,
            Options.Create(Jwt),
            Options.Create(Auth),
            Options.Create(Security),
            Options.Create(OpenApi),
            Options.Create(ReverseProxy),
            Options.Create(Https),
            Options.Create(Database));
    }
}
