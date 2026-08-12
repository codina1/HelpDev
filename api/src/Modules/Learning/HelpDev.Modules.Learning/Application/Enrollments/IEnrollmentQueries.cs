using HelpDev.Modules.Learning.Application.Enrollments.Dtos;

namespace HelpDev.Modules.Learning.Application.Enrollments;

public interface IEnrollmentQueries
{
    Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
