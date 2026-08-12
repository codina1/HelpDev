namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>Maps the rule-engine report into the SEO platform audit contract.</summary>
public static class SeoAuditMapper
{
    public static SeoAuditReport ToAuditReport(Guid contentId, SeoAnalysisReportDto report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var findings = report.Findings
            .Select(MapFinding)
            .ToList();

        var summary = new SeoAuditSummary(
            ErrorCount: findings.Count(f => f.Severity == SeoFindingSeverity.Error),
            WarningCount: findings.Count(f => f.Severity == SeoFindingSeverity.Warning),
            InfoCount: findings.Count(f => f.Severity == SeoFindingSeverity.Info));

        return new SeoAuditReport(contentId, report.AnalyzedAtUtc, summary, findings);
    }

    public static SeoAuditReportDto ToDto(SeoAuditReport report) =>
        new(
            report.ContentId,
            report.GeneratedAtUtc,
            new SeoAuditSummaryDto(
                report.Summary.ErrorCount,
                report.Summary.WarningCount,
                report.Summary.InfoCount),
            report.Findings
                .Select(f => new SeoAuditFindingDto(
                    f.RuleId,
                    f.Category,
                    f.Severity,
                    f.Message,
                    f.Suggestion,
                    f.Field))
                .ToList());

    public static SeoAuditReportDto ToDto(Guid contentId, SeoAnalysisReportDto report) =>
        ToDto(ToAuditReport(contentId, report));

    private static SeoAuditFinding MapFinding(SeoAnalysisFindingDto finding)
    {
        var category = MapCategory(finding.Category);
        var field = MapField(finding);
        var message = string.IsNullOrWhiteSpace(finding.Title)
            ? finding.Message
            : $"{finding.Title}: {finding.Message}";

        if (!finding.Passed && finding.Severity is SeoFindingSeverity.Warning or SeoFindingSeverity.Error)
        {
            return new SeoAuditFinding(
                finding.RuleId,
                category,
                finding.Severity,
                message,
                finding.Recommendation,
                field);
        }

        return new SeoAuditFinding(
            finding.RuleId,
            category,
            finding.Severity,
            message,
            finding.Recommendation,
            field);
    }

    public static SeoPlatformCategory MapCategory(SeoFindingCategory category) =>
        category switch
        {
            SeoFindingCategory.Metadata or SeoFindingCategory.Title or SeoFindingCategory.Description
                or SeoFindingCategory.Keyword => SeoPlatformCategory.Metadata,
            SeoFindingCategory.Structure or SeoFindingCategory.Content => SeoPlatformCategory.ContentStructure,
            SeoFindingCategory.Media => SeoPlatformCategory.Images,
            SeoFindingCategory.Links => SeoPlatformCategory.Links,
            SeoFindingCategory.Url => SeoPlatformCategory.Technical,
            _ => SeoPlatformCategory.Metadata,
        };

    private static string? MapField(SeoAnalysisFindingDto finding) =>
        finding.Category switch
        {
            SeoFindingCategory.Title => "seoTitle",
            SeoFindingCategory.Description => "seoDescription",
            SeoFindingCategory.Keyword => "focusKeyword",
            SeoFindingCategory.Url => "canonicalUrl",
            SeoFindingCategory.Media when finding.RuleId.Contains("cover", StringComparison.Ordinal) => "coverImage",
            SeoFindingCategory.Media when finding.RuleId.Contains("og", StringComparison.Ordinal) => "ogImage",
            SeoFindingCategory.Structure => "body",
            _ => null,
        };
}
