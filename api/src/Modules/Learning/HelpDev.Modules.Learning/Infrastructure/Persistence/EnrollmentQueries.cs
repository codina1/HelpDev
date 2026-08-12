using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class EnrollmentQueries : IEnrollmentQueries
{
    private readonly ILearningDbContext _dbContext;

    public EnrollmentQueries(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.UserId == userId)
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .Select(enrollment => new
            {
                enrollment.Id,
                enrollment.CourseId,
                enrollment.UserId,
                enrollment.EnrolledAt,
                enrollment.Status,
                enrollment.ProgressPercentage,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new EnrollmentListItemDto(
                row.Id,
                row.CourseId,
                row.UserId,
                row.EnrolledAt,
                row.Status.ToString(),
                row.ProgressPercentage.Value))
            .ToList();
    }
}
