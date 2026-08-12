using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class JsonFormatterToolExecutorTests
{
    private readonly JsonFormatterToolExecutor _sut = new();

    [Fact]
    public async Task Execute_formats_valid_json_with_indent()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"{\"a\":1}","indent":true}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.True(ToolExecutionTestHelpers.GetBool(output.Payload, "isValid"));
        var formatted = ToolExecutionTestHelpers.GetString(output.Payload, "formatted");
        Assert.Contains("\n", formatted, StringComparison.Ordinal);
        Assert.Contains("\"a\"", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Execute_returns_invalid_for_bad_json()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"{bad","indent":true}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.False(ToolExecutionTestHelpers.GetBool(output.Payload, "isValid"));
        Assert.False(output.Payload.GetProperty("formatted").ValueKind == System.Text.Json.JsonValueKind.String);
        Assert.False(string.IsNullOrWhiteSpace(ToolExecutionTestHelpers.GetString(output.Payload, "error")));
    }
}
