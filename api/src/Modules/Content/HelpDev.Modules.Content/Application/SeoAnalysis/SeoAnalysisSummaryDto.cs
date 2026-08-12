namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>Honest count summary — not an SEO score or percentage.</summary>
public sealed record SeoAnalysisSummaryDto(
    int PassedCount,
    int WarningCount,
    int ErrorCount,
    int InformationalCount);
