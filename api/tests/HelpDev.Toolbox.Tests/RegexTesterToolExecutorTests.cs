using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class RegexTesterToolExecutorTests
{
    private readonly RegexTesterToolExecutor _sut = new();

    [Fact]
    public async Task Execute_returns_matches()
    {
        var input = ToolExecutionTestHelpers.ParseInput(
            """{"pattern":"\\d+","text":"a1b22c","options":["IgnoreCase"]}""");

        var output = await _sut.ExecuteAsync(input);

        Assert.Equal(2, ToolExecutionTestHelpers.GetInt(output.Payload, "matchCount"));
        Assert.Equal(2, output.Payload.GetProperty("matches").GetArrayLength());
    }

    [Fact]
    public async Task Execute_rejects_invalid_pattern()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"pattern":"(","text":"abc"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.RegexPatternInvalid, ex.Code);
    }

    [Fact]
    public async Task Execute_times_out_on_catastrophic_backtracking()
    {
        // Non-matching suffix forces catastrophic backtracking on (a+)+.
        var longText = new string('a', ToolboxLimits.MaxRegexTextLength - 1) + "X";
        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                pattern = "(a+)+$",
                text = longText,
                timeoutMs = ToolboxLimits.MinRegexTimeoutMs,
            }));
        var input = new ToolExecutionInput(document.RootElement.Clone());

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.RegexTimeout, ex.Code);
    }

    [Fact]
    public async Task Execute_rejects_invalid_options()
    {
        var input = ToolExecutionTestHelpers.ParseInput(
            """{"pattern":"a","text":"a","options":["ECMAScript"]}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.RegexOptionsInvalid, ex.Code);
    }

    [Fact]
    public async Task Execute_never_allows_compiled()
    {
        var input = ToolExecutionTestHelpers.ParseInput(
            """{"pattern":"a","text":"a","options":["Compiled"]}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.RegexOptionsInvalid, ex.Code);
    }
}
