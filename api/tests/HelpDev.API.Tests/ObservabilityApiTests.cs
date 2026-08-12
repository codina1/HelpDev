using System.Text.Json;
using HelpDev.API.Controllers;
using HelpDev.API.Observability;
using HelpDev.API.Security;
using HelpDev.API.Tests.Fakes;
using HelpDev.Infrastructure.Observability.HealthChecks;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedContracts.Observability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests;

public sealed class ObservabilityApiTests
{
    [Fact]
    public void Operations_admin_controller_requires_AdminOnly_policy()
    {
        var attribute = Assert.Single(
            typeof(OperationsAdminController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>());

        Assert.Equal(AuthorizationPolicies.AdminOnly, attribute.Policy);
    }

    [Fact]
    public void Operations_admin_controller_depends_on_operational_abstractions_only()
    {
        var parameters = typeof(OperationsAdminController).GetConstructors().Single().GetParameters()
            .Select(parameter => parameter.ParameterType)
            .ToList();

        Assert.Contains(typeof(IOperationalStatusService), parameters);
        Assert.Contains(typeof(IOutboxOperationalQueries), parameters);
        Assert.Contains(typeof(ISearchOperationalQueries), parameters);
        Assert.Contains(typeof(IAnalyticsOperationalQueries), parameters);
        Assert.Contains(typeof(IAuditOperationalQueries), parameters);
        Assert.DoesNotContain(
            parameters,
            type => type.Name.Contains("DbContext", StringComparison.Ordinal)
                || type.Name.Contains("Repository", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Public_health_response_writer_produces_minimal_json()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Healthy,
            TimeSpan.FromMilliseconds(1));

        await PublicHealthResponseWriter.WriteMinimalResponse(context, report);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(OperationalHealthStates.Healthy, document.RootElement.GetProperty("status").GetString());
        Assert.Single(document.RootElement.EnumerateObject());
    }

    [Fact]
    public async Task Public_health_response_writer_returns_503_for_unhealthy()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var report = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Unhealthy,
            TimeSpan.FromMilliseconds(1));

        await PublicHealthResponseWriter.WriteMinimalResponse(context, report);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
    }

    [Fact]
    public async Task Request_logging_middleware_logs_request_completed()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var middleware = CreateMiddleware(logger, next: _ => Task.CompletedTask);
        var context = CreateHttpContext("/api/courses");

        await middleware.InvokeAsync(context, new FakeCorrelationContext());

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Information, entry.LogLevel);
        Assert.Contains(LoggingEventNames.RequestCompleted, entry.Message, StringComparison.Ordinal);
        Assert.Contains("CorrelationId=", entry.Message, StringComparison.Ordinal);
        Assert.Contains("RouteTemplate=", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Request_logging_middleware_skips_excluded_routes()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var middleware = CreateMiddleware(logger, next: _ => Task.CompletedTask);

        await middleware.InvokeAsync(CreateHttpContext("/health/live"), new FakeCorrelationContext());

        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task Request_logging_middleware_logs_request_failed_on_exception()
    {
        var logger = new TestLogger<RequestLoggingMiddleware>();
        var middleware = CreateMiddleware(
            logger,
            _ => throw new InvalidOperationException("boom"));
        var context = CreateHttpContext("/api/courses");

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(context, new FakeCorrelationContext()));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.LogLevel);
        Assert.Contains(LoggingEventNames.RequestFailed, entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Request_logging_middleware_does_not_log_request_body()
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
    }

    private static RequestLoggingMiddleware CreateMiddleware(
        ILogger<RequestLoggingMiddleware> logger,
        RequestDelegate next)
    {
        var options = Options.Create(new ObservabilityOptions
        {
            SlowRequests = new SlowRequestOptions
            {
                Enabled = false,
            },
        });

        return new RequestLoggingMiddleware(
            next,
            logger,
            options);
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = HttpMethods.Get;
        context.Response.StatusCode = StatusCodes.Status200OK;
        return context;
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

    private sealed class FakeCorrelationContext : ICorrelationContext
    {
        public string CorrelationId { get; } = "test-correlation-id";
    }

    internal sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    internal sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);
}
