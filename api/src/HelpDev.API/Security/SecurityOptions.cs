namespace HelpDev.API.Security;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public bool EnableSecurityHeaders { get; set; } = true;

    public bool EnableRateLimiting { get; set; } = true;

    public bool EnableAudit { get; set; } = true;

    public int DefaultRequestBodyLimitBytes { get; set; } = 256 * 1024;

    public int MaxJsonRequestBodyLimitBytes { get; set; } = 256 * 1024;

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool AllowDetailedAuthenticationErrors { get; set; } = false;

    public string[] AllowedCorsOrigins { get; set; } = [];

    public string[] TrustedProxyAddresses { get; set; } = [];

    public string[] TrustedProxyNetworks { get; set; } = [];

    public string PartitionHashKey { get; set; } = string.Empty;
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }

    public int QueueLimit { get; set; }

    public int RejectionStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    public bool RetryAfterEnabled { get; set; } = true;
}

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions General { get; set; } = new() { PermitLimit = 120, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions Authentication { get; set; } = new() { PermitLimit = 20, WindowSeconds = 300, QueueLimit = 0 };

    public RateLimitPolicyOptions OtpRequest { get; set; } = new() { PermitLimit = 5, WindowSeconds = 900, QueueLimit = 0 };

    public RateLimitPolicyOptions OtpRequestNetwork { get; set; } = new() { PermitLimit = 10, WindowSeconds = 900, QueueLimit = 0 };

    public RateLimitPolicyOptions OtpVerify { get; set; } = new() { PermitLimit = 10, WindowSeconds = 900, QueueLimit = 0 };

    public RateLimitPolicyOptions Search { get; set; } = new() { PermitLimit = 60, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions SearchAnonymous { get; set; } = new() { PermitLimit = 30, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions ToolboxExecution { get; set; } = new() { PermitLimit = 30, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions ToolboxExecutionAnonymous { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions PromptRender { get; set; } = new() { PermitLimit = 30, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions PromptRenderAnonymous { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions PublicContentRead { get; set; } = new() { PermitLimit = 180, WindowSeconds = 60, QueueLimit = 0 };

    public RateLimitPolicyOptions AdminMutation { get; set; } = new() { PermitLimit = 60, WindowSeconds = 60, QueueLimit = 0 };
}

public static class RateLimitPolicyNames
{
    public const string GeneralApi = "GeneralApi";
    public const string Authentication = "Authentication";
    public const string OtpRequest = "OtpRequest";
    public const string OtpVerify = "OtpVerify";
    public const string Search = "Search";
    public const string ToolboxExecution = "ToolboxExecution";
    public const string PromptRender = "PromptRender";
    public const string PublicContentRead = "PublicContentRead";
    public const string AdminMutation = "AdminMutation";
}

public static class SecurityErrorCodes
{
    public const string RateLimitExceeded = "security_rate_limit_exceeded";
    public const string RequestTooLarge = "security_request_too_large";
    public const string CorrelationIdInvalid = "security_correlation_id_invalid";
    public const string ConfigurationInvalid = "security_configuration_invalid";
    public const string CorsOriginInvalid = "security_cors_origin_invalid";
    public const string ForwardedHeaderInvalid = "security_forwarded_header_invalid";
    public const string JwtConfigurationInvalid = "security_jwt_configuration_invalid";
}
