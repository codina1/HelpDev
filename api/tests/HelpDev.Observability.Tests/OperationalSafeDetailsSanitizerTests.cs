using HelpDev.Infrastructure.Observability;



namespace HelpDev.Observability.Tests;



public sealed class OperationalSafeDetailsSanitizerTests

{

    private readonly OperationalSafeDetailsSanitizer _sanitizer = new();



    [Fact]

    public void Sanitize_null_or_empty_returns_null()

    {

        Assert.Null(_sanitizer.Sanitize(null));

        Assert.Null(_sanitizer.Sanitize(new Dictionary<string, string>()));

    }



    [Fact]

    public void Sanitize_allows_whitelisted_keys()

    {

        var result = _sanitizer.Sanitize(new Dictionary<string, string>

        {

            ["connectivity"] = "available",

            ["latencyBucket"] = "under_50ms",

            ["scope"] = "Instance",

        });



        Assert.NotNull(result);

        Assert.Equal(3, result!.Count);

        Assert.Equal("available", result["connectivity"]);

    }



    [Fact]

    public void Sanitize_filters_disallowed_keys()

    {

        var result = _sanitizer.Sanitize(new Dictionary<string, string>

        {

            ["connectivity"] = "available",

            ["host"] = "db.internal",

            ["customField"] = "value",

        });



        Assert.NotNull(result);

        Assert.Single(result!);

        Assert.Equal("available", result["connectivity"]);

    }



    [Fact]

    public void Sanitize_filters_sensitive_values()

    {

        var result = _sanitizer.Sanitize(new Dictionary<string, string>

        {

            ["connectivity"] = "password leaked",

            ["processorEnabled"] = "True",

        });



        Assert.NotNull(result);

        Assert.Single(result!);

        Assert.Equal("True", result["processorEnabled"]);

    }



    [Fact]

    public void Sanitize_returns_null_when_too_many_entries()

    {

        var details = Enumerable.Range(1, 11)

            .ToDictionary(i => $"key{i}", i => "value");



        Assert.Null(_sanitizer.Sanitize(details));

    }



    [Fact]

    public void Sanitize_filters_control_characters()

    {

        var result = _sanitizer.Sanitize(new Dictionary<string, string>

        {

            ["scope"] = "Instance\u0001",

        });



        Assert.Null(result);

    }

}


