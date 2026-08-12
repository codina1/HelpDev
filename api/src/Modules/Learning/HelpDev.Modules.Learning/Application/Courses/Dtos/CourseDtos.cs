namespace HelpDev.Modules.Learning.Application.Courses.Dtos;

public sealed record CourseListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string Status,
    Guid InstructorId,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int SectionCount,
    int LessonCount);

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    Guid InstructorId,
    string Status,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    IReadOnlyList<CourseSectionDto> Sections);

public sealed record CourseSectionDto(
    Guid Id,
    string Title,
    int Order,
    IReadOnlyList<CourseLessonDto> Lessons);

public sealed record CourseLessonDto(
    Guid Id,
    string Title,
    int Order,
    Guid? ContentId,
    string? VideoUrl,
    int? DurationMinutes,
    bool IsPreview);
