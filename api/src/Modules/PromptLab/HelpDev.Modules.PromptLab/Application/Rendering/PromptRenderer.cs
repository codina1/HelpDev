using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Application.Rendering;

public sealed record PromptVersionSnapshot(
    Guid VersionId,
    int VersionNumber,
    string Template,
    IReadOnlyList<PromptVariableSnapshot> Variables);

public sealed record PromptVariableSnapshot(
    string Name,
    PromptVariableType Type,
    bool IsRequired,
    string? DefaultValue,
    int? MinLength,
    int? MaxLength,
    decimal? MinValue,
    decimal? MaxValue,
    string? ValidationPattern,
    IReadOnlyList<string> AllowedValues);

public sealed record PromptRenderOutput(string RenderedText);

public interface IPromptRenderer
{
    PromptRenderOutput Render(
        PromptVersionSnapshot version,
        IReadOnlyDictionary<string, JsonElement> values);
}

/// <summary>
/// Single-pass token renderer. User values are inserted literally and never re-parsed.
/// </summary>
public sealed class PromptRenderer : IPromptRenderer
{
    private static readonly Regex TokenPattern = new(
        @"\{\{([A-Za-z][A-Za-z0-9_]*)\}\}",
        RegexOptions.CultureInvariant);

    public PromptRenderOutput Render(
        PromptVersionSnapshot version,
        IReadOnlyDictionary<string, JsonElement> values)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(values);

        var variablesByName = version.Variables
            .ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var key in values.Keys)
        {
            if (!variablesByName.ContainsKey(key))
            {
                throw new DomainException(
                    $"Unknown variable '{key}'.",
                    PromptLabErrorCodes.RenderUnknownVariable);
            }
        }

        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var variable in version.Variables)
        {
            resolved[variable.Name] = ResolveValue(variable, values);
        }

        var builder = new StringBuilder(version.Template.Length);
        var lastIndex = 0;
        foreach (Match match in TokenPattern.Matches(version.Template))
        {
            builder.Append(version.Template, lastIndex, match.Index - lastIndex);
            var name = match.Groups[1].Value;
            // Insert literal text — never scan inserted value for more placeholders.
            builder.Append(resolved[name]);
            lastIndex = match.Index + match.Length;
        }

        builder.Append(version.Template, lastIndex, version.Template.Length - lastIndex);
        var rendered = builder.ToString();
        if (rendered.Length > PromptLabLimits.MaxRenderedLength)
        {
            throw new DomainException(
                "Rendered output exceeds the maximum allowed length.",
                PromptLabErrorCodes.RenderOutputTooLong);
        }

        return new PromptRenderOutput(rendered);
    }

    private static string ResolveValue(
        PromptVariableSnapshot variable,
        IReadOnlyDictionary<string, JsonElement> values)
    {
        JsonElement? supplied = null;
        if (values.TryGetValue(variable.Name, out var element))
        {
            supplied = element;
        }

        string? raw;
        if (supplied is null || supplied.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            if (variable.DefaultValue is not null)
            {
                raw = variable.DefaultValue;
            }
            else if (variable.IsRequired)
            {
                throw new DomainException(
                    $"Required variable '{variable.Name}' is missing.",
                    PromptLabErrorCodes.RenderRequiredVariableMissing);
            }
            else
            {
                return string.Empty;
            }
        }
        else
        {
            raw = ConvertJsonToString(variable, supplied.Value);
        }

        ValidateResolved(variable, raw);
        return raw;
    }

    private static string ConvertJsonToString(PromptVariableSnapshot variable, JsonElement element)
    {
        return variable.Type switch
        {
            PromptVariableType.Boolean => NormalizeBoolean(element),
            PromptVariableType.Integer => NormalizeInteger(element),
            PromptVariableType.Decimal => NormalizeDecimal(element),
            PromptVariableType.Text
                or PromptVariableType.MultilineText
                or PromptVariableType.Select => element.ValueKind == JsonValueKind.String
                    ? element.GetString() ?? string.Empty
                    : throw new DomainException(
                        $"Variable '{variable.Name}' must be a string.",
                        PromptLabErrorCodes.RenderValueInvalid),
            _ => throw new DomainException(
                "Variable type is invalid.",
                PromptLabErrorCodes.VariableTypeInvalid),
        };
    }

    private static string NormalizeBoolean(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return element.GetBoolean() ? "true" : "false";
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var text = element.GetString() ?? string.Empty;
            if (bool.TryParse(text, out var parsed))
            {
                return parsed ? "true" : "false";
            }

            if (string.Equals(text, "1", StringComparison.Ordinal)
                || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "on", StringComparison.OrdinalIgnoreCase))
            {
                return "true";
            }

            if (string.Equals(text, "0", StringComparison.Ordinal)
                || string.Equals(text, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "off", StringComparison.OrdinalIgnoreCase))
            {
                return "false";
            }
        }

        throw new DomainException("Boolean value is invalid.", PromptLabErrorCodes.RenderValueInvalid);
    }

    private static string NormalizeInteger(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var number))
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        if (element.ValueKind == JsonValueKind.String
            && long.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed.ToString(CultureInfo.InvariantCulture);
        }

        throw new DomainException("Integer value is invalid.", PromptLabErrorCodes.RenderValueInvalid);
    }

    private static string NormalizeDecimal(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var number))
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        if (element.ValueKind == JsonValueKind.String
            && decimal.TryParse(element.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed.ToString(CultureInfo.InvariantCulture);
        }

        throw new DomainException("Decimal value is invalid.", PromptLabErrorCodes.RenderValueInvalid);
    }

    private static void ValidateResolved(PromptVariableSnapshot variable, string value)
    {
        if (value.Length > PromptLabLimits.MaxVariableValueLength)
        {
            throw new DomainException(
                $"Variable '{variable.Name}' exceeds the maximum length.",
                PromptLabErrorCodes.RenderValueTooLong);
        }

        if (variable.Type is PromptVariableType.Text or PromptVariableType.MultilineText or PromptVariableType.Select)
        {
            if (variable.MinLength is not null && value.Length < variable.MinLength)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' is too short.",
                    PromptLabErrorCodes.RenderValueInvalid);
            }

            if (variable.MaxLength is not null && value.Length > variable.MaxLength)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' is too long.",
                    PromptLabErrorCodes.RenderValueTooLong);
            }
        }

        if (variable.Type is PromptVariableType.Integer or PromptVariableType.Decimal)
        {
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var numeric))
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' is invalid.",
                    PromptLabErrorCodes.RenderValueInvalid);
            }

            if (variable.MinValue is not null && numeric < variable.MinValue)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' is below the minimum.",
                    PromptLabErrorCodes.RenderValueInvalid);
            }

            if (variable.MaxValue is not null && numeric > variable.MaxValue)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' is above the maximum.",
                    PromptLabErrorCodes.RenderValueInvalid);
            }
        }

        if (variable.Type == PromptVariableType.Select
            && !variable.AllowedValues.Contains(value, StringComparer.Ordinal))
        {
            throw new DomainException(
                $"Variable '{variable.Name}' is not an allowed option.",
                PromptLabErrorCodes.RenderValueInvalid);
        }

        if (!string.IsNullOrWhiteSpace(variable.ValidationPattern)
            && variable.Type is PromptVariableType.Text or PromptVariableType.MultilineText)
        {
            try
            {
                if (!Regex.IsMatch(
                        value,
                        variable.ValidationPattern,
                        RegexOptions.CultureInvariant,
                        TimeSpan.FromMilliseconds(PromptLabLimits.ValidationRegexTimeoutMs)))
                {
                    throw new DomainException(
                        $"Variable '{variable.Name}' failed pattern validation.",
                        PromptLabErrorCodes.RenderValueInvalid);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' pattern validation timed out.",
                    PromptLabErrorCodes.RenderPatternTimeout);
            }
            catch (ArgumentException)
            {
                throw new DomainException(
                    $"Variable '{variable.Name}' has an invalid validation pattern.",
                    PromptLabErrorCodes.VariablePatternInvalid);
            }
        }
    }
}
