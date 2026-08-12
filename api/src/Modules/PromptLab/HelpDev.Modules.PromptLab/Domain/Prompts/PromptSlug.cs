using System.Text.RegularExpressions;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed class PromptSlug : ValueObject
{
    private static readonly Regex Pattern = new(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant);

    private PromptSlug(string value) => Value = value;

    public string Value { get; }

    public static PromptSlug FromPersisted(string value) => new(value ?? string.Empty);

    public static PromptSlug Create(string? input, int maxLength, string requiredCode, string invalidCode)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new DomainException("Slug is required.", requiredCode);
        }

        var normalized = input.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 || normalized.Length > maxLength || !Pattern.IsMatch(normalized))
        {
            throw new DomainException("Slug is invalid.", invalidCode);
        }

        return new PromptSlug(normalized);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
