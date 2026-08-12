using System.Text;
using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

internal static class ToolExecutorHelpers
{
    public static readonly JsonSerializerOptions CompactJson = new()
    {
        WriteIndented = false,
    };

    public static readonly JsonSerializerOptions PrettyJson = new()
    {
        WriteIndented = true,
    };

    public static JsonElement RequireObject(ToolExecutionInput input)
    {
        if (input.Payload.ValueKind != JsonValueKind.Object)
        {
            throw new ToolboxException(
                "Execution input must be a JSON object.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        return input.Payload;
    }

    public static string RequireString(JsonElement root, string propertyName, int maxLength)
    {
        if (!root.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            throw new ToolboxException(
                $"Property '{propertyName}' is required.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        var value = property.GetString() ?? string.Empty;
        EnsureTextLength(value, maxLength);
        return value;
    }

    public static string? OptionalString(JsonElement root, string propertyName, int maxLength)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new ToolboxException(
                $"Property '{propertyName}' must be a string.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        var value = property.GetString() ?? string.Empty;
        EnsureTextLength(value, maxLength);
        return value;
    }

    public static bool OptionalBool(JsonElement root, string propertyName, bool defaultValue)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return defaultValue;
        }

        if (property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new ToolboxException(
                $"Property '{propertyName}' must be a boolean.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        return property.GetBoolean();
    }

    public static int OptionalInt(JsonElement root, string propertyName, int defaultValue)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return defaultValue;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out var value))
        {
            throw new ToolboxException(
                $"Property '{propertyName}' must be an integer.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        return value;
    }

    public static void EnsureTextLength(string value, int maxLength)
    {
        if (value.Length > maxLength)
        {
            throw new ToolboxException(
                "Input text exceeds the maximum allowed length.",
                ToolboxApplicationErrorCodes.ExecutionInputTooLarge);
        }
    }

    public static void EnsureUtf8ByteLimit(string value)
    {
        var bytes = Encoding.UTF8.GetByteCount(value);
        if (bytes > ToolboxLimits.MaxRequestBytes)
        {
            throw new ToolboxException(
                "Input exceeds the maximum allowed size.",
                ToolboxApplicationErrorCodes.ExecutionInputTooLarge);
        }
    }

    public static ToolExecutionOutput ToOutput(object payload)
    {
        var json = JsonSerializer.SerializeToElement(payload, CompactJson);
        var raw = json.GetRawText();
        if (raw.Length > ToolboxLimits.MaxOutputLength)
        {
            throw new ToolboxException(
                "Execution output exceeds the maximum allowed length.",
                ToolboxApplicationErrorCodes.ExecutionOutputTooLarge);
        }

        return new ToolExecutionOutput(json);
    }

    public static CancellationToken ThrowIfCancellationRequested(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return cancellationToken;
    }
}
