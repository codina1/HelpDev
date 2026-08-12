namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>A single deterministic SEO finding. RuleId is stable and language-neutral.</summary>
public sealed record SeoAnalysisFindingDto(
    string RuleId,
    SeoFindingCategory Category,
    SeoFindingSeverity Severity,
    bool Passed,
    string Title,
    string Message,
    string? Evidence,
    string? Recommendation);
