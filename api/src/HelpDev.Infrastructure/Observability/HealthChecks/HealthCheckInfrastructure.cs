using System.Text.Json;
using HelpDev.SharedContracts.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Observability.HealthChecks;

public sealed class SelfHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(HealthCheckResult.Healthy("Process is running."));
    }
}

public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly IPostgreSqlHealthProbe _probe;

    public PostgreSqlHealthCheck(IPostgreSqlHealthProbe probe)
    {
        _probe = probe;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await _probe.CheckAsync(cancellationToken);
        return Map(result.Status, result.IsAvailable ? "Database connectivity verified." : "Database connectivity failed.", result.Code);
    }

    private static HealthCheckResult Map(string status, string description, string? code) =>
        status switch
        {
            OperationalHealthStates.Healthy => HealthCheckResult.Healthy(description),
            OperationalHealthStates.Degraded => HealthCheckResult.Degraded(description, data: ToData(code)),
            _ => HealthCheckResult.Unhealthy(description, data: ToData(code)),
        };

    private static IReadOnlyDictionary<string, object>? ToData(string? code) =>
        code is null ? null : new Dictionary<string, object> { ["code"] = code };
}

public sealed class ReadinessHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IApplicationReadinessState _readinessState;

    public ReadinessHealthCheck(
        IServiceScopeFactory scopeFactory,
        IApplicationReadinessState readinessState)
    {
        _scopeFactory = scopeFactory;
        _readinessState = readinessState;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        switch (_readinessState.Status)
        {
            case ApplicationReadinessStatus.Starting:
                return HealthCheckResult.Unhealthy("Starting");
            case ApplicationReadinessStatus.Stopping:
                return HealthCheckResult.Unhealthy("Stopping");
            case ApplicationReadinessStatus.Failed:
                return HealthCheckResult.Unhealthy("Failed");
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var statusService = scope.ServiceProvider.GetRequiredService<IOperationalStatusService>();
        var status = await statusService.GetReadinessStatusAsync(cancellationToken);
        return status switch
        {
            OperationalHealthStates.Healthy => HealthCheckResult.Healthy(status),
            OperationalHealthStates.Degraded => HealthCheckResult.Degraded(status),
            _ => HealthCheckResult.Unhealthy(status),
        };
    }
}

public static class PublicHealthResponseWriter
{
    public static Task WriteMinimalResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var status = report.Status switch
        {
            HealthStatus.Healthy => OperationalHealthStates.Healthy,
            HealthStatus.Degraded => OperationalHealthStates.Degraded,
            _ => OperationalHealthStates.Unhealthy,
        };

        var statusCode = status == OperationalHealthStates.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(
            JsonSerializer.Serialize(new { status }),
            context.RequestAborted);
    }
}

public sealed class ObservabilityOptionsValidator : IValidateOptions<ObservabilityOptions>
{
    public ValidateOptionsResult Validate(string? name, ObservabilityOptions options)
    {
        if (options.PublicHealthCacheSeconds <= 0 || options.FailureCacheSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("Health cache seconds must be positive.");
        }

        if (options.GlobalTimeoutSeconds <= 0 || options.MaximumConcurrentChecks <= 0)
        {
            return ValidateOptionsResult.Fail("Global health timeout and concurrency must be positive.");
        }

        if (!ValidatePostgreSql(options.PostgreSql, out var postgresError))
        {
            return ValidateOptionsResult.Fail(postgresError);
        }

        if (!ValidateThresholds(options.Outbox.WarningPendingCount, options.Outbox.CriticalPendingCount, "Outbox pending"))
        {
            return ValidateOptionsResult.Fail("Outbox critical pending count must exceed warning count.");
        }

        if (!ValidateThresholds(options.Search.WarningPendingCount, options.Search.CriticalPendingCount, "Search pending"))
        {
            return ValidateOptionsResult.Fail("Search critical pending count must exceed warning count.");
        }

        if (!ValidateThresholds(options.Analytics.WarningFailureCount, options.Analytics.CriticalFailureCount, "Analytics failure"))
        {
            return ValidateOptionsResult.Fail("Analytics critical failure count must exceed warning count.");
        }

        if (options.SlowRequests.WarningThresholdMilliseconds <= 0 ||
            options.SlowRequests.ErrorThresholdMilliseconds <= options.SlowRequests.WarningThresholdMilliseconds)
        {
            return ValidateOptionsResult.Fail("Slow request thresholds are invalid.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool ValidatePostgreSql(PostgreSqlHealthOptions options, out string error)
    {
        error = string.Empty;
        if (options.TimeoutSeconds is < 1 or > 30)
        {
            error = "PostgreSQL health timeout must be between 1 and 30 seconds.";
            return false;
        }

        if (options.DegradedLatencyMilliseconds <= 0 ||
            options.UnhealthyLatencyMilliseconds <= options.DegradedLatencyMilliseconds)
        {
            error = "PostgreSQL latency thresholds are invalid.";
            return false;
        }

        return true;
    }

    private static bool ValidateThresholds(long warning, long critical, string name) =>
        warning >= 0 && critical > warning;
}
