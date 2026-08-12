using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class CourseQueries : ICourseQueries
{
    private readonly ILearningDbContext _dbContext;

    public CourseQueries(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseStatus? status,
        Guid? instructorId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Courses.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(course => course.Status == status.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(course => course.InstructorId == instructorId.Value);
        }

        var rows = await query
            .OrderByDescending(course => course.CreatedAt)
            .Select(course => new
            {
                course.Id,
                course.Title,
                course.Slug,
                course.Status,
                course.InstructorId,
                course.CreatedAt,
                course.PublishedAt,
                SectionCount = course.Sections.Count,
                LessonCount = course.Sections.SelectMany(section => section.Lessons).Count(),
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new CourseListItemDto(
                row.Id,
                row.Title,
                row.Slug.Value,
                row.Status.ToString(),
                row.InstructorId,
                row.CreatedAt,
                row.PublishedAt,
                row.SectionCount,
                row.LessonCount))
            .ToList();
    }
}
