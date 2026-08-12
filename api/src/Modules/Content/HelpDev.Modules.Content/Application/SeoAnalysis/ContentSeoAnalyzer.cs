using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;
using HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Deterministic, side-effect-free SEO analyzer. No EF, HTTP, AI, or network.
/// Rules are explicitly ordered — no reflection discovery.
/// </summary>
public sealed class ContentSeoAnalyzer : IContentSeoAnalyzer
{
    private readonly IReadOnlyList<ISeoAnalysisRule> _rules;

    public ContentSeoAnalyzer()
        : this(CreateDefaultRules())
    {
    }

    public ContentSeoAnalyzer(IReadOnlyList<ISeoAnalysisRule> rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    public static IReadOnlyList<ISeoAnalysisRule> CreateDefaultRules() =>
    [
        new SeoTitleExistsRule(),
        new SeoTitleLengthRule(),
        new SeoTitleKeywordRule(),
        new SeoDescriptionExistsRule(),
        new SeoDescriptionLengthRule(),
        new SeoDescriptionKeywordRule(),
        new FocusKeywordPresenceRule(),
        new FocusKeywordCoverageRule(),
        new SlugQualityRule(),
        new HeadingStructureRule(),
        new ContentLengthRule(),
        new FirstParagraphRule(),
        new LinkSafetyRule(),
        new InternalLinksPresenceRule(),
        new MediaPresenceRule(),
        new ImageAltInBodyRule(),
        new CanonicalMissingRule(),
        new CanonicalUrlRule(),
        new SeoMetadataValidityRule(),
        new CodeBlockRule(),
    ];

    public SeoAnalysisReportDto Analyze(SeoAnalysisInput input, DateTime analyzedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(input);

        var facts = MarkdownDocumentScanner.Scan(input.Body);
        var context = new SeoAnalysisContext(input, facts);

        var findings = new List<SeoAnalysisFindingDto>();
        foreach (var rule in _rules)
        {
            var ruleFindings = rule.Analyze(context);
            if (ruleFindings is { Count: > 0 })
            {
                findings.AddRange(ruleFindings);
            }
        }

        findings.Sort(static (a, b) =>
        {
            var category = a.Category.CompareTo(b.Category);
            return category != 0
                ? category
                : string.CompareOrdinal(a.RuleId, b.RuleId);
        });

        var summary = new SeoAnalysisSummaryDto(
            PassedCount: findings.Count(f => f.Passed),
            WarningCount: findings.Count(f => !f.Passed && f.Severity == SeoFindingSeverity.Warning),
            ErrorCount: findings.Count(f => !f.Passed && f.Severity == SeoFindingSeverity.Error),
            InformationalCount: findings.Count(f => f.Severity == SeoFindingSeverity.Info));

        var readingMinutes = facts.WordCount <= 0
            ? 0
            : Math.Max(1, (int)Math.Ceiling(facts.WordCount / (double)SeoAnalysisOptions.WordsPerMinuteEstimate));

        var statistics = new SeoAnalysisStatisticsDto(
            WordCount: facts.WordCount,
            CharacterCount: facts.CharacterCount,
            ParagraphCount: facts.Paragraphs.Count,
            HeadingCount: facts.Headings.Count,
            CodeBlockCount: facts.CodeBlocks.Count,
            LanguageLabelledCodeBlockCount: facts.LanguageLabelledCodeBlockCount,
            UnlabelledCodeBlockCount: facts.UnlabelledCodeBlockCount,
            LinkCount: facts.Links.Count,
            InternalLinkCount: CountInternal(facts),
            ExternalLinkCount: CountExternal(facts),
            EstimatedReadingMinutes: readingMinutes);

        return new SeoAnalysisReportDto(analyzedAtUtc, summary, findings, statistics);
    }

    private static int CountInternal(MarkdownDocumentFacts facts) =>
        facts.Links.Count(l =>
        {
            var href = l.Href.Trim();
            return href.StartsWith('/')
                || href.StartsWith('#')
                || href.StartsWith("./", StringComparison.Ordinal)
                || href.StartsWith("../", StringComparison.Ordinal)
                || !Uri.TryCreate(href, UriKind.Absolute, out _);
        });

    private static int CountExternal(MarkdownDocumentFacts facts) =>
        facts.Links.Count(l =>
            Uri.TryCreate(l.Href.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
