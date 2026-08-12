using System.Security.Cryptography;
using System.Text;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class HashGeneratorToolExecutorTests
{
    private readonly HashGeneratorToolExecutor _sut = new();

    [Theory]
    [InlineData("SHA256")]
    [InlineData("SHA384")]
    [InlineData("SHA512")]
    public async Task Execute_supports_allowed_algorithms(string algorithm)
    {
        var text = "deterministic-input";
        var input = ToolExecutionTestHelpers.ParseInput(
            $$"""{"text":"{{text}}","algorithm":"{{algorithm}}"}""");

        var output = await _sut.ExecuteAsync(input);
        var expected = Convert.ToHexString(algorithm switch
        {
            "SHA256" => SHA256.HashData(Encoding.UTF8.GetBytes(text)),
            "SHA384" => SHA384.HashData(Encoding.UTF8.GetBytes(text)),
            _ => SHA512.HashData(Encoding.UTF8.GetBytes(text)),
        }).ToLowerInvariant();

        Assert.Equal(algorithm, ToolExecutionTestHelpers.GetString(output.Payload, "algorithm"));
        Assert.Equal(expected, ToolExecutionTestHelpers.GetString(output.Payload, "hex"));
    }

    [Fact]
    public async Task Execute_rejects_invalid_algorithm()
    {
        var input = ToolExecutionTestHelpers.ParseInput("""{"text":"x","algorithm":"MD5"}""");

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.ExecuteAsync(input));
        Assert.Equal(ToolboxApplicationErrorCodes.HashAlgorithmInvalid, ex.Code);
    }

    [Fact]
    public async Task Execute_is_deterministic()
    {
        var inputJson = """{"text":"same","algorithm":"SHA256"}""";

        var first = await _sut.ExecuteAsync(ToolExecutionTestHelpers.ParseInput(inputJson));
        var second = await _sut.ExecuteAsync(ToolExecutionTestHelpers.ParseInput(inputJson));

        Assert.Equal(
            ToolExecutionTestHelpers.GetString(first.Payload, "hex"),
            ToolExecutionTestHelpers.GetString(second.Payload, "hex"));
    }
}
