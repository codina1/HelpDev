namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Editorial recommendation constants for the SEO analyzer.
/// These are NOT Google ranking guarantees — they are HelpDev editorial guidance.
/// Hard domain limits (e.g. SeoTitle ≤ 70) remain in the Domain VO.
/// </summary>
public static class SeoAnalysisOptions
{
    // Domain hard maxima (mirrored from SeoMetadata / Content for rule awareness).
    public const int MaxSeoTitleLength = 70;
    public const int MaxSeoDescriptionLength = 160;
    public const int MaxExcerptLength = 500;
    public const int MaxFocusKeywordLength = 100;

    // Recommended editorial ranges (warnings, not validity failures).
    public const int RecommendedSeoTitleMin = 30;
    public const int RecommendedSeoTitleNearMax = 60;
    public const int RecommendedSeoDescriptionMin = 70;
    public const int RecommendedSeoDescriptionNearMax = 150;

    public const int RecommendedSlugMin = 3;
    public const int RecommendedSlugMax = 60;

    public const int RecommendedFirstParagraphMaxChars = 300;
    public const int SufficientlyLongBodyWords = 300;
    public const int ShortBodyWords = 100;

    public const int MaxEvidenceSnippetLength = 160;

    public const int WordsPerMinuteEstimate = 200;
}
