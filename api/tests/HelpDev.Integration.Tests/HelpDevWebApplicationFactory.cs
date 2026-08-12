using System.Collections.Concurrent;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedContracts.Auditing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HelpDev.Integration.Tests;

public sealed class HelpDevWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<IServiceCollection>? _configureTestServices;
    private readonly IReadOnlyDictionary<string, string?>? _configurationOverrides;
    private readonly ConcurrentBag<CapturedLogEntry> _capturedLogs = new();
    private readonly TestAuditPersistenceFailureInjector _auditFailureInjector = new();
    private readonly TestAnalyticsFailureInjector _analyticsFailureInjector = new();
    private readonly CapturingLoggerProvider _loggerProvider;

    public HelpDevWebApplicationFactory(
        string connectionString,
        Action<IServiceCollection>? configureTestServices = null,
        IReadOnlyDictionary<string, string?>? configurationOverrides = null)
    {
        ConnectionString = connectionString;
        _configureTestServices = configureTestServices;
        _configurationOverrides = configurationOverrides;
        _loggerProvider = new CapturingLoggerProvider(_capturedLogs);
    }

    public string ConnectionString { get; }

    public ConcurrentBag<CapturedLogEntry> CapturedLogs => _capturedLogs;

    public TestAuditPersistenceFailureInjector AuditFailureInjector => _auditFailureInjector;

    public TestAnalyticsFailureInjector AnalyticsFailureInjector => _analyticsFailureInjector;

    public void ClearCapturedLogs() => _capturedLogs.Clear();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Auth:ExposeOtpInResponse", "true");

        foreach (var (key, value) in CreateTestConfiguration(ConnectionString))
        {
            if (value is not null)
            {
                builder.UseSetting(key, value);
            }
        }

        if (_configurationOverrides is not null)
        {
            foreach (var (key, value) in _configurationOverrides)
            {
                if (value is not null)
                {
                    builder.UseSetting(key, value);
                }
            }
        }

        builder.ConfigureLogging(logging =>
        {
            logging.AddProvider(_loggerProvider);
        });

        builder.ConfigureTestServices(services =>
        {
            RemoveHostedService<OutboxProcessor>(services);
            services.AddSingleton<OutboxProcessor>();

            services.RemoveAll<IAuditPersistenceFailureInjector>();
            services.AddSingleton<IAuditPersistenceFailureInjector>(_auditFailureInjector);
            services.AddSingleton(_auditFailureInjector);

            services.RemoveAll<IAnalyticsFailureInjector>();
            services.AddSingleton<IAnalyticsFailureInjector>(_analyticsFailureInjector);
            services.AddSingleton(_analyticsFailureInjector);

            _configureTestServices?.Invoke(services);
        });
    }

    internal static Dictionary<string, string?> CreateTestConfiguration(string connectionString) =>
        new()
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString,
            ["Jwt:Secret"] = "HelpDev_Integration_Test_Secret_Key_32+",
            ["Jwt:Issuer"] = "HelpDev",
            ["Jwt:Audience"] = "HelpDev.Client",
            ["Jwt:ExpirationMinutes"] = "60",
            ["Auth:ExposeOtpInResponse"] = "true",
            ["Auth:OtpExpirationMinutes"] = "5",
            ["Otp:MaxFailedAttempts"] = "5",
            ["Outbox:BatchSize"] = "20",
            ["Outbox:PollIntervalSeconds"] = "5",
            ["Outbox:LockDurationSeconds"] = "30",
            ["Outbox:MaxAttempts"] = "3",
            ["Security:PartitionHashKey"] = "HelpDev_Integration_Partition_Hash_Key_32+",
            ["Security:EnableSecurityHeaders"] = "true",
            ["Security:EnableRateLimiting"] = "true",
            ["Security:DefaultRequestBodyLimitBytes"] = "1024",
            ["Security:MaxJsonRequestBodyLimitBytes"] = "1024",
            ["Cors:FrontendOrigins:0"] = "http://localhost:3000",
            // Keep OTP policies tight via appsettings.Testing.json; raise global/admin ceilings so
            // multi-step admin workflows are not blocked by GeneralApi PermitLimit=2.
            ["RateLimiting:General:PermitLimit"] = "10000",
            ["RateLimiting:General:WindowSeconds"] = "60",
            ["RateLimiting:AdminMutation:PermitLimit"] = "10000",
            ["RateLimiting:AdminMutation:WindowSeconds"] = "60",
            ["RateLimiting:PublicContentRead:PermitLimit"] = "10000",
            ["RateLimiting:PublicContentRead:WindowSeconds"] = "60",
            ["RateLimiting:Authentication:PermitLimit"] = "10000",
            ["RateLimiting:Search:PermitLimit"] = "10000",
            ["RateLimiting:SearchAnonymous:PermitLimit"] = "10000",
            ["RateLimiting:ToolboxExecution:PermitLimit"] = "10000",
            ["RateLimiting:ToolboxExecutionAnonymous:PermitLimit"] = "10000",
            ["RateLimiting:PromptRender:PermitLimit"] = "10000",
            ["RateLimiting:PromptRenderAnonymous:PermitLimit"] = "10000",
            // Keep OTP limits intentionally tight for 429 regression coverage.
            ["RateLimiting:OtpRequest:PermitLimit"] = "2",
            ["RateLimiting:OtpRequest:WindowSeconds"] = "10",
            ["RateLimiting:OtpRequestNetwork:PermitLimit"] = "2",
            ["RateLimiting:OtpRequestNetwork:WindowSeconds"] = "10",
            ["RateLimiting:OtpVerify:PermitLimit"] = "10",
            ["RateLimiting:OtpVerify:WindowSeconds"] = "10",
            ["Media:MaxUploadBytes"] = "5242880",
            ["Media:MaxWidth"] = "8192",
            ["Media:MaxHeight"] = "8192",
            ["Media:PublicBasePath"] = "/media",
            ["Ai:Enabled"] = "true",
            ["Ai:ProviderName"] = "Fake",
            ["Ai:Model"] = "fake-v1",
            ["Ai:AllowedTasks"] =
                "ContentAnalysis,TitleSuggestion,MetaDescription,OutlineGeneration,FaqGeneration",
            ["Ai:DefaultMaxTokens"] = "1024",
            ["Embedding:Enabled"] = "true",
            ["Embedding:ProviderName"] = "Fake",
            ["Embedding:Model"] = "fake-embed-v1",
            ["Embedding:Dimensions"] = "384",
        };

    private static void RemoveHostedService<TImplementation>(IServiceCollection services)
        where TImplementation : class, IHostedService
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(TImplementation))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
