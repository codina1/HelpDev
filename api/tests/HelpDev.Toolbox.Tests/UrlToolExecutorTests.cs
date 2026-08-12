using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class UrlToolExecutorTests
{
    private readonly UrlEncodeToolExecutor _encode = new();
    private readonly UrlDecodeToolExecutor _decode = new();

    [Fact]
    public async Task Encode_and_decode_roundtrip()
    {
        var original = "hello world & more=values";
        var encodeInput = ToolExecutionTestHelpers.ParseInput(
            $$"""{"text":"{{original}}"}""");

        var encoded = await _encode.ExecuteAsync(encodeInput);
        var value = ToolExecutionTestHelpers.GetString(encoded.Payload, "value");
        Assert.DoesNotContain(" ", value, StringComparison.Ordinal);

        var decodeInput = ToolExecutionTestHelpers.ParseInput(
            $$"""{"text":"{{value}}"}""");
        var decoded = await _decode.ExecuteAsync(decodeInput);

        Assert.Equal(original, ToolExecutionTestHelpers.GetString(decoded.Payload, "value"));
    }
}
