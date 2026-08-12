using HelpDev.API.Deployment;
using HelpDev.API.Security;
using HelpDev.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;

namespace HelpDev.API.Tests.Deployment;

[Trait("Category", "ProductionSafety")]
public sealed class ProductionSafetyValidatorTests
{
    [Fact]
    public void Safe_production_configuration_is_valid()
    {
        var result = new ProductionSafetyValidatorBuilder().Build().Validate();

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Missing_jwt_secret_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Secret = string.Empty;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JWT signing key is missing", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("changeme-changeme-changeme-change")]
    [InlineData("your-secret-here-your-secret-here")]
    [InlineData("HelpDev_Dev_Secret_Key_Change_In_Production_32+")]
    public void Placeholder_jwt_secret_is_rejected(string secret)
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Secret = secret;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("placeholder", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Errors, e => e.Contains(secret, StringComparison.Ordinal));
    }

    [Fact]
    public void Short_jwt_secret_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Secret = "short-key";

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("at least 32", StringComparison.Ordinal));
    }

    [Fact]
    public void Missing_partition_key_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.PartitionHashKey = string.Empty;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("partition HMAC key is missing", StringComparison.Ordinal));
    }

    [Fact]
    public void Identical_jwt_and_partition_secrets_are_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.PartitionHashKey = ProductionSafetyValidatorBuilder.StrongJwtSecret;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("must not be identical", StringComparison.Ordinal));
    }

    [Fact]
    public void Deterministic_test_otp_provider_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Auth.ExposeOtpInResponse = true;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("OTP", StringComparison.Ordinal));
    }

    [Fact]
    public void Deterministic_test_otp_provider_is_allowed_outside_production()
    {
        var builder = new ProductionSafetyValidatorBuilder { Environment = Environments.Development };
        builder.Auth.ExposeOtpInResponse = true;

        var result = builder.Build().Validate();

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Wildcard_cors_origin_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.AllowedCorsOrigins = ["*"];

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("wildcard", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Invalid_cors_origin_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.AllowedCorsOrigins = ["not-a-uri"];

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("CORS origin is invalid", StringComparison.Ordinal));
    }

    [Fact]
    public void Http_cors_origin_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.AllowedCorsOrigins = ["http://app.example.com"];

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("HTTPS in Production", StringComparison.Ordinal));
    }

    [Fact]
    public void Reverse_proxy_enabled_without_trusted_proxies_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.ReverseProxy = new ReverseProxyOptions { Enabled = true, RequireKnownProxyConfiguration = true };

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Trusted proxy configuration is required", StringComparison.Ordinal));
    }

    [Fact]
    public void Production_swagger_enabled_unintentionally_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.OpenApi.Enabled = true;
        builder.OpenApi.EnableUi = true;
        builder.OpenApi.EnableInProduction = false;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Swagger UI", StringComparison.Ordinal));
    }

    [Fact]
    public void Ef_sensitive_logging_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.ExtraConfiguration["Logging:EnableSensitiveDataLogging"] = "true";

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("sensitive-data logging", StringComparison.Ordinal));
    }

    [Fact]
    public void Debug_log_level_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.ExtraConfiguration["Logging:LogLevel:Default"] = "Debug";

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Debug or Trace", StringComparison.Ordinal));
    }

    [Fact]
    public void Development_seed_mode_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Database.SeedMode = DatabaseSeedMode.DevelopmentDemo;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("DevelopmentDemo seed mode is forbidden", StringComparison.Ordinal));
    }

    [Fact]
    public void Missing_connection_string_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder { ConnectionString = null };

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("PostgreSQL connection is not configured", StringComparison.Ordinal));
    }

    [Fact]
    public void Test_database_in_production_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder
        {
            ConnectionString = "Host=db;Port=5432;Database=helpdev_it_123;Username=app;Password=StrongDbPassword123",
        };

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("test database", StringComparison.Ordinal));
    }

    [Fact]
    public void Require_https_metadata_disabled_is_rejected_in_production()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.RequireHttpsMetadata = false;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("RequireHttpsMetadata", StringComparison.Ordinal));
    }

    [Fact]
    public void Errors_never_contain_secret_values()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Secret = "changeme"; // placeholder + short + will be flagged
        builder.Security.PartitionHashKey = "password"; // placeholder + short

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.All(result.Errors, e =>
        {
            Assert.DoesNotContain("changeme", e, StringComparison.Ordinal);
            Assert.DoesNotContain("password", e, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Missing_jwt_issuer_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Issuer = string.Empty;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JWT issuer", StringComparison.Ordinal));
    }

    [Fact]
    public void Missing_jwt_audience_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.Audience = string.Empty;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JWT audience", StringComparison.Ordinal));
    }

    [Fact]
    public void Invalid_jwt_expiration_is_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Jwt.ExpirationMinutes = 0;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("JWT expiration", StringComparison.Ordinal));
    }

    [Fact]
    public void Apply_migration_mode_in_production_emits_warning_not_error()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Database.MigrationMode = DatabaseMigrationMode.Apply;

        var result = builder.Build().Validate();

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.Contains(result.Warnings, w => w.Contains("migration mode is 'Apply'", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_migration_mode_and_none_seed_remain_safe()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Database.MigrationMode = DatabaseMigrationMode.Validate;
        builder.Database.SeedMode = DatabaseSeedMode.None;

        var result = builder.Build().Validate();

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.DoesNotContain(result.Warnings, w => w.Contains("migration mode is 'Apply'", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("test-secret-test-secret-test-secret!!")]
    [InlineData("password-password-password-password")]
    [InlineData("replace-me-replace-me-replace-me-xx")]
    public void Invalid_partition_secret_placeholders_are_rejected(string secret)
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.PartitionHashKey = secret;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("partition HMAC key", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Errors, e => e.Contains(secret, StringComparison.Ordinal));
    }

    [Fact]
    public void Zero_request_body_limits_are_rejected()
    {
        var builder = new ProductionSafetyValidatorBuilder();
        builder.Security.DefaultRequestBodyLimitBytes = 0;

        var result = builder.Build().Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Request body limits", StringComparison.Ordinal));
    }
}
