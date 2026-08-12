namespace HelpDev.Modules.Learning.Application.Courses.Dtos;

/// <summary>
/// Lean published-course projection for Search indexing/backfill (no sections/lessons).
/// </summary>
public sealed record CourseSearchSourceDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    DateTime CreatedAt,
    DateTime? PublishedAt);
