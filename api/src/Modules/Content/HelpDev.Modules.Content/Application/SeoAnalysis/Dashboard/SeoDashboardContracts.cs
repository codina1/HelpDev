using HelpDev.Modules.Content.Application.Contents;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Dashboard;

public interface ISeoDashboardQueries
{
    Task<SeoDashboardDto> GetAsync(ContentManagementActor actor, CancellationToken cancellationToken = default);
}

public sealed record SeoDashboardDto(
    int TotalContent,
    int PublishedContent,
    int MissingSeoTitleCount,
    int MissingSeoDescriptionCount,
    int MissingCoverImageCount,
    int MissingCanonicalCount,
    DateTime? LastAnalysisTime,
    IReadOnlyList<SeoDashboardCriticalFindingDto> CriticalFindings,
    IReadOnlyList<SeoDashboardRecentContentDto> RecentContent);

public sealed record SeoDashboardCriticalFindingDto(
    Guid ContentId,
    string Title,
    string IssueCode,
    string Message);

public sealed record SeoDashboardRecentContentDto(
    Guid ContentId,
    string Title,
    string Status,
    DateTime UpdatedAtUtc,
    bool MissingSeoTitle,
    bool MissingSeoDescription);
