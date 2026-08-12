namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>Factual content statistics derived from a single Markdown scan.</summary>
public sealed record SeoAnalysisStatisticsDto(
    int WordCount,
    int CharacterCount,
    int ParagraphCount,
    int HeadingCount,
    int CodeBlockCount,
    int LanguageLabelledCodeBlockCount,
    int UnlabelledCodeBlockCount,
    int LinkCount,
    int InternalLinkCount,
    int ExternalLinkCount,
    int EstimatedReadingMinutes);
