using System.Diagnostics;
using HelpDev.API.Security;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedContracts.Observability;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Observability;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly SlowRequestOptions _slowRequestOptions;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<ObservabilityOptions> options)
    {
        _next = next;
        _logger = logger;
        _slowRequestOptions = options.Value.SlowRequests;
    }

    // ICorrelationContext is scoped, so it is injected per-request here rather than
    // through the constructor (which resolves from the root provider).
    public async Task InvokeAsync(HttpContext context, ICorrelationContext correlationContext)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var started = Stopwatch.GetTimestamp();
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            LogCompletion(context, started, ex, correlationContext);
            throw;
        }

        LogCompletion(context, started, null, correlationContext);
    }

    private void LogCompletion(
        HttpContext context,
        long started,
        Exception? exception,
        ICorrelationContext correlationContext)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        var routeTemplate = context.GetEndpoint()?.Metadata
            .GetMetadata<Microsoft.AspNetCore.Routing.RouteEndpoint>()?.RoutePattern.RawText
            ?? context.Request.Path.Value
            ?? "/";

        var authenticated = context.User.Identity?.IsAuthenticated == true;
        Guid? userId = null;
        if (authenticated &&
            Guid.TryParse(
                context.User.FindFirst(HelpDev.Modules.Identity.Application.Auth.JwtClaimTypes.UserId)?.Value,
                out var parsed))
        {
            userId = parsed;
        }

        var statusCode = exception is null ? context.Response.StatusCode : StatusCodes.Status500InternalServerError;
        var logLevel = ResolveLogLevel(statusCode, exception);

        _logger.Log(
            logLevel,
            exception,
            "Event={Event} CorrelationId={CorrelationId} RequestMethod={RequestMethod} RouteTemplate={RouteTemplate} StatusCode={StatusCode} DurationMilliseconds={DurationMilliseconds} Authenticated={Authenticated} UserId={UserId}",
            exception is null ? LoggingEventNames.RequestCompleted : LoggingEventNames.RequestFailed,
            correlationContext.CorrelationId,
            context.Request.Method,
            routeTemplate,
            statusCode,
            (long)elapsedMs,
            authenticated,
            userId);

        if (_slowRequestOptions.Enabled &&
            elapsedMs >= _slowRequestOptions.WarningThresholdMilliseconds &&
            exception is null)
        {
            _logger.LogWarning(
                "Event={Event} CorrelationId={CorrelationId} RequestMethod={RequestMethod} RouteTemplate={RouteTemplate} StatusCode={StatusCode} DurationMilliseconds={DurationMilliseconds}",
                LoggingEventNames.SlowRequestDetected,
                correlationContext.CorrelationId,
                context.Request.Method,
                routeTemplate,
                statusCode,
                (long)elapsedMs);
        }
    }

    private bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;
        foreach (var prefix in _slowRequestOptions.ExcludedRoutePrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static LogLevel ResolveLogLevel(int statusCode, Exception? exception)
    {
        if (exception is not null)
        {
            return LogLevel.Error;
        }

        if (statusCode >= 500)
        {
            return LogLevel.Error;
        }

        if (statusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden or StatusCodes.Status429TooManyRequests)
        {
            return LogLevel.Warning;
        }

        return LogLevel.Information;
    }
}
