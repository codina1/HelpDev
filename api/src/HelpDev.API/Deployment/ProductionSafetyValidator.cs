using HelpDev.API.OpenApi;
using HelpDev.API.Security;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Identity.Application.Auth;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Deployment;

public sealed class ProductionSafetyValidationResult
{
    private readonly List<string> _errors = [];
    private readonly List<string> _warnings = [];

    public bool IsValid => _errors.Count == 0;

    public IReadOnlyList<string> Errors => _errors;

    public IReadOnlyList<string> Warnings => _warnings;

    public void AddError(string message) => _errors.Add(message);

    public void AddWarning(string message) => _warnings.Add(message);
}

/// <summary>
/// Centralized production configuration safety validation. Aggregates critical configuration
/// failures so startup can fail fast before serving traffic. Never includes secret values in messages.
/// </summary>
public interface IProductionSafetyValidator
{
    ProductionSafetyValidationResult Validate();
}

public sealed class ProductionSafetyValidator : IProductionSafetyValidator
{
    private static readonly string[] PlaceholderSecrets =
    [
        "changeme", "secret", "password", "test", "dev-secret",
        "your-secret-here", "replace-me", "change_in_production", "dev_secret",
    ];

    private const int MinSecretLength = 32;

    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly JwtSettings _jwt;
    private readonly AuthSettings _auth;
    private readonly SecurityOptions _security;
    private readonly OpenApiOptions _openApi;
    private readonly ReverseProxyOptions _reverseProxy;
    private readonly HttpsPolicyOptions _https;
    private readonly DatabaseStartupOptions _database;

    public ProductionSafetyValidator(
        IHostEnvironment environment,
        IConfiguration configuration,
        IOptions<JwtSettings> jwt,
        IOptions<AuthSettings> auth,
        IOptions<SecurityOptions> security,
        IOptions<OpenApiOptions> openApi,
        IOptions<ReverseProxyOptions> reverseProxy,
        IOptions<HttpsPolicyOptions> https,
        IOptions<DatabaseStartupOptions> database)
    {
        _environment = environment;
        _configuration = configuration;
        _jwt = jwt.Value;
        _auth = auth.Value;
        _security = security.Value;
        _openApi = openApi.Value;
        _reverseProxy = reverseProxy.Value;
        _https = https.Value;
        _database = database.Value;
    }

    public ProductionSafetyValidationResult Validate()
    {
        var result = new ProductionSafetyValidationResult();

        ValidateConnection(result);
        ValidateSecrets(result);
        ValidateJwtIdentity(result);
        ValidateCors(result);
        ValidateOtpProvider(result);
        ValidateReverseProxy(result);
        ValidateHttps(result);
        ValidateOpenApi(result);
        ValidateLogging(result);
        ValidateDatabasePolicy(result);
        ValidateRequestLimits(result);

        return result;
    }

    private void ValidateConnection(ProductionSafetyValidationResult result)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            result.AddError("PostgreSQL connection is not configured.");
            return;
        }

        try
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.Database))
            {
                result.AddError("PostgreSQL database name is not configured.");
            }

            if (_environment.IsProduction()
                && !string.IsNullOrWhiteSpace(builder.Database)
                && LooksLikeTestDatabase(builder.Database))
            {
                result.AddError("PostgreSQL connection appears to point at a test database in Production.");
            }
        }
        catch (Exception)
        {
            result.AddError("PostgreSQL connection string is malformed.");
        }
    }

    private static bool LooksLikeTestDatabase(string database) =>
        database.Contains("test", StringComparison.OrdinalIgnoreCase)
        || database.Contains("integration", StringComparison.OrdinalIgnoreCase)
        || database.StartsWith("helpdev_it_", StringComparison.OrdinalIgnoreCase);

    private void ValidateSecrets(ProductionSafetyValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Secret))
        {
            result.AddError("JWT signing key is missing.");
        }
        else
        {
            if (_jwt.Secret.Trim().Length < MinSecretLength)
            {
                result.AddError($"JWT signing key must be at least {MinSecretLength} characters.");
            }

            if (IsPlaceholder(_jwt.Secret))
            {
                result.AddError("JWT signing key uses a forbidden placeholder value.");
            }
        }

        if (string.IsNullOrWhiteSpace(_security.PartitionHashKey))
        {
            result.AddError("Security partition HMAC key is missing.");
        }
        else
        {
            if (_security.PartitionHashKey.Trim().Length < MinSecretLength)
            {
                result.AddError($"Security partition HMAC key must be at least {MinSecretLength} characters.");
            }

            if (IsPlaceholder(_security.PartitionHashKey))
            {
                result.AddError("Security partition HMAC key uses a forbidden placeholder value.");
            }
        }

        if (!string.IsNullOrWhiteSpace(_jwt.Secret)
            && !string.IsNullOrWhiteSpace(_security.PartitionHashKey)
            && string.Equals(_jwt.Secret, _security.PartitionHashKey, StringComparison.Ordinal))
        {
            result.AddError("JWT signing key and partition HMAC key must not be identical.");
        }
    }

    private void ValidateJwtIdentity(ProductionSafetyValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(_jwt.Issuer))
        {
            result.AddError("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_jwt.Audience))
        {
            result.AddError("JWT audience is not configured.");
        }

        if (_jwt.ExpirationMinutes is <= 0 or > 24 * 60)
        {
            result.AddError("JWT expiration must be between 1 minute and 24 hours.");
        }
    }

    private void ValidateCors(ProductionSafetyValidationResult result)
    {
        var origins = _security.AllowedCorsOrigins;

        if (origins.Any(o => o == "*"))
        {
            result.AddError("CORS origins are unsafe: wildcard origins are not allowed.");
        }

        foreach (var origin in origins)
        {
            if (origin == "*")
            {
                continue;
            }

            if (!SecurityOptionsValidator.TryValidateOrigin(origin, out var error))
            {
                result.AddError($"CORS origin is invalid: {error}");
                continue;
            }

            if (_environment.IsProduction()
                && origin.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !IsLocalhost(origin))
            {
                result.AddError("CORS origins must use HTTPS in Production (non-localhost).");
            }
        }

        if (origins.Length > 50)
        {
            result.AddError("CORS origin count exceeds the supported maximum (50).");
        }
    }

    private static bool IsLocalhost(string origin) =>
        origin.Contains("localhost", StringComparison.OrdinalIgnoreCase)
        || origin.Contains("127.0.0.1", StringComparison.Ordinal);

    private void ValidateOtpProvider(ProductionSafetyValidationResult result)
    {
        if (_environment.IsProduction() && _auth.ExposeOtpInResponse)
        {
            result.AddError("Production OTP provider cannot expose OTP codes in responses (deterministic test mode).");
        }
    }

    private void ValidateReverseProxy(ProductionSafetyValidationResult result)
    {
        if (!_reverseProxy.Enabled)
        {
            return;
        }

        if (_reverseProxy.RequireKnownProxyConfiguration && !_reverseProxy.HasTrustedProxies)
        {
            result.AddError("Trusted proxy configuration is required when reverse proxy mode is enabled.");
        }

        if (_reverseProxy.ForwardLimit is < 1 or > 8)
        {
            result.AddError("Reverse proxy ForwardLimit is out of the safe range (1-8).");
        }
    }

    private void ValidateHttps(ProductionSafetyValidationResult result)
    {
        if (_environment.IsProduction() && !_security.RequireHttpsMetadata)
        {
            result.AddError("RequireHttpsMetadata must be enabled in Production.");
        }

        if (_https.EnableHsts && _https.HstsMaxAgeDays <= 0)
        {
            result.AddError("HSTS is enabled but max-age is not positive.");
        }
    }

    private void ValidateOpenApi(ProductionSafetyValidationResult result)
    {
        if (!_environment.IsProduction())
        {
            return;
        }

        if (_openApi.Enabled && _openApi.EnableUi && !_openApi.EnableInProduction)
        {
            result.AddError("Swagger UI must not be enabled in Production unless EnableInProduction is explicitly set.");
        }
    }

    private void ValidateLogging(ProductionSafetyValidationResult result)
    {
        if (!_environment.IsProduction())
        {
            return;
        }

        var sensitive = _configuration.GetValue<bool?>("Logging:EnableSensitiveDataLogging");
        if (sensitive == true)
        {
            result.AddError("EF sensitive-data logging must not be enabled in Production.");
        }

        var detailedErrors = _configuration.GetValue<bool?>("Logging:EnableDetailedErrors");
        if (detailedErrors == true)
        {
            result.AddError("Detailed EF errors must not be enabled in Production.");
        }

        var defaultLevel = _configuration["Logging:LogLevel:Default"];
        if (!string.IsNullOrWhiteSpace(defaultLevel)
            && (defaultLevel.Equals("Debug", StringComparison.OrdinalIgnoreCase)
                || defaultLevel.Equals("Trace", StringComparison.OrdinalIgnoreCase)))
        {
            result.AddError("Production default log level must not be Debug or Trace.");
        }
    }

    private void ValidateDatabasePolicy(ProductionSafetyValidationResult result)
    {
        var migrationMode = _database.ResolveMigrationMode(_environment.IsProduction());
        var seedMode = _database.ResolveSeedMode(_environment.IsDevelopment());

        if (_environment.IsProduction() && seedMode == DatabaseSeedMode.DevelopmentDemo)
        {
            result.AddError("DevelopmentDemo seed mode is forbidden in Production.");
        }

        if (_environment.IsProduction() && migrationMode == DatabaseMigrationMode.Apply)
        {
            result.AddWarning("Database migration mode is 'Apply' in Production; ensure this is a controlled deployment.");
        }
    }

    private void ValidateRequestLimits(ProductionSafetyValidationResult result)
    {
        if (_security.DefaultRequestBodyLimitBytes <= 0 || _security.MaxJsonRequestBodyLimitBytes <= 0)
        {
            result.AddError("Request body limits must be positive.");
        }
    }

    private static bool IsPlaceholder(string value)
    {
        var trimmed = value.Trim();
        return PlaceholderSecrets.Any(p => trimmed.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
