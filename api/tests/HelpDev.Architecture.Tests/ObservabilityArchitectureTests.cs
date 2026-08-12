using System.Reflection;
using System.Text.Json;
using HelpDev.API.Controllers;
using HelpDev.API.Observability;
using HelpDev.Infrastructure.Observability;
using HelpDev.Infrastructure.Observability.HealthChecks;
using HelpDev.SharedContracts.Observability;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

public sealed class ObservabilityArchitectureTests
{
    [Fact]
    public void Health_checks_do_not_take_DbContext_in_constructors()
    {
        var healthCheckTypes = typeof(SelfHealthCheck).Assembly
            .GetTypes()
            .Where(type => typeof(Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck).IsAssignableFrom(type)
                && type is { IsAbstract: false, IsInterface: false })
            .ToList();

        foreach (var type in healthCheckTypes)
        {
            var ctor = type.GetConstructors().SingleOrDefault();
            if (ctor is null)
            {
                continue;
            }

            Assert.DoesNotContain(
                ctor.GetParameters(),
                parameter => parameter.ParameterType.Name.Contains("DbContext", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Public_health_response_contains_only_status_property()
    {
        var method = typeof(PublicHealthResponseWriter)
            .GetMethod(nameof(PublicHealthResponseWriter.WriteMinimalResponse), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);

        var source = File.ReadAllText(
            Path.Combine(
                FindRepositoryRoot(),
                "src",
                "HelpDev.Infrastructure",
                "Observability",
                "HealthChecks",
                "HealthCheckInfrastructure.cs"));

        Assert.Contains("new { status }", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Solution_does_not_reference_external_monitoring_sdks()
    {
        var forbiddenPackages = new[]
        {
            "OpenTelemetry",
            "ApplicationInsights",
            "Sentry",
            "Datadog",
            "NewRelic",
            "Serilog",
        };

        var apiProject = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "src", "HelpDev.API", "HelpDev.API.csproj"));
        var infrastructureProject = File.ReadAllText(
            Path.Combine(FindRepositoryRoot(), "src", "HelpDev.Infrastructure", "HelpDev.Infrastructure.csproj"));

        foreach (var package in forbiddenPackages)
        {
            Assert.DoesNotContain(package, apiProject, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(package, infrastructureProject, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Request_logging_middleware_does_not_read_or_log_request_body()
    {
        var source = File.ReadAllText(
            Path.Combine(
                FindRepositoryRoot(),
                "src",
                "HelpDev.API",
                "Observability",
                "RequestLoggingMiddleware.cs"));

        Assert.DoesNotContain("Request.Body", source, StringComparison.Ordinal);
        Assert.DoesNotContain("EnableBuffering", source, StringComparison.Ordinal);
        Assert.DoesNotContain("RequestBody", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Operational_query_implementations_reside_in_Infrastructure()
    {
        var result = Types.InAssembly(typeof(OperationalStatusService).Assembly)
            .That()
            .ImplementInterface(typeof(IOutboxOperationalQueries))
            .Or()
            .ImplementInterface(typeof(ISearchOperationalQueries))
            .Or()
            .ImplementInterface(typeof(IAnalyticsOperationalQueries))
            .Or()
            .ImplementInterface(typeof(IAuditOperationalQueries))
            .Should()
            .ResideInNamespaceContaining("HelpDev.Infrastructure.Observability")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Operations_admin_controller_does_not_depend_on_Infrastructure_concrete_types()
    {
        var result = Types.InAssembly(typeof(OperationsAdminController).Assembly)
            .That()
            .HaveName(nameof(OperationsAdminController))
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Operational_status_service_lives_in_Infrastructure_not_API()
    {
        var apiResult = Types.InAssembly(typeof(OperationsAdminController).Assembly)
            .ShouldNot()
            .HaveDependencyOn(typeof(OperationalStatusService).FullName!)
            .GetResult();

        Assert.True(apiResult.IsSuccessful, FormatFailures(apiResult));

        var infrastructureResult = Types.InAssembly(typeof(OperationalStatusService).Assembly)
            .That()
            .HaveName(nameof(OperationalStatusService))
            .Should()
            .ResideInNamespace("HelpDev.Infrastructure.Observability")
            .GetResult();

        Assert.True(infrastructureResult.IsSuccessful, FormatFailures(infrastructureResult));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HelpDev.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}
