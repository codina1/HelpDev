using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Modules.Learning.Application.Persistence;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        CourseSlug slug,
        Guid? excludingCourseId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Course course, CancellationToken cancellationToken = default);
}
