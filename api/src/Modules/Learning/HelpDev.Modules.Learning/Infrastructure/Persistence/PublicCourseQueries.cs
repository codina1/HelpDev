using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class PublicCourseQueries : IPublicCourseQueries
{
    private readonly ILearningDbContext _dbContext;

    public PublicCourseQueries(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CourseListItemDto>> ListPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Status == CourseStatus.Published)
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

    public async Task<CourseDetailDto?> GetPublishedByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await PublishedCoursesWithGraph()
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);

        return course is null ? null : CourseMapper.ToDetailDto(course);
    }

    public async Task<CourseDetailDto?> GetPublishedBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (!CourseSlug.TryCreate(slug, out var courseSlug) || courseSlug is null)
        {
            throw new CourseException("Course slug is invalid.", CourseErrorCodes.SlugInvalid);
        }

        var course = await PublishedCoursesWithGraph()
            .FirstOrDefaultAsync(item => item.Slug == courseSlug, cancellationToken);

        return course is null ? null : CourseMapper.ToDetailDto(course);
    }

    public async Task<IReadOnlyList<CourseSearchSourceDto>> ListPublishedSearchBatchAsync(
        Guid? afterCourseId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var query = _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Status == CourseStatus.Published);

        if (afterCourseId.HasValue)
        {
            var after = afterCourseId.Value;
            query = query.Where(course => course.Id > after);
        }

        var rows = await query
            .OrderBy(course => course.Id)
            .Take(take)
            .Select(course => new
            {
                course.Id,
                course.Title,
                course.Slug,
                course.Description,
                course.CreatedAt,
                course.PublishedAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new CourseSearchSourceDto(
                row.Id,
                row.Title,
                row.Slug.Value,
                row.Description,
                row.CreatedAt,
                row.PublishedAt))
            .ToList();
    }

    private IQueryable<Course> PublishedCoursesWithGraph() =>
        _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Status == CourseStatus.Published)
            .Include(course => course.Sections)
            .ThenInclude(section => section.Lessons);
}
