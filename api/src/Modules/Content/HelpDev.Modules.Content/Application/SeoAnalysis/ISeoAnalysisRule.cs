namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Explicit, independently testable SEO analysis rule. No EF/HTTP/user/network.
/// </summary>
public interface ISeoAnalysisRule
{
    /// <summary>Stable, language-neutral rule identifier (e.g. seo.title.missing).</summary>
    string RuleId { get; }

    IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context);
}
