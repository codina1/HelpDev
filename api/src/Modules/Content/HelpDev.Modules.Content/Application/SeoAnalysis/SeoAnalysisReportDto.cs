namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>
/// Ephemeral, side-effect-free SEO analysis report. Not persisted. Not an SEO score.
/// </summary>
public sealed record SeoAnalysisReportDto(
    DateTime AnalyzedAtUtc,
    SeoAnalysisSummaryDto Summary,
    IReadOnlyList<SeoAnalysisFindingDto> Findings,
    SeoAnalysisStatisticsDto Statistics);
