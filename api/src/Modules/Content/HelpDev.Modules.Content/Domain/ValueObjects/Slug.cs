using System.Text.RegularExpressions;
using HelpDev.SharedKernel.Common;

namespace HelpDev.Modules.Content.Domain.ValueObjects;

/// <summary>
/// Normalized URL slug. Persisted as the existing slug string column.
/// </summary>
public sealed class Slug : ValueObject
{
    private static readonly Regex Pattern = CreatePattern();

    private Slug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Slug FromPersisted(string value) =>
        new Slug(value ?? string.Empty);

    public static bool TryCreate(string? input, out Slug? slug)
    {
        slug = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();

        if (normalized.Length is < 2 or > 300)
        {
            return false;
        }

        if (!Pattern.IsMatch(normalized))
        {
            return false;
        }

        slug = new Slug(normalized);
        return true;
    }

    public static Slug Create(string input)
    {
        if (!TryCreate(input, out var slug) || slug is null)
        {
            throw new ArgumentException("Invalid slug.", nameof(input));
        }

        return slug;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static Regex CreatePattern() =>
        new Regex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
}