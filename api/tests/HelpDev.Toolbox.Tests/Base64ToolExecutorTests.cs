using System.Text;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class Base64ToolExecutorTests
{
    private readonly Base64EncodeToolExecutor _encode = new();
    private readonly Base64DecodeToolExecutor _decode = new();

    [Fact]
    public async Task Encode_and_decode_roundtrip()
    {
        var original = "Hello, Toolbox!";
        var encodeInput = ToolExecutionTestHelpers.ParseInput(
            $$"""{"text":"{{original}}","encoding":"utf-8"}""");

        var encoded = await _encode.ExecuteAsync(encodeInput);
        var value = ToolExecutionTestHelpers.GetString(encoded.Payload, "value");

        var decodeInput = ToolExecutionTestHelpers.ParseInput(
            $$"""{"value":"{{value}}"}""");
        var decoded = await _decode.ExecuteAsync(decodeInput);

        Assert.Equal(original, ToolExecutionTestHelpers.GetString(decoded.Payload, "text"));
    }

    [Fact]
    public async Task Decode_rejects_invalid_base64()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"value":"$$$not-base64$$$"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _decode.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.Base64Invalid, ex.Code);
    }

    [Fact]
    public async Task Decode_rejects_invalid_utf8()
    {
        var invalidUtf8 = Convert.ToBase64String(new byte[] { 0xFF, 0xFE, 0xFD });
        var input = ToolExecutionTestHelpers.ParseInput(
            $$"""{"value":"{{invalidUtf8}}"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _decode.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.Utf8Invalid, ex.Code);
    }

    [Fact]
    public async Task Encode_rejects_non_utf8_encoding()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"hi","encoding":"ascii"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _encode.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.ExecutionInputInvalid, ex.Code);
    }
}
