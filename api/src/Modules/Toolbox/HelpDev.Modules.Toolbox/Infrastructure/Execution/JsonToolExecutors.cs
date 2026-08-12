using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

public sealed class JsonFormatterToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.JsonFormatter;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxJsonLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);
        var indent = ToolExecutorHelpers.OptionalBool(root, "indent", defaultValue: true);

        try
        {
            using var document = JsonDocument.Parse(text);
            var options = indent ? ToolExecutorHelpers.PrettyJson : ToolExecutorHelpers.CompactJson;
            var formatted = JsonSerializer.Serialize(document.RootElement, options);
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                isValid = true,
                formatted,
                error = (string?)null,
            }));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                isValid = false,
                formatted = (string?)null,
                error = ex.Message,
                bytePosition = ex.BytePositionInLine,
                lineNumber = ex.LineNumber,
            }));
        }
    }
}

public sealed class JsonValidatorToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.JsonValidator;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxJsonLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);

        if (text.Length == 0)
        {
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                isValid = false,
                error = "JSON text is empty.",
                bytePosition = (long?)null,
                lineNumber = (long?)null,
            }));
        }

        try
        {
            using var _ = JsonDocument.Parse(text);
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                isValid = true,
                error = (string?)null,
                bytePosition = (long?)null,
                lineNumber = (long?)null,
            }));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                isValid = false,
                error = ex.Message,
                bytePosition = ex.BytePositionInLine,
                lineNumber = ex.LineNumber,
            }));
        }
    }
}
