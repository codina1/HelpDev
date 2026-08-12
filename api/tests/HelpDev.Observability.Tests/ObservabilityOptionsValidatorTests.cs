using HelpDev.Infrastructure.Observability.HealthChecks;

using HelpDev.SharedContracts.Observability;

using Microsoft.Extensions.Options;



namespace HelpDev.Observability.Tests;



public sealed class ObservabilityOptionsValidatorTests

{

    private readonly ObservabilityOptionsValidator _validator = new();



    [Fact]

    public void Validate_succeeds_for_default_options()

    {

        var result = _validator.Validate(null, new ObservabilityOptions());



        Assert.Equal(ValidateOptionsResult.Success, result);

    }



    [Fact]

    public void Validate_fails_when_cache_seconds_are_not_positive()

    {

        var options = ValidOptions();

        options.PublicHealthCacheSeconds = 0;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("cache", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_global_timeout_or_concurrency_invalid()

    {

        var options = ValidOptions();

        options.GlobalTimeoutSeconds = 0;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("timeout", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_postgresql_latency_thresholds_invalid()

    {

        var options = ValidOptions();

        options.PostgreSql.DegradedLatencyMilliseconds = 2000;

        options.PostgreSql.UnhealthyLatencyMilliseconds = 500;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("PostgreSQL", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_outbox_critical_does_not_exceed_warning()

    {

        var options = ValidOptions();

        options.Outbox.WarningPendingCount = 200;

        options.Outbox.CriticalPendingCount = 100;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("Outbox", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_search_critical_does_not_exceed_warning()

    {

        var options = ValidOptions();

        options.Search.WarningPendingCount = 200;

        options.Search.CriticalPendingCount = 100;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("Search", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_analytics_critical_does_not_exceed_warning()

    {

        var options = ValidOptions();

        options.Analytics.WarningFailureCount = 200;

        options.Analytics.CriticalFailureCount = 100;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("Analytics", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    [Fact]

    public void Validate_fails_when_slow_request_thresholds_invalid()

    {

        var options = ValidOptions();

        options.SlowRequests.WarningThresholdMilliseconds = 5000;

        options.SlowRequests.ErrorThresholdMilliseconds = 1000;



        var result = _validator.Validate(null, options);



        Assert.True(result.Failed);

        Assert.Contains("Slow request", result.FailureMessage, StringComparison.OrdinalIgnoreCase);

    }



    private static ObservabilityOptions ValidOptions() => new();

}


