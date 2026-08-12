using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class CourseLearningQueries : ICourseLearningQueries
{
    private readonly ILearningDbContext _dbContext;

    public CourseLearningQueries(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CourseLearningStructure?> GetStructureAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var row = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Id == courseId)
            .Select(course => new
            {
                course.Id,
                course.Status,
                LessonIds = course.Sections
                    .OrderBy(section => section.Order)
                    .SelectMany(section => section.Lessons
                        .OrderBy(lesson => lesson.Order)
                        .Select(lesson => lesson.Id))
                    .Distinct()
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new CourseLearningStructure(row.Id, row.Status, row.LessonIds);
    }
}
