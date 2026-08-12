namespace HelpDev.Modules.Search.Domain;

/// <summary>
/// Unified HelpDev knowledge source kinds for semantic indexing.
/// Wire values are lowercase and shared with <see cref="SearchSourceTypes"/>.
/// </summary>
public static class KnowledgeSourceType
{
    public const string Content = SearchSourceTypes.Content;
    public const string Course = SearchSourceTypes.Course;
    public const string Lesson = SearchSourceTypes.Lesson;
    public const string Tool = SearchSourceTypes.Tool;
    public const string Prompt = SearchSourceTypes.Prompt;

    public static IReadOnlyList<string> All { get; } =
    [
        Content,
        Course,
        Lesson,
        Tool,
        Prompt,
    ];
}
