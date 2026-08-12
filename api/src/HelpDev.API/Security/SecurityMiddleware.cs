using HelpDev.SharedContracts.Auditing;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Security;

public sealed class CorrelationContext : ICorrelationContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
}

public sealed class AuditRequestContext : IAuditRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationContext _correlationContext;

    public AuditRequestContext(IHttpContextAccessor httpContextAccessor, ICorrelationContext correlationContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _correlationContext = correlationContext;
    }

    public string? RequestMethod => _httpContextAccessor.HttpContext?.Request.Method;

    public string? RequestPathTemplate =>
        _httpContextAccessor.HttpContext?.GetEndpoint()?.Metadata
            .GetMetadata<Microsoft.AspNetCore.Routing.RouteEndpoint>()?.RoutePattern.RawText
        ?? _httpContextAccessor.HttpContext?.Request.Path.Value;

    public string? CorrelationId => _correlationContext.CorrelationId;
}

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private const int MaxLength = 100;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CorrelationContext correlationContext)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(incoming) && IsValidCorrelationId(incoming))
        {
            correlationContext.CorrelationId = incoming;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationContext.CorrelationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }

    internal static bool IsValidCorrelationId(string value)
    {
        if (value.Length > MaxLength)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!(char.IsLetterOrDigit(character) || character is '-' or '_' or '.'))
            {
                return false;
            }
        }

        return true;
    }
}

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityOptions _options;
    private readonly IHostEnvironment _environment;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IOptions<SecurityOptions> options,
        IHostEnvironment environment)
    {
        _next = next;
        _options = options.Value;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.EnableSecurityHeaders)
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "DENY";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Cross-Origin-Resource-Policy"] = "same-site";

            var isSwagger = context.Request.Path.StartsWithSegments("/swagger");
            if (!isSwagger || !_environment.IsDevelopment())
            {
                headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none';";
            }

            if (!_environment.IsDevelopment())
            {
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }
        }

        await _next(context);
    }
}

public sealed class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityOptions _options;

    public RequestSizeLimitMiddleware(RequestDelegate next, IOptions<SecurityOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var limit = ResolveLimit(path, _options);

        if (context.Request.ContentLength is > 0 and var contentLength && contentLength > limit)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "The request payload is too large.",
                code = SecurityErrorCodes.RequestTooLarge,
            });
            return;
        }

        context.Request.Body = new LimitedReadStream(context.Request.Body, limit);
        await _next(context);
    }

    private static int ResolveLimit(string path, SecurityOptions options)
    {
        if (path.Contains("/api/auth/", StringComparison.OrdinalIgnoreCase))
        {
            return 16 * 1024;
        }

        if (path.Contains("/api/tools/", StringComparison.OrdinalIgnoreCase) &&
            path.Contains("/execute", StringComparison.OrdinalIgnoreCase))
        {
            return 128 * 1024;
        }

        if (path.Contains("/api/prompts/", StringComparison.OrdinalIgnoreCase) &&
            path.Contains("/render", StringComparison.OrdinalIgnoreCase))
        {
            return 128 * 1024;
        }

        // POST /api/v1/admin/media (upload root only — not /admin/media/{id}).
        if (IsMediaUploadPath(path))
        {
            return 20 * 1024 * 1024;
        }

        if (path.Contains("/api/admin/", StringComparison.OrdinalIgnoreCase))
        {
            return options.DefaultRequestBodyLimitBytes;
        }

        return options.MaxJsonRequestBodyLimitBytes;
    }

    private static bool IsMediaUploadPath(string path)
    {
        var normalized = path.TrimEnd('/');
        return normalized.EndsWith("/admin/media", StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class LimitedReadStream : Stream
{
    private readonly Stream _inner;
    private readonly int _maxBytes;
    private int _readBytes;

    public LimitedReadStream(Stream inner, int maxBytes)
    {
        _inner = inner;
        _maxBytes = maxBytes;
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => _inner.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        _readBytes += read;
        if (_readBytes > _maxBytes)
        {
            throw new InvalidOperationException(SecurityErrorCodes.RequestTooLarge);
        }

        return read;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var read = await _inner.ReadAsync(buffer, cancellationToken);
        _readBytes += read;
        if (_readBytes > _maxBytes)
        {
            throw new InvalidOperationException(SecurityErrorCodes.RequestTooLarge);
        }

        return read;
    }
}

public sealed class AccessDeniedAuditMiddleware
{
    private readonly RequestDelegate _next;

    public AccessDeniedAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuditRecorder auditRecorder,
        IAuditRequestContext auditRequestContext)
    {
        await _next(context);

        if (context.Response.StatusCode != StatusCodes.Status403Forbidden)
        {
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (!ShouldAudit(path))
        {
            return;
        }

        var userId = context.User.FindFirst(HelpDev.Modules.Identity.Application.Auth.JwtClaimTypes.UserId)?.Value;
        Guid? actorUserId = Guid.TryParse(userId, out var parsed) ? parsed : null;

        await auditRecorder.RecordAsync(new AuditRecordInput(
            Category: AuditCategories.Authorization,
            Action: AuditActions.AuthorizationAccessDenied,
            Outcome: AuditOutcomes.Denied,
            ActorUserId: actorUserId,
            ActorType: actorUserId.HasValue ? AuditActorTypes.User : AuditActorTypes.Anonymous,
            ReasonCode: "admin_policy_required",
            CorrelationId: auditRequestContext.CorrelationId,
            RequestMethod: auditRequestContext.RequestMethod,
            RequestPathTemplate: auditRequestContext.RequestPathTemplate,
            Metadata: new Dictionary<string, string> { ["reasonCode"] = "admin_policy_required" }), context.RequestAborted);
    }

    private static bool ShouldAudit(string path) =>
        path.StartsWith("/api/admin/", StringComparison.OrdinalIgnoreCase);
}
