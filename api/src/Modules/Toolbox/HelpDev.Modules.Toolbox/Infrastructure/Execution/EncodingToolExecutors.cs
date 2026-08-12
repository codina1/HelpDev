using System.Text;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

public sealed class Base64EncodeToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.Base64Encode;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);

        var encoding = ToolExecutorHelpers.OptionalString(root, "encoding", 20) ?? "utf-8";
        if (!string.Equals(encoding, "utf-8", StringComparison.OrdinalIgnoreCase))
        {
            throw new ToolboxException(
                "Only utf-8 encoding is supported.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        return Task.FromResult(ToolExecutorHelpers.ToOutput(new { value = encoded }));
    }
}

public sealed class Base64DecodeToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.Base64Decode;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var value = ToolExecutorHelpers.RequireString(root, "value", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(value);

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            throw new ToolboxException("Base64 value is invalid.", ToolboxApplicationErrorCodes.Base64Invalid);
        }

        string decoded;
        try
        {
            decoded = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                .GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            throw new ToolboxException("Decoded bytes are not valid UTF-8.", ToolboxApplicationErrorCodes.Utf8Invalid);
        }

        return Task.FromResult(ToolExecutorHelpers.ToOutput(new { text = decoded }));
    }
}

public sealed class UrlEncodeToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.UrlEncode;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);
        var encoded = Uri.EscapeDataString(text);
        return Task.FromResult(ToolExecutorHelpers.ToOutput(new { value = encoded }));
    }
}

public sealed class UrlDecodeToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.UrlDecode;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);

        try
        {
            var decoded = Uri.UnescapeDataString(text);
            return Task.FromResult(ToolExecutorHelpers.ToOutput(new { value = decoded }));
        }
        catch (UriFormatException)
        {
            throw new ToolboxException(
                "URL-encoded text is invalid.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }
    }
}
