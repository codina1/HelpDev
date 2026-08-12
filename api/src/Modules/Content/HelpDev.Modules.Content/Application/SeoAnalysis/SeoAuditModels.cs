namespace HelpDev.Modules.Content.Application.SeoAnalysis;

/// <summary>Deterministic SEO audit output. Analytical only — not a domain aggregate.</summary>
public sealed record SeoAuditReport(
    Guid ContentId,
    DateTime GeneratedAtUtc,
    SeoAuditSummary Summary,
    IReadOnlyList<SeoAuditFinding> Findings);

public sealed record SeoAuditSummary(
    int ErrorCount,
    int WarningCount,
    int InfoCount);

/// <summary>Single rule outcome for the SEO platform (no score, rank, or percentage).</summary>
public sealed record SeoAuditFinding(
    string RuleId,
    SeoPlatformCategory Category,
    SeoFindingSeverity Severity,
    string Message,
    string? Suggestion,
    string? Field);

public sealed record SeoAuditReportDto(
    Guid ContentId,
    DateTime GeneratedAtUtc,
    SeoAuditSummaryDto Summary,
    IReadOnlyList<SeoAuditFindingDto> Findings);

public sealed record SeoAuditSummaryDto(
    int ErrorCount,
    int WarningCount,
    int InfoCount);

public sealed record SeoAuditFindingDto(
    string RuleId,
    SeoPlatformCategory Category,
    SeoFindingSeverity Severity,
    string Message,
    string? Suggestion,
    string? Field);
