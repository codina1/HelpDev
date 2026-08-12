using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class JsonValidatorToolExecutorTests
{
    private readonly JsonValidatorToolExecutor _sut = new();

    [Fact]
    public async Task Execute_marks_valid_json()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"{\"ok\":true}"}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.True(ToolExecutionTestHelpers.GetBool(output.Payload, "isValid"));
        Assert.Equal(System.Text.Json.JsonValueKind.Null, output.Payload.GetProperty("error").ValueKind);
    }

    [Fact]
    public async Task Execute_marks_invalid_json()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"{bad"}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.False(ToolExecutionTestHelpers.GetBool(output.Payload, "isValid"));
        Assert.False(string.IsNullOrWhiteSpace(ToolExecutionTestHelpers.GetString(output.Payload, "error")));
    }

    [Fact]
    public async Task Execute_marks_empty_as_invalid()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":""}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.False(ToolExecutionTestHelpers.GetBool(output.Payload, "isValid"));
        Assert.Equal("JSON text is empty.", ToolExecutionTestHelpers.GetString(output.Payload, "error"));
    }
}
