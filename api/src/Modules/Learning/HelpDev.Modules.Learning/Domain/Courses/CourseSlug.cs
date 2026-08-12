using System.Text.RegularExpressions;
using HelpDev.SharedKernel.Common;

namespace HelpDev.Modules.Learning.Domain.Courses;

/// <summary>
/// Normalized course URL slug (same normalization rules as Content slug, kept local to Learning).
/// </summary>
public sealed class CourseSlug : ValueObject
{
    private static readonly Regex Pattern = CreatePattern();

    private CourseSlug(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static bool TryCreate(string? input, out CourseSlug? slug)
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

        slug = new CourseSlug(normalized);
        return true;
    }

    public static CourseSlug Create(string input)
    {
        if (!TryCreate(input, out var slug) || slug is null)
        {
            throw new ArgumentException("Invalid course slug.", nameof(input));
        }

        return slug;
    }

    /// <summary>
    /// Reconstitutes a slug from persistence without re-validation.
    /// </summary>
    public static CourseSlug FromPersisted(string value) =>
        new CourseSlug(value ?? string.Empty);

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    private static Regex CreatePattern() =>
        new Regex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
