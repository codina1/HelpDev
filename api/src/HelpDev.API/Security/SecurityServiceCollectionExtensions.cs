using System.Text.Json;
using System.Threading.RateLimiting;
using HelpDev.API.Security.RateLimiting;
using HelpDev.Modules.Identity.Application.Auth;
using HelpDev.Modules.Identity.Application.Common;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityHardening(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<SecurityOptions>, SecurityOptionsValidator>();
        services.AddOptions<SecurityOptions>()
            .Bind(configuration.GetSection(SecurityOptions.SectionName))
            .PostConfigure(options =>
            {
                var corsOrigins = configuration.GetSection("Cors:FrontendOrigins").Get<string[]>() ?? [];
                if (options.AllowedCorsOrigins.Length == 0)
                {
                    options.AllowedCorsOrigins = corsOrigins;
                }
            })
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RateLimitOptions>, RateLimitOptionsValidator>();
        services.AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IRateLimitPartitionKeyProvider>(sp =>
            new RateLimitPartitionKeyProvider(sp.GetRequiredService<IOptions<SecurityOptions>>().Value));

        services.AddSingleton<ISensitiveDataRedactor, SensitiveDataRedactor>();

        if (environment.IsProduction())
        {
            services.AddSingleton<IValidateOptions<JwtSettings>, JwtProductionSettingsValidator>();
        }

        var securityOptions = configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
        if (string.IsNullOrWhiteSpace(securityOptions.PartitionHashKey))
        {
            var fallbackKey = configuration["Security:PartitionHashKey"]
                ?? configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Security partition hash key is required.");
            services.PostConfigure<SecurityOptions>(options => options.PartitionHashKey = fallbackKey);
        }

        services.AddRateLimiter(options =>
        {
            var rateLimitOptions = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                var payload = JsonSerializer.Serialize(new
                {
                    message = "Too many requests. Please try again later.",
                    code = SecurityErrorCodes.RateLimitExceeded,
                });

                await context.HttpContext.Response.WriteAsync(payload, token);
            };

            options.AddPolicy(RateLimitPolicyNames.GeneralApi, CreateUserOrNetworkPolicy(rateLimitOptions.General));
            options.AddPolicy(RateLimitPolicyNames.Authentication, CreateUserOrNetworkPolicy(rateLimitOptions.Authentication));
            options.AddPolicy(RateLimitPolicyNames.OtpRequest, CreateOtpRequestPolicy(rateLimitOptions));
            options.AddPolicy(RateLimitPolicyNames.OtpVerify, CreateOtpVerifyPolicy(rateLimitOptions));
            options.AddPolicy(RateLimitPolicyNames.Search, CreateAuthenticatedOrAnonymousPolicy(rateLimitOptions.Search, rateLimitOptions.SearchAnonymous));
            options.AddPolicy(RateLimitPolicyNames.ToolboxExecution, CreateAuthenticatedOrAnonymousPolicy(rateLimitOptions.ToolboxExecution, rateLimitOptions.ToolboxExecutionAnonymous));
            options.AddPolicy(RateLimitPolicyNames.PromptRender, CreateAuthenticatedOrAnonymousPolicy(rateLimitOptions.PromptRender, rateLimitOptions.PromptRenderAnonymous));
            options.AddPolicy(RateLimitPolicyNames.PublicContentRead, CreateUserOrNetworkPolicy(rateLimitOptions.PublicContentRead));
            options.AddPolicy(RateLimitPolicyNames.AdminMutation, CreateAuthenticatedOnlyPolicy(rateLimitOptions.AdminMutation));
        });

        return services;
    }

    public static IServiceCollection AddHelpDevCors(
        this IServiceCollection services,
        SecurityOptions securityOptions)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("HelpDevCors", policy =>
            {
                policy.WithOrigins(securityOptions.AllowedCorsOrigins)
                    .WithHeaders("Authorization", "Content-Type", "Accept", "X-Correlation-ID")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        return services;
    }

    private static Func<HttpContext, RateLimitPartition<string>> CreateUserOrNetworkPolicy(RateLimitPolicyOptions policyOptions) =>
        httpContext =>
        {
            var provider = httpContext.RequestServices.GetRequiredService<IRateLimitPartitionKeyProvider>();
            var userId = provider.TryGetAuthenticatedUserId(httpContext);
            var partitionKey = userId.HasValue
                ? provider.GetAuthenticatedPartition(userId.Value)
                : provider.GetAnonymousNetworkPartition(httpContext);

            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => CreateSlidingWindowOptions(policyOptions));
        };

    private static Func<HttpContext, RateLimitPartition<string>> CreateAuthenticatedOnlyPolicy(RateLimitPolicyOptions policyOptions) =>
        httpContext =>
        {
            var provider = httpContext.RequestServices.GetRequiredService<IRateLimitPartitionKeyProvider>();
            var userId = provider.TryGetAuthenticatedUserId(httpContext);
            var partitionKey = userId.HasValue
                ? provider.GetAuthenticatedPartition(userId.Value)
                : provider.GetAnonymousNetworkPartition(httpContext);

            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => CreateSlidingWindowOptions(policyOptions));
        };

    private static Func<HttpContext, RateLimitPartition<string>> CreateAuthenticatedOrAnonymousPolicy(
        RateLimitPolicyOptions authenticated,
        RateLimitPolicyOptions anonymous) =>
        httpContext =>
        {
            var provider = httpContext.RequestServices.GetRequiredService<IRateLimitPartitionKeyProvider>();
            var userId = provider.TryGetAuthenticatedUserId(httpContext);
            var policy = userId.HasValue ? authenticated : anonymous;
            var partitionKey = userId.HasValue
                ? provider.GetAuthenticatedPartition(userId.Value)
                : provider.GetAnonymousNetworkPartition(httpContext);

            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey,
                _ => CreateSlidingWindowOptions(policy));
        };

    private static Func<HttpContext, RateLimitPartition<string>> CreateOtpRequestPolicy(RateLimitOptions rateLimitOptions) =>
        httpContext =>
        {
            var provider = httpContext.RequestServices.GetRequiredService<IRateLimitPartitionKeyProvider>();
            var networkKey = provider.GetAnonymousNetworkPartition(httpContext);

            if (TryReadNormalizedPhone(httpContext, out var normalizedPhone))
            {
                var phoneKey = provider.GetOtpTargetPartition(normalizedPhone);
                return RateLimitPartition.GetSlidingWindowLimiter(
                    phoneKey,
                    _ => CreateSlidingWindowOptions(rateLimitOptions.OtpRequest));
            }

            return RateLimitPartition.GetSlidingWindowLimiter(
                networkKey,
                _ => CreateSlidingWindowOptions(rateLimitOptions.OtpRequestNetwork));
        };

    private static Func<HttpContext, RateLimitPartition<string>> CreateOtpVerifyPolicy(RateLimitOptions rateLimitOptions) =>
        httpContext =>
        {
            var provider = httpContext.RequestServices.GetRequiredService<IRateLimitPartitionKeyProvider>();

            if (TryReadNormalizedPhone(httpContext, out var normalizedPhone))
            {
                var phoneKey = provider.GetOtpTargetPartition(normalizedPhone);
                return RateLimitPartition.GetSlidingWindowLimiter(
                    phoneKey,
                    _ => CreateSlidingWindowOptions(rateLimitOptions.OtpVerify));
            }

            var networkKey = provider.GetAnonymousNetworkPartition(httpContext);
            return RateLimitPartition.GetSlidingWindowLimiter(
                networkKey,
                _ => CreateSlidingWindowOptions(rateLimitOptions.OtpVerify));
        };

    private static SlidingWindowRateLimiterOptions CreateSlidingWindowOptions(RateLimitPolicyOptions policyOptions) =>
        new()
        {
            PermitLimit = policyOptions.PermitLimit,
            Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds),
            SegmentsPerWindow = 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = policyOptions.QueueLimit,
        };

    private static bool TryReadNormalizedPhone(HttpContext httpContext, out string normalizedPhone)
    {
        normalizedPhone = string.Empty;
        if (!httpContext.Request.Body.CanSeek)
        {
            httpContext.Request.EnableBuffering();
        }

        httpContext.Request.Body.Position = 0;
        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var body = reader.ReadToEndAsync().GetAwaiter().GetResult();
        httpContext.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("mobile", out var mobileElement))
            {
                return false;
            }

            var mobile = mobileElement.GetString();
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return false;
            }

            normalizedPhone = mobile;
            return MobileNormalizer.TryNormalize(mobile, out normalizedPhone);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public sealed class SecurityOptionsValidator : IValidateOptions<SecurityOptions>
{
    private readonly IHostEnvironment _environment;

    public SecurityOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, SecurityOptions options)
    {
        if (options.DefaultRequestBodyLimitBytes <= 0 || options.MaxJsonRequestBodyLimitBytes <= 0)
        {
            return ValidateOptionsResult.Fail("Request body limits must be positive.");
        }

        foreach (var origin in options.AllowedCorsOrigins)
        {
            if (!TryValidateOrigin(origin, out var error))
            {
                return ValidateOptionsResult.Fail(error);
            }
        }

        if (_environment.IsProduction())
        {
            if (options.AllowedCorsOrigins.Any(origin => origin == "*"))
            {
                return ValidateOptionsResult.Fail("Wildcard CORS origin is not allowed in production.");
            }

            if (!options.RequireHttpsMetadata)
            {
                return ValidateOptionsResult.Fail("RequireHttpsMetadata must be enabled in production.");
            }
        }

        foreach (var network in options.TrustedProxyNetworks)
        {
            if (!network.Contains('/', StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Fail($"Trusted proxy network '{network}' is malformed.");
            }
        }

        return ValidateOptionsResult.Success;
    }

    internal static bool TryValidateOrigin(string origin, out string error)
    {
        error = string.Empty;
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            error = $"CORS origin '{origin}' is not a valid absolute URI.";
            return false;
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            error = $"CORS origin '{origin}' must use http or https.";
            return false;
        }

        if (!string.IsNullOrEmpty(uri.PathAndQuery) && uri.PathAndQuery != "/")
        {
            error = $"CORS origin '{origin}' must not include a path.";
            return false;
        }

        return true;
    }
}

public sealed class RateLimitOptionsValidator : IValidateOptions<RateLimitOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitOptions options)
    {
        var policies = new[]
        {
            options.General, options.Authentication, options.OtpRequest, options.OtpRequestNetwork,
            options.OtpVerify, options.Search, options.SearchAnonymous, options.ToolboxExecution,
            options.ToolboxExecutionAnonymous, options.PromptRender, options.PromptRenderAnonymous,
            options.PublicContentRead, options.AdminMutation,
        };

        foreach (var policy in policies)
        {
            if (policy.PermitLimit <= 0 || policy.WindowSeconds <= 0 || policy.QueueLimit < 0)
            {
                return ValidateOptionsResult.Fail("Rate limit policy values must be positive and queue limit non-negative.");
            }
        }

        return ValidateOptionsResult.Success;
    }
}

public sealed class JwtProductionSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        if (options.Secret.Contains("Change_In_Production", StringComparison.OrdinalIgnoreCase) ||
            options.Secret.Contains("Dev_Secret", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("Production JWT secret must not use development placeholder values.");
        }

        if (options.ExpirationMinutes > 24 * 60)
        {
            return ValidateOptionsResult.Fail("JWT expiration must not exceed 24 hours in production.");
        }

        return ValidateOptionsResult.Success;
    }
}

public interface ISensitiveDataRedactor
{
    string RedactPhone(string phone);

    string RedactToken(string token);

    string RedactKeyValue(string key, string value);

    bool IsSensitiveKey(string key);
}

public sealed class SensitiveDataRedactor : ISensitiveDataRedactor
{
    public string RedactPhone(string phone) => "***";

    public string RedactToken(string token) => "***";

    public string RedactKeyValue(string key, string value) =>
        IsSensitiveKey(key) ? "***" : value;

    public bool IsSensitiveKey(string key)
    {
        var normalized = key.Replace("-", string.Empty, StringComparison.Ordinal);
        return normalized.Contains("password", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("otp", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("token", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("secret", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("authorization", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("cookie", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("apikey", StringComparison.OrdinalIgnoreCase);
    }
}
