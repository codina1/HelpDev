using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Modules.Learning.Application.Courses;

public interface ICourseService
{
    Task<CourseDetailDto> CreateAsync(
        CourseManagementActor actor,
        CreateCourseRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> GetByIdAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseManagementActor actor,
        CourseStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> UpdateDetailsAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateCourseRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> AddSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> RenameSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        RenameSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> ReorderSectionAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> AddLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        AddLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> UpdateLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        UpdateLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> ReorderLessonAsync(
        CourseManagementActor actor,
        Guid courseId,
        ReorderLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<CourseDetailDto> PublishAsync(
        CourseManagementActor actor,
        Guid courseId,
        CancellationToken cancellationToken = default);
}
