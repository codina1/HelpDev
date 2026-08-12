using HelpDev.Modules.Learning.Application.Enrollments.Dtos;

namespace HelpDev.Modules.Learning.Application.Enrollments;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(
        EnrollStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<EnrollmentDto> GetByIdAsync(
        Guid enrollmentId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<EnrollmentDto> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<EnrollmentDto> StartLessonAsync(
        StartLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<EnrollmentDto> CompleteLessonAsync(
        CompleteLessonRequest request,
        CancellationToken cancellationToken = default);
}
