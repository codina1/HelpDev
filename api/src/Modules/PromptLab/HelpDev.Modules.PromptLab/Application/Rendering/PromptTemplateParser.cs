using System.Text.RegularExpressions;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Application.Rendering;

public interface IPromptTemplateParser
{
    /// <summary>
    /// Extracts unique placeholder names in first-occurrence order.
    /// Rejects malformed syntax. Surrounding whitespace inside braces is rejected.
    /// </summary>
    IReadOnlyList<string> ExtractPlaceholders(string template);
}

public sealed class PromptTemplateParser : IPromptTemplateParser
{
    // Exact {{name}} with no inner whitespace. Identifier: [A-Za-z][A-Za-z0-9_]*
    private static readonly Regex PlaceholderPattern = new(
        @"\{\{([A-Za-z][A-Za-z0-9_]*)\}\}",
        RegexOptions.CultureInvariant);

    private static readonly Regex AnyBracesPattern = new(
        @"\{\{|\}|\{\{|\{|\}",
        RegexOptions.CultureInvariant);

    public IReadOnlyList<string> ExtractPlaceholders(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new DomainException("Template is required.", PromptLabErrorCodes.TemplateRequired);
        }

        if (template.Length > PromptLabLimits.MaxTemplateLength)
        {
            throw new DomainException(
                "Template exceeds the maximum allowed length.",
                PromptLabErrorCodes.TemplateTooLong);
        }

        // Reject nested/malformed by ensuring every {{...}} matches and no leftover braces.
        ValidateBraceBalance(template);

        var ordered = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lastIndex = 0;

        foreach (Match match in PlaceholderPattern.Matches(template))
        {
            // Ensure no stray braces between lastIndex and match
            var gap = template[lastIndex..match.Index];
            if (gap.Contains('{', StringComparison.Ordinal) || gap.Contains('}', StringComparison.Ordinal))
            {
                throw new DomainException(
                    "Template contains malformed placeholder syntax.",
                    PromptLabErrorCodes.TemplateSyntaxInvalid);
            }

            var name = match.Groups[1].Value;
            if (seen.Add(name))
            {
                ordered.Add(name);
            }

            lastIndex = match.Index + match.Length;
        }

        var trailing = template[lastIndex..];
        if (trailing.Contains('{', StringComparison.Ordinal) || trailing.Contains('}', StringComparison.Ordinal))
        {
            throw new DomainException(
                "Template contains malformed placeholder syntax.",
                PromptLabErrorCodes.TemplateSyntaxInvalid);
        }

        if (ordered.Count > PromptLabLimits.MaxVariablesPerVersion)
        {
            throw new DomainException(
                "Template declares too many placeholders.",
                PromptLabErrorCodes.TemplateTooManyVariables);
        }

        return ordered;
    }

    private static void ValidateBraceBalance(string template)
    {
        // Reject empty {{}} and whitespace inside braces {{ name }}
        if (template.Contains("{{}}", StringComparison.Ordinal)
            || Regex.IsMatch(template, @"\{\{\s+\}\}|\{\{\s+[A-Za-z]|[A-Za-z0-9_]\s+\}\}", RegexOptions.CultureInvariant))
        {
            throw new DomainException(
                "Template placeholder is empty or contains whitespace.",
                PromptLabErrorCodes.TemplatePlaceholderInvalid);
        }

        // Reject nested {{a{{b}}}} style by scanning
        var depth = 0;
        for (var i = 0; i < template.Length; i++)
        {
            if (i + 1 < template.Length && template[i] == '{' && template[i + 1] == '{')
            {
                depth++;
                if (depth > 1)
                {
                    throw new DomainException(
                        "Nested placeholders are not allowed.",
                        PromptLabErrorCodes.TemplatePlaceholderInvalid);
                }

                i++;
                continue;
            }

            if (i + 1 < template.Length && template[i] == '}' && template[i + 1] == '}')
            {
                depth--;
                if (depth < 0)
                {
                    throw new DomainException(
                        "Template contains malformed placeholder syntax.",
                        PromptLabErrorCodes.TemplateSyntaxInvalid);
                }

                i++;
            }
        }

        if (depth != 0)
        {
            throw new DomainException(
                "Template contains malformed placeholder syntax.",
                PromptLabErrorCodes.TemplateSyntaxInvalid);
        }
    }
}
