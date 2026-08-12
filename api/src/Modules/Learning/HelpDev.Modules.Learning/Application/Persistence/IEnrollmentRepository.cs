using HelpDev.Modules.Learning.Domain.Enrollments;

namespace HelpDev.Modules.Learning.Application.Persistence;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Enrollment?> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
