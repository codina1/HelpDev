using HelpDev.API.Deployment;
using HelpDev.Infrastructure.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Tests.Deployment;

[Trait("Category", "Deployment")]
public sealed class DeploymentOptionsValidatorTests
{
    [Fact]
    public void ReverseProxy_valid_defaults_succeed()
    {
        var validator = new ReverseProxyOptionsValidator(new FakeHostEnvironment { EnvironmentName = Environments.Production });

        var result = validator.Validate(null, new ReverseProxyOptions());

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void ReverseProxy_out_of_range_forward_limit_fails()
    {
        var validator = new ReverseProxyOptionsValidator(new FakeHostEnvironment());

        var result = validator.Validate(null, new ReverseProxyOptions { ForwardLimit = 99 });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void ReverseProxy_malformed_network_fails()
    {
        var validator = new ReverseProxyOptionsValidator(new FakeHostEnvironment());

        var result = validator.Validate(null, new ReverseProxyOptions { TrustedProxyNetworks = ["10.0.0.0"] });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
        Assert.Contains("CIDR", result.FailureMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void ReverseProxy_enabled_without_trusted_proxies_fails_in_production()
    {
        var validator = new ReverseProxyOptionsValidator(new FakeHostEnvironment { EnvironmentName = Environments.Production });

        var result = validator.Validate(null, new ReverseProxyOptions { Enabled = true, RequireKnownProxyConfiguration = true });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void Https_hsts_enabled_with_zero_max_age_fails()
    {
        var validator = new HttpsPolicyOptionsValidator();

        var result = validator.Validate(null, new HttpsPolicyOptions { EnableHsts = true, HstsMaxAgeDays = 0 });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void Shutdown_timeout_out_of_range_fails()
    {
        var validator = new ShutdownOptionsValidator();

        Assert.NotEqual(ValidateOptionsResult.Success, validator.Validate(null, new ShutdownOptions { TimeoutSeconds = 0 }));
        Assert.NotEqual(ValidateOptionsResult.Success, validator.Validate(null, new ShutdownOptions { TimeoutSeconds = 5000 }));
        Assert.Equal(ValidateOptionsResult.Success, validator.Validate(null, new ShutdownOptions { TimeoutSeconds = 30 }));
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData("v1.0.0-rc.1", true)]
    [InlineData("bad\nvalue", false)]
    [InlineData("has spaces ok", true)]
    [InlineData("weird;semicolon", false)]
    public void ReleaseMetadata_version_character_rules(string version, bool expectedValid)
    {
        var validator = new ReleaseMetadataOptionsValidator();

        var result = validator.Validate(null, new ReleaseMetadataOptions { Version = version });

        Assert.Equal(expectedValid, result == ValidateOptionsResult.Success);
    }

    [Fact]
    public void ReleaseMetadata_rejects_invalid_build_timestamp()
    {
        var validator = new ReleaseMetadataOptionsValidator();

        var result = validator.Validate(null, new ReleaseMetadataOptions { BuildTimestamp = "not-a-date" });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void ReleaseMetadata_accepts_utc_timestamp()
    {
        var validator = new ReleaseMetadataOptionsValidator();

        var result = validator.Validate(null, new ReleaseMetadataOptions { BuildTimestamp = "2026-07-21T08:00:00Z" });

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void ReleaseMetadata_rejects_overlong_commit()
    {
        var validator = new ReleaseMetadataOptionsValidator();

        var result = validator.Validate(null, new ReleaseMetadataOptions { Commit = new string('a', 128) });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }

    [Fact]
    public void Database_startup_valid_defaults_succeed()
    {
        var validator = new DatabaseStartupOptionsValidator();

        var result = validator.Validate(null, new DatabaseStartupOptions());

        Assert.Equal(ValidateOptionsResult.Success, result);
    }

    [Theory]
    [InlineData(0, 50, true)]
    [InlineData(60, 50, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 1000, false)]
    public void Database_startup_pool_bounds_validation(int min, int max, bool expectedValid)
    {
        var validator = new DatabaseStartupOptionsValidator();
        var options = new DatabaseStartupOptions();
        options.Postgres.MinPoolSize = min;
        options.Postgres.MaxPoolSize = max;

        var result = validator.Validate(null, options);

        Assert.Equal(expectedValid, result == ValidateOptionsResult.Success);
    }

    [Fact]
    public void Database_startup_invalid_migration_lock_timeout_fails()
    {
        var validator = new DatabaseStartupOptionsValidator();

        var result = validator.Validate(null, new DatabaseStartupOptions { MigrationLockTimeoutSeconds = 0 });

        Assert.NotEqual(ValidateOptionsResult.Success, result);
    }
}
