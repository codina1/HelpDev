using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Normalized analysis context shared by all rules. Built once per Analyze call.
/// </summary>
public sealed class SeoAnalysisContext
{
    public SeoAnalysisContext(SeoAnalysisInput input, MarkdownDocumentFacts facts)
    {
        Input = input;
        Facts = facts;
        EffectiveTitle = FirstNonBlank(input.SeoTitle, input.Title) ?? string.Empty;
        EffectiveDescription = FirstNonBlank(input.SeoDescription, input.Excerpt) ?? string.Empty;
        FocusKeyword = NormalizeOptional(input.FocusKeyword);
        Slug = (input.Slug ?? string.Empty).Trim();
        CoverImage = NormalizeOptional(input.CoverImage);
        OgImage = NormalizeOptional(input.OgImage);
        CanonicalUrl = NormalizeOptional(input.CanonicalUrl);
        HasFocusKeyword = FocusKeyword is not null;
    }

    public SeoAnalysisInput Input { get; }

    public MarkdownDocumentFacts Facts { get; }

    public string EffectiveTitle { get; }

    public string EffectiveDescription { get; }

    public string? FocusKeyword { get; }

    public bool HasFocusKeyword { get; }

    public string Slug { get; }

    public string? CoverImage { get; }

    public string? OgImage { get; }

    public string? CanonicalUrl { get; }

    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string? FirstNonBlank(params string?[] values)
    {
        foreach (var value in values)
        {
            var normalized = NormalizeOptional(value);
            if (normalized is not null)
            {
                return normalized;
            }
        }

        return null;
    }

    public static string? CapEvidence(string? evidence)
    {
        if (evidence is null)
        {
            return null;
        }

        var trimmed = evidence.Trim();
        if (trimmed.Length <= SeoAnalysisOptions.MaxEvidenceSnippetLength)
        {
            return trimmed;
        }

        return trimmed[..SeoAnalysisOptions.MaxEvidenceSnippetLength] + "…";
    }
}
