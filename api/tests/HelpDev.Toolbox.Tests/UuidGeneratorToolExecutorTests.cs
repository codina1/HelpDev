using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class UuidGeneratorToolExecutorTests
{
    private readonly UuidGeneratorToolExecutor _sut = new();

    [Fact]
    public async Task Execute_returns_one_uuid_by_default()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{}""");

        var output = await _sut.ExecuteAsync(input);
        var values = output.Payload.GetProperty("values");

        Assert.Equal(JsonValueKind.Array, values.ValueKind);
        Assert.Equal(1, values.GetArrayLength());
        Assert.True(Guid.TryParse(values[0].GetString(), out _));
    }

    [Fact]
    public async Task Execute_returns_one_hundred_uuids()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"count":100}""");

        var output = await _sut.ExecuteAsync(input);
        Assert.Equal(100, output.Payload.GetProperty("values").GetArrayLength());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Execute_rejects_invalid_count(int count)
    {
        var input = ToolExecutionTestHelpers.ParseInput($$"""{"count":{{count}}}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.UuidCountInvalid, ex.Code);
    }

    [Fact]
    public async Task Execute_rejects_invalid_format()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"format":"X"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.ExecutionInputInvalid, ex.Code);
    }
}
