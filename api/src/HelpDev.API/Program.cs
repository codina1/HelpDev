using HelpDev.Application;
using HelpDev.Infrastructure;
using HelpDev.Infrastructure.Ai;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Administration;
using HelpDev.Modules.Auditing;
using HelpDev.Modules.Content;
using HelpDev.Modules.Identity;
using HelpDev.Modules.Learning;
using HelpDev.Modules.Search;
using HelpDev.Modules.Toolbox;
using HelpDev.Modules.PromptLab;
using HelpDev.Modules.Analytics;
using HelpDev.Modules.Media;
using HelpDev.SharedInfrastructure;
using HelpDev.API.Deployment;
using HelpDev.API.Observability;
using HelpDev.API.Extensions;
using HelpDev.API.Filters;
using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.API.Security.RateLimiting;
using HelpDev.SharedContracts.Auditing;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var exportOpenApiDirectory = TryGetExportOpenApiDirectory(args);
var isOpenApiExport = exportOpenApiDirectory is not null;

var deploymentCommand = DeploymentCommandParser.Parse(args);

// The release manifest is emitted without building the host: no configuration, database, or
// service registration is required, and no secrets are read.
if (deploymentCommand is { Kind: DeploymentCommandKind.EmitReleaseManifest })
{
    return await DeploymentCommands.EmitReleaseManifestAsync(deploymentCommand);
}

// Config-validation and controlled-migration commands run against Production configuration so the
// same safety rules that gate startup are enforced by the command.
var forcedEnvironmentName =
    isOpenApiExport ? Environments.Development
    : deploymentCommand is not null ? Environments.Production
    : null;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = forcedEnvironmentName,
});

if (isOpenApiExport)
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["OpenApi:Enabled"] = "true",
        ["OpenApi:EnableUi"] = "false",
        ["OpenApi:EnableInProduction"] = "false",
        ["ConnectionStrings:DefaultConnection"] =
            "Host=127.0.0.1;Port=1;Database=helpdev_openapi_export;Username=openapi;Password=openapi;Pooling=false",
    });
}

// Some required values are hard registration failures (they throw before the host is built). For
// deployment commands we detect the common "missing" cases first and report safe, aggregated
// configuration errors, then exit non-zero, instead of surfacing a raw stack trace.
if (deploymentCommand is not null)
{
    var missingConfiguration = new List<string>();

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    {
        missingConfiguration.Add("PostgreSQL connection is not configured.");
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Secret"]))
    {
        missingConfiguration.Add("JWT signing key is missing.");
    }

    if (string.IsNullOrWhiteSpace(builder.Configuration["Security:PartitionHashKey"]))
    {
        missingConfiguration.Add("Security partition HMAC key is missing.");
    }

    if (missingConfiguration.Count > 0)
    {
        foreach (var missing in missingConfiguration)
        {
            Console.Error.WriteLine($"[config] ERROR: {missing}");
        }

        Console.Error.WriteLine($"[config] Production safety validation FAILED with {missingConfiguration.Count} error(s).");
        return DeploymentCommands.ExitFailure;
    }
}

ForwardedHeadersConfiguration.Configure(
    builder,
    builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ContentExceptionFilter>();
    options.Filters.Add<CourseExceptionFilter>();
    options.Filters.Add<EnrollmentExceptionFilter>();
    options.Filters.Add<LearningPersonalizationExceptionFilter>();
    options.Filters.Add<SearchExceptionFilter>();
    options.Filters.Add<OutboxOperationsExceptionFilter>();
    options.Filters.Add<AdministrationExceptionFilter>();
    options.Filters.Add<ToolboxExceptionFilter>();
    options.Filters.Add<PromptLabExceptionFilter>();
    options.Filters.Add<AnalyticsExceptionFilter>();
    options.Filters.Add<AuditExceptionFilter>();
    options.Filters.Add<MediaExceptionFilter>();
    options.Conventions.Add(new AdminRateLimitConvention());
    options.Conventions.Add(new PublicContentRateLimitConvention());
});

builder.Services.AddScoped<ContentExceptionFilter>();
builder.Services.AddScoped<CourseExceptionFilter>();
builder.Services.AddScoped<EnrollmentExceptionFilter>();
builder.Services.AddScoped<LearningPersonalizationExceptionFilter>();
builder.Services.AddScoped<SearchExceptionFilter>();
builder.Services.AddScoped<OutboxOperationsExceptionFilter>();
builder.Services.AddScoped<AdministrationExceptionFilter>();
builder.Services.AddScoped<ToolboxExceptionFilter>();
builder.Services.AddScoped<PromptLabExceptionFilter>();
builder.Services.AddScoped<AnalyticsExceptionFilter>();
builder.Services.AddScoped<AuditExceptionFilter>();
builder.Services.AddScoped<MediaExceptionFilter>();

builder.Services.AddHelpDevApiVersioning();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHelpDevOpenApi(builder.Configuration, builder.Environment);
builder.Services.AddHttpContextAccessor();

builder.Services.AddSharedInfrastructure();
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddContentModule();
builder.Services.AddLearningModule();
builder.Services.AddSearchModule();
builder.Services.AddAdministrationModule();
builder.Services.AddToolboxModule();
builder.Services.AddPromptLabModule();
builder.Services.AddAnalyticsModule(builder.Configuration);
builder.Services.AddAuditingModule(builder.Configuration);
builder.Services.AddMediaModule(builder.Configuration);
builder.Services.AddAiInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSecurityHardening(builder.Configuration, builder.Environment);
builder.Services.AddHelpDevObservability(builder.Configuration, builder.Environment);
builder.Services.AddHelpDevDeployment(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHelpDevAuthorization();

var securityOptions = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
if (securityOptions.AllowedCorsOrigins.Length == 0)
{
    securityOptions.AllowedCorsOrigins = builder.Configuration
        .GetSection("Cors:FrontendOrigins")
        .Get<string[]>() ?? ["http://localhost:3000"];
}

builder.Services.AddSingleton(securityOptions);
builder.Services.AddHelpDevCors(securityOptions);
builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(sp => sp.GetRequiredService<CorrelationContext>());
builder.Services.AddScoped<IAuditRequestContext, AuditRequestContext>();

if (deploymentCommand is not null)
{
    WebApplication commandApp;
    try
    {
        commandApp = builder.Build();
    }
    catch (OptionsValidationException optionsValidationException)
    {
        foreach (var failure in optionsValidationException.Failures)
        {
            Console.Error.WriteLine($"[config] ERROR: {failure}");
        }

        return DeploymentCommands.ExitFailure;
    }

    return await DeploymentCommands.RunAsync(commandApp, deploymentCommand, args);
}

var app = builder.Build();

if (!isOpenApiExport)
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    startupLogger.LogInformation("Event={Event}", DeploymentLogEvents.ApplicationStarting);

    // Production safety validation runs before any traffic is served. Bypass only for the test host.
    app.ValidateProductionSafety(bypass: app.Environment.IsEnvironment("Testing"));

    await DatabaseStartupManager.RunAsync(app.Services, app.Environment);

    app.RegisterReadinessLifecycle();
}

app.UseForwardedHeaders();
app.UseHelpDevOpenApi();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHelpDevProductionHardening();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("HelpDevCors");
app.UseMiddleware<RequestSizeLimitMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseMiddleware<AccessDeniedAuditMiddleware>();

app.MapControllers().RequireRateLimiting(RateLimitPolicyNames.GeneralApi);
app.MapHelpDevHealthEndpoints();

app.MapGet("/api/health", async (
    IDatabaseConnectionChecker database,
    CancellationToken cancellationToken) =>
{
    var databaseConnected = await database.CanConnectAsync(cancellationToken);
    var payload = new
    {
        status = databaseConnected ? "Healthy" : "Degraded",
        service = "HelpDev API",
        database = new
        {
            provider = "PostgreSQL",
            connected = databaseConnected,
        },
    };

    return databaseConnected
        ? Results.Ok(payload)
        : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
})
.WithName("LegacyHealthCheck")
.AllowAnonymous()
.DisableRateLimiting();

if (isOpenApiExport)
{
    var configuredExport = app.Services.GetRequiredService<IOptions<OpenApiOptions>>().Value.ExportDirectory;
    var outputDir = Path.GetFullPath(
        Path.Combine(app.Environment.ContentRootPath, "..", "..", "..", exportOpenApiDirectory ?? configuredExport));
    // Prefer explicit CLI directory relative to solution/api root when provided.
    if (!Path.IsPathRooted(exportOpenApiDirectory!))
    {
        var apiRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", ".."));
        outputDir = Path.GetFullPath(Path.Combine(apiRoot, exportOpenApiDirectory!));
    }
    else
    {
        outputDir = exportOpenApiDirectory!;
    }

    Directory.CreateDirectory(outputDir);
    var exitCode = await OpenApiExport.ExportAsync(app, outputDir);
    Console.WriteLine($"OpenAPI documents exported to {outputDir}");
    return exitCode;
}

app.Run();
return 0;

static string? TryGetExportOpenApiDirectory(string[] arguments)
{
    for (var i = 0; i < arguments.Length; i++)
    {
        if (!string.Equals(arguments[i], "--export-openapi", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            return arguments[i + 1];
        }

        return "artifacts/openapi";
    }

    return null;
}

public partial class Program;
