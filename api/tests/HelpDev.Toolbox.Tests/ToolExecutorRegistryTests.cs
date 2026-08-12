using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Modules.Toolbox.Infrastructure.Execution;

namespace HelpDev.Toolbox.Tests;

public sealed class ToolExecutorRegistryTests
{
    [Fact]
    public void GetRequired_resolves_all_registered_types()
    {
        var registry = CreateFullRegistry();

        foreach (ToolType type in Enum.GetValues<ToolType>())
        {
            var executor = registry.GetRequired(type);
            Assert.Equal(type, executor.Type);
        }
    }

    [Fact]
    public void GetRequired_throws_for_unsupported_type()
    {
        var registry = new ToolExecutorRegistry([new JsonFormatterToolExecutor()]);

        var ex = Assert.Throws<ToolboxException>(() => registry.GetRequired(ToolType.RegexTester));
        Assert.Equal(ToolboxApplicationErrorCodes.ExecutionTypeUnsupported, ex.Code);
    }

    [Fact]
    public void Constructor_rejects_duplicate_registration()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ToolExecutorRegistry(
            [
                new JsonFormatterToolExecutor(),
                new JsonFormatterToolExecutor(),
            ]));

        Assert.Contains(nameof(ToolType.JsonFormatter), ex.Message, StringComparison.Ordinal);
    }

    private static ToolExecutorRegistry CreateFullRegistry() =>
        new(
        [
            new JsonFormatterToolExecutor(),
            new JsonValidatorToolExecutor(),
            new Base64EncodeToolExecutor(),
            new Base64DecodeToolExecutor(),
            new UrlEncodeToolExecutor(),
            new UrlDecodeToolExecutor(),
            new UuidGeneratorToolExecutor(),
            new HashGeneratorToolExecutor(),
            new TimestampConverterToolExecutor(),
            new TextStatisticsToolExecutor(),
            new RegexTesterToolExecutor(),
        ]);
}
