using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Modules.Learning.Application.Courses;

public interface ICourseQueries
{
    /// <summary>
    /// Lists courses optionally filtered by status and instructor.
    /// Pass <paramref name="instructorId"/> as null only for actors who can manage all courses.
    /// </summary>
    Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseStatus? status,
        Guid? instructorId,
        CancellationToken cancellationToken = default);
}
