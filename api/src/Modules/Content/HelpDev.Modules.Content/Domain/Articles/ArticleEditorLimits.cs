namespace HelpDev.Modules.Content.Domain.Articles;

public static class ArticleEditorLimits
{
    public const string MarkdownFormat = "markdown";
    public const string BlocksFormat = "blocks";
    public const string CurrentEditorVersion = "1";
    public const int MaxContentJsonLength = 1_500_000;
    public const int MaxContentHtmlLength = 1_500_000;
    public const int MaxContentFormatLength = 32;
    public const int MaxEditorVersionLength = 20;
    public const int MaxReadingTimeMinutes = 600;
}

public sealed record ArticleEditorPayload(
    string? ContentJson,
    string? ContentFormat,
    string? EditorVersion);

public sealed record ArticleEditorDocument(
    string? ContentJson,
    string? ContentHtml,
    string? ContentFormat,
    string? EditorVersion,
    int? WordCount,
    int? ReadingTimeMinutes);
