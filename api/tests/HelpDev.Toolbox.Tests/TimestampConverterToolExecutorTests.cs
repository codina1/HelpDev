using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class TimestampConverterToolExecutorTests
{
    private readonly TimestampConverterToolExecutor _sut = new();

    [Fact]
    public async Task Execute_converts_unix_seconds()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"unixSeconds":0}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal("1970-01-01T00:00:00Z", ToolExecutionTestHelpers.GetString(output.Payload, "isoUtc"));
        Assert.Equal(0, output.Payload.GetProperty("unixSeconds").GetInt64());
        Assert.Equal(0, output.Payload.GetProperty("unixMilliseconds").GetInt64());
    }

    [Fact]
    public async Task Execute_converts_iso_utc()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"isoUtc":"2026-07-19T12:00:00Z"}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal("2026-07-19T12:00:00Z", ToolExecutionTestHelpers.GetString(output.Payload, "isoUtc"));
        Assert.Equal(1784462400, output.Payload.GetProperty("unixSeconds").GetInt64());
    }

    [Fact]
    public async Task Execute_rejects_invalid_input()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"isoUtc":"not-a-timestamp"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.TimestampInvalid, ex.Code);
    }
}
