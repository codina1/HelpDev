using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Learning.Infrastructure.Persistence;

public sealed class CourseRepository : ICourseRepository
{
    private readonly ILearningDbContext _dbContext;

    public CourseRepository(ILearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        CoursesWithGraph()
            .FirstOrDefaultAsync(course => course.Id == id, cancellationToken);

    public Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = CourseSlug.FromPersisted(slug);
        return CoursesWithGraph()
            .FirstOrDefaultAsync(course => course.Slug == slugValue, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(
        CourseSlug slug,
        Guid? excludingCourseId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slug);

        var query = _dbContext.Courses.Where(course => course.Slug == slug);

        if (excludingCourseId.HasValue)
        {
            query = query.Where(course => course.Id != excludingCourseId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        _dbContext.Courses.Add(course);
        return Task.CompletedTask;
    }

    private IQueryable<Course> CoursesWithGraph() =>
        _dbContext.Courses
            .Include(course => course.Sections)
            .ThenInclude(section => section.Lessons);
}
