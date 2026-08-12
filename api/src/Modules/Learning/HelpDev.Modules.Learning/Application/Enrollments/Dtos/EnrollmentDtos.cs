namespace HelpDev.Modules.Learning.Application.Enrollments.Dtos;

public sealed record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    Guid UserId,
    DateTime EnrolledAt,
    string Status,
    int ProgressPercentage,
    IReadOnlyList<LessonProgressDto> LessonProgress);

public sealed record LessonProgressDto(
    Guid LessonId,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    bool IsCompleted);

public sealed record EnrollmentListItemDto(
    Guid Id,
    Guid CourseId,
    Guid UserId,
    DateTime EnrolledAt,
    string Status,
    int ProgressPercentage);

public sealed class EnrollStudentRequest
{
    public Guid CourseId { get; set; }

    public Guid UserId { get; set; }
}

public sealed class StartLessonRequest
{
    public Guid CourseId { get; set; }

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }
}

public sealed class CompleteLessonRequest
{
    public Guid CourseId { get; set; }

    public Guid UserId { get; set; }

    public Guid LessonId { get; set; }
}
