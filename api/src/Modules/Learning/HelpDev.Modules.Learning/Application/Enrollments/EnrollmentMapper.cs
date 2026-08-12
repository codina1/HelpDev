using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Enrollments;

namespace HelpDev.Modules.Learning.Application.Enrollments;

public static class EnrollmentMapper
{
    public static EnrollmentDto ToDto(Enrollment enrollment) =>
        new(
            enrollment.Id,
            enrollment.CourseId,
            enrollment.UserId,
            enrollment.EnrolledAt,
            enrollment.Status.ToString(),
            enrollment.ProgressPercentage.Value,
            enrollment.LessonProgressEntries
                .OrderBy(progress => progress.LessonId)
                .Select(ToLessonProgressDto)
                .ToList());

    public static LessonProgressDto ToLessonProgressDto(LessonProgress progress) =>
        new(
            progress.LessonId,
            progress.StartedAt,
            progress.CompletedAt,
            progress.IsCompleted);
}
