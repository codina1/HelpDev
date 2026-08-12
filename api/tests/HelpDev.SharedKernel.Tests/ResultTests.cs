using HelpDev.SharedKernel.Results;

namespace HelpDev.SharedKernel.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Map_transforms_successful_value()
    {
        var result = Result<int>.Success(2);

        var mapped = result.Map(value => value * 3);

        Assert.True(mapped.IsSuccess);
        Assert.Equal(6, mapped.Value);
    }

    [Fact]
    public void Map_on_failure_preserves_error_and_skips_mapper()
    {
        var error = new Error("code", "failed");
        var mapperCalled = false;

        var mapped = Result<int>.Failure(error).Map(value =>
        {
            mapperCalled = true;
            return value * 3;
        });

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
        Assert.False(mapperCalled);
    }

    [Fact]
    public void Bind_chains_successful_results()
    {
        var result = Result<int>.Success(2)
            .Bind(value => Result<string>.Success($"n={value}"));

        Assert.True(result.IsSuccess);
        Assert.Equal("n=2", result.Value);
    }

    [Fact]
    public void Bind_on_failure_short_circuits()
    {
        var error = new Error("code", "failed");
        var binderCalled = false;

        var result = Result<int>.Failure(error).Bind(value =>
        {
            binderCalled = true;
            return Result<string>.Success(value.ToString());
        });

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.False(binderCalled);
    }

    [Fact]
    public void Match_invokes_success_branch()
    {
        var matched = Result<int>.Success(5).Match(
            onSuccess: value => $"ok:{value}",
            onFailure: _ => "fail");

        Assert.Equal("ok:5", matched);
    }

    [Fact]
    public void Match_invokes_failure_branch()
    {
        var error = new Error("code", "failed");

        var matched = Result<int>.Failure(error).Match(
            onSuccess: _ => "ok",
            onFailure: err => err.Code);

        Assert.Equal("code", matched);
    }

    [Fact]
    public void Tap_runs_side_effect_on_success()
    {
        var tapped = 0;

        var result = Result<int>.Success(4).Tap(value => tapped = value);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, tapped);
        Assert.Equal(4, result.Value);
    }

    [Fact]
    public void Tap_skips_side_effect_on_failure()
    {
        var tapped = false;
        var error = new Error("code", "failed");

        var result = Result<int>.Failure(error).Tap(_ => tapped = true);

        Assert.True(result.IsFailure);
        Assert.False(tapped);
        Assert.Equal(error, result.Error);
    }
}
