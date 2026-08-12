using System.Text.Json;
using System.Text.RegularExpressions;
using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Infrastructure.Execution;

public sealed class RegexTesterToolExecutor : IToolExecutor
{
    private static readonly HashSet<string> AllowedOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "IgnoreCase",
        "Multiline",
        "Singleline",
        "CultureInvariant",
    };

    public ToolType Type => ToolType.RegexTester;

    public Task<ToolExecutionOutput> ExecuteAsync(
        ToolExecutionInput input,
        CancellationToken cancellationToken = default)
    {
        ToolExecutorHelpers.ThrowIfCancellationRequested(cancellationToken);
        var root = ToolExecutorHelpers.RequireObject(input);
        var pattern = ToolExecutorHelpers.RequireString(root, "pattern", ToolboxLimits.MaxRegexPatternLength);
        var text = ToolExecutorHelpers.RequireString(root, "text", ToolboxLimits.MaxRegexTextLength);
        ToolExecutorHelpers.EnsureUtf8ByteLimit(text);

        var timeoutMs = ToolExecutorHelpers.OptionalInt(root, "timeoutMs", ToolboxLimits.DefaultRegexTimeoutMs);
        if (timeoutMs is < ToolboxLimits.MinRegexTimeoutMs or > ToolboxLimits.MaxRegexTimeoutMs)
        {
            throw new ToolboxException(
                $"timeoutMs must be between {ToolboxLimits.MinRegexTimeoutMs} and {ToolboxLimits.MaxRegexTimeoutMs}.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        var options = RegexOptions.None;
        if (root.TryGetProperty("options", out var optionsElement)
            && optionsElement.ValueKind != JsonValueKind.Null)
        {
            if (optionsElement.ValueKind != JsonValueKind.Array)
            {
                throw new ToolboxException(
                    "options must be an array of strings.",
                    ToolboxApplicationErrorCodes.RegexOptionsInvalid);
            }

            foreach (var item in optionsElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    throw new ToolboxException(
                        "options must contain only strings.",
                        ToolboxApplicationErrorCodes.RegexOptionsInvalid);
                }

                var name = item.GetString() ?? string.Empty;
                if (!AllowedOptions.Contains(name))
                {
                    throw new ToolboxException(
                        "Regex option is not allow-listed.",
                        ToolboxApplicationErrorCodes.RegexOptionsInvalid);
                }

                options |= name.ToLowerInvariant() switch
                {
                    "ignorecase" => RegexOptions.IgnoreCase,
                    "multiline" => RegexOptions.Multiline,
                    "singleline" => RegexOptions.Singleline,
                    "cultureinvariant" => RegexOptions.CultureInvariant,
                    _ => RegexOptions.None,
                };
            }
        }

        // Never use RegexOptions.Compiled for user-supplied patterns.
        if ((options & RegexOptions.Compiled) != 0)
        {
            throw new ToolboxException(
                "Compiled regex is not allowed.",
                ToolboxApplicationErrorCodes.RegexOptionsInvalid);
        }

        Regex regex;
        try
        {
            regex = new Regex(pattern, options, TimeSpan.FromMilliseconds(timeoutMs));
        }
        catch (ArgumentException)
        {
            throw new ToolboxException(
                "Regex pattern is invalid.",
                ToolboxApplicationErrorCodes.RegexPatternInvalid);
        }

        try
        {
            var matches = regex.Matches(text);
            var results = new List<object>();
            var count = Math.Min(matches.Count, ToolboxLimits.MaxRegexMatches);
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var match = matches[i];
                var groups = new List<object>();
                for (var g = 0; g < match.Groups.Count; g++)
                {
                    var group = match.Groups[g];
                    var value = group.Value;
                    if (value.Length > ToolboxLimits.MaxCaptureValueLength)
                    {
                        value = value[..ToolboxLimits.MaxCaptureValueLength];
                    }

                    groups.Add(new
                    {
                        name = group.Name,
                        value,
                        index = group.Index,
                        length = group.Length,
                    });
                }

                results.Add(new
                {
                    value = match.Value.Length > ToolboxLimits.MaxCaptureValueLength
                        ? match.Value[..ToolboxLimits.MaxCaptureValueLength]
                        : match.Value,
                    index = match.Index,
                    length = match.Length,
                    groups,
                });
            }

            return Task.FromResult(ToolExecutorHelpers.ToOutput(new
            {
                matchCount = matches.Count,
                returnedMatchCount = results.Count,
                truncated = matches.Count > ToolboxLimits.MaxRegexMatches,
                matches = results,
            }));
        }
        catch (RegexMatchTimeoutException)
        {
            throw new ToolboxException(
                "Regex execution timed out.",
                ToolboxApplicationErrorCodes.RegexTimeout);
        }
    }
}
