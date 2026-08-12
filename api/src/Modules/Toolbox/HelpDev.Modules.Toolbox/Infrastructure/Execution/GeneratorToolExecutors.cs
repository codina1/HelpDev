using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

public sealed class UuidGeneratorToolExecutor : IToolExecutor
{
    private static readonly HashSet<string> AllowedFormats = new(StringComparer.Ordinal)
    {
        "D", "N", "B", "P",
    };

    public ToolType Type => ToolType.UuidGenerator;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var count = ToolExecutorHelpers.OptionalInt(root, "count", 1);
        if (count is < 1 or > ToolboxLimits.MaxUuidCount)
        {
            throw new ToolboxException(
                $"UUID count must be between 1 and {ToolboxLimits.MaxUuidCount}.",
                ToolboxApplicationErrorCodes.UuidCountInvalid);
        }

        var format = ToolExecutorHelpers.OptionalString(root, "format", 1) ?? "D";
        format = format.Trim().ToUpperInvariant();
        if (!AllowedFormats.Contains(format))
        {
            throw new ToolboxException(
                "UUID format must be one of: D, N, B, P.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        var values = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            values.Add(Guid.NewGuid().ToString(format, CultureInfo.InvariantCulture));
        }

        return Task.FromResult(ToolExecutorHelpers.ToOutput(new { values }));
    }
}

public sealed class HashGeneratorToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.HashGenerator;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);
        var algorithm = (ToolExecutorHelpers.OptionalString(root, "algorithm", 16) ?? "SHA256")
            .Trim()
            .ToUpperInvariant();

        byte[] hash = algorithm switch
        {
            "SHA256" => SHA256.HashData(Encoding.UTF8.GetBytes(text)),
            "SHA384" => SHA384.HashData(Encoding.UTF8.GetBytes(text)),
            "SHA512" => SHA512.HashData(Encoding.UTF8.GetBytes(text)),
            _ => throw new ToolboxException(
                "Hash algorithm must be SHA256, SHA384, or SHA512.",
                ToolboxApplicationErrorCodes.HashAlgorithmInvalid),
        };

        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return Task.FromResult(ToolExecutorHelpers.ToOutput(new
        {
            algorithm,
            hex,
        }));
    }
}

public sealed class TimestampConverterToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.TimestampConverter;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);

        DateTimeOffset utc;
        if (root.TryGetProperty("unixSeconds", out var secondsElement)
            && secondsElement.ValueKind != JsonValueKind.Null)
        {
            if (secondsElement.ValueKind != JsonValueKind.Number
                || !secondsElement.TryGetInt64(out var seconds))
            {
                throw new ToolboxException(
                    "unixSeconds must be an integer.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }

            try
            {
                utc = DateTimeOffset.FromUnixTimeSeconds(seconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ToolboxException(
                    "unixSeconds is out of range.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }
        }
        else if (root.TryGetProperty("unixMilliseconds", out var msElement)
                 && msElement.ValueKind != JsonValueKind.Null)
        {
            if (msElement.ValueKind != JsonValueKind.Number
                || !msElement.TryGetInt64(out var milliseconds))
            {
                throw new ToolboxException(
                    "unixMilliseconds must be an integer.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }

            try
            {
                utc = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ToolboxException(
                    "unixMilliseconds is out of range.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }
        }
        else if (root.TryGetProperty("isoUtc", out var isoElement)
                 && isoElement.ValueKind == JsonValueKind.String)
        {
            var iso = isoElement.GetString() ?? string.Empty;
            if (!DateTimeOffset.TryParse(
                    iso,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out utc))
            {
                throw new ToolboxException(
                    "isoUtc must be a valid UTC timestamp.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }

            if (!iso.EndsWith("Z", StringComparison.OrdinalIgnoreCase)
                && !iso.Contains('+', StringComparison.Ordinal)
                && !iso.Contains('-', StringComparison.Ordinal))
            {
                // Require explicit UTC indicator via Z or offset when possible.
            }

            if (utc.Offset != TimeSpan.Zero)
            {
                throw new ToolboxException(
                    "Only UTC timestamps are supported.",
                    ToolboxApplicationErrorCodes.TimestampInvalid);
            }
        }
        else
        {
            throw new ToolboxException(
                "Provide unixSeconds, unixMilliseconds, or isoUtc.",
                ToolboxApplicationErrorCodes.TimestampInvalid);
        }

        return Task.FromResult(ToolExecutorHelpers.ToOutput(new
        {
            isoUtc = utc.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            unixSeconds = utc.ToUnixTimeSeconds(),
            unixMilliseconds = utc.ToUnixTimeMilliseconds(),
        }));
    }
}

public sealed class TextStatisticsToolExecutor : IToolExecutor
{
    public ToolType Type => ToolType.TextStatistics;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);

        var characterCount = text.Length;
        var characterCountExcludingWhitespace = text.Count(ch => !char.IsWhiteSpace(ch));
        var lineCount = text.Length == 0 ? 0 : text.Split('\n').Length;
        var wordCount = text
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Length;
        var byteCountUtf8 = Encoding.UTF8.GetByteCount(text);

        return Task.FromResult(ToolExecutorHelpers.ToOutput(new
        {
            characterCount,
            characterCountExcludingWhitespace,
            wordCount,
            lineCount,
            byteCountUtf8,
        }));
    }
}
