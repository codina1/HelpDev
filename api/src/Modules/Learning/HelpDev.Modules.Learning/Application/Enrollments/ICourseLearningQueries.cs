using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Modules.Learning.Application.Enrollments;

public sealed record CourseLearningStructure(
    Guid CourseId,
    CourseStatus Status,
    IReadOnlyList<Guid> LessonIds);

public interface ICourseLearningQueries
{
    Task<CourseLearningStructure?> GetStructureAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);
}
