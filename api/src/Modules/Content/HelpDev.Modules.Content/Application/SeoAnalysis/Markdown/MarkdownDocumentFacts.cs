namespace HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

public sealed record MarkdownHeading(int Level, string Text, int LineIndex);

public sealed record MarkdownParagraph(string Text, int LineIndex);

public sealed record MarkdownCodeBlock(string Text, string? Language, int LineIndex);

public sealed record MarkdownLink(string Label, string Href, int LineIndex);

public sealed record MarkdownImage(string AltText, string Src, int LineIndex);

/// <summary>
/// Facts extracted from a single bounded Markdown scan. Rules reuse this instead of
/// re-parsing the body. Pure / side-effect free.
/// </summary>
public sealed record MarkdownDocumentFacts(
    int CharacterCount,
    int WordCount,
    IReadOnlyList<MarkdownHeading> Headings,
    IReadOnlyList<MarkdownParagraph> Paragraphs,
    IReadOnlyList<MarkdownCodeBlock> CodeBlocks,
    IReadOnlyList<MarkdownLink> Links,
    IReadOnlyList<MarkdownImage> Images)
{
    public MarkdownParagraph? FirstParagraph =>
        Paragraphs.Count > 0 ? Paragraphs[0] : null;

    public int LanguageLabelledCodeBlockCount =>
        CodeBlocks.Count(b => !string.IsNullOrWhiteSpace(b.Language));

    public int UnlabelledCodeBlockCount =>
        CodeBlocks.Count(b => string.IsNullOrWhiteSpace(b.Language));
}
