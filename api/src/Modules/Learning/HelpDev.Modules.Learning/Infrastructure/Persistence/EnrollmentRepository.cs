using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Enrollments;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ILearningDbContext _dbContext;

    public EnrollmentRepository(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        EnrollmentsWithProgress()
            .FirstOrDefaultAsync(enrollment => enrollment.Id == id, cancellationToken);

    public Task<Enrollment?> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        EnrollmentsWithProgress()
            .FirstOrDefaultAsync(
                enrollment => enrollment.CourseId == courseId && enrollment.UserId == userId,
                cancellationToken);

    public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _dbContext.Enrollments.Add(enrollment);
        return Task.CompletedTask;
    }

    private IQueryable<Enrollment> EnrollmentsWithProgress() =>
        _dbContext.Enrollments
            .Include(enrollment => enrollment.LessonProgressEntries);
}
