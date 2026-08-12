using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class TextStatisticsToolExecutorTests
{
    private readonly TextStatisticsToolExecutor _sut = new();

    [Fact]
    public async Task Execute_handles_empty_text()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":""}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal(0, ToolExecutionTestHelpers.GetInt(output.Payload, "characterCount"));
        Assert.Equal(0, ToolExecutionTestHelpers.GetInt(output.Payload, "wordCount"));
        Assert.Equal(0, ToolExecutionTestHelpers.GetInt(output.Payload, "lineCount"));
        Assert.Equal(0, ToolExecutionTestHelpers.GetInt(output.Payload, "byteCountUtf8"));
    }

    [Fact]
    public async Task Execute_counts_multiline_text()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"one\ntwo\nthree"}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal(3, ToolExecutionTestHelpers.GetInt(output.Payload, "lineCount"));
        Assert.Equal(3, ToolExecutionTestHelpers.GetInt(output.Payload, "wordCount"));
    }

    [Fact]
    public async Task Execute_counts_unicode_bytes()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"سلام"}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal(4, ToolExecutionTestHelpers.GetInt(output.Payload, "characterCount"));
        Assert.Equal(8, ToolExecutionTestHelpers.GetInt(output.Payload, "byteCountUtf8"));
        Assert.Equal(1, ToolExecutionTestHelpers.GetInt(output.Payload, "wordCount"));
    }
}
