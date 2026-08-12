namespace HelpDev.Modules.Search.Domain;

public static class SearchSourceTypes
{
    public const string Content = "content";

    public const string Course = "course";

    public const string Lesson = "lesson";

    public const string Tool = "tool";

    public const string Prompt = "prompt";

    public static bool IsKnown(string? value) =>
        value is Content or Course or Lesson or Tool or Prompt;

    public static string NormalizeOrThrow(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        if (!IsKnown(normalized))
        {
            throw new ArgumentException($"Unsupported search source type '{value}'.", nameof(value));
        }

        return normalized!;
    }
}
