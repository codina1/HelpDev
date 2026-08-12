using HelpDev.Modules.Learning.Application.Courses.Dtos;

namespace HelpDev.Modules.Learning.Application.Courses;

public interface IPublicCourseQueries
{
    Task<IReadOnlyList<CourseListItemDto>> ListPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto?> GetPublishedByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Keyset page of published courses ordered by Id ascending for Search backfill.
    /// </summary>
    Task<IReadOnlyList<CourseSearchSourceDto>> ListPublishedSearchBatchAsync(
        Guid? afterCourseId,
        int take,
        CancellationToken cancellationToken = default);
}
