using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using HelpDev.Testing.Analytics;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Learning.Application.Tests.Fakes;

internal static class CourseActors
{
    public static CourseManagementActor Owner(Guid userId) =>
        new(userId, canManageAllCourses: false);

    public static CourseManagementActor Admin(Guid userId) =>
        new(userId, canManageAllCourses: true);
}

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }

    public void SetUtcNow(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeCourseQueries : ICourseQueries
{
    public CourseStatus? LastStatusFilter { get; private set; }

    public Guid? LastInstructorIdFilter { get; private set; }

    public int ListCallCount { get; private set; }

    public IReadOnlyList<CourseListItemDto> ItemsToReturn { get; set; } = [];

    public Task<IReadOnlyList<CourseListItemDto>> ListAsync(
        CourseStatus? status,
        Guid? instructorId,
        CancellationToken cancellationToken = default)
    {
        ListCallCount++;
        LastStatusFilter = status;
        LastInstructorIdFilter = instructorId;
        return Task.FromResult(ItemsToReturn);
    }
}

internal sealed class FakeCourseRepository : ICourseRepository
{
    private readonly List<Course> _courses = [];

    public int AddCallCount { get; private set; }

    /// <summary>
    /// Learning repositories must not commit; this stays zero for AddAsync.
    /// </summary>
    public int IndependentSaveCount { get; private set; }

    public IReadOnlyList<Course> Courses => _courses;

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_courses.FirstOrDefault(course => course.Id == id));

    public Task<Course?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var slugValue = CourseSlug.FromPersisted(slug);
        return Task.FromResult(_courses.FirstOrDefault(course => course.Slug == slugValue));
    }

    public Task<bool> SlugExistsAsync(
        CourseSlug slug,
        Guid? excludingCourseId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = _courses.Any(course =>
            course.Slug == slug
            && (!excludingCourseId.HasValue || course.Id != excludingCourseId.Value));
        return Task.FromResult(exists);
    }

    public Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _courses.Add(course);
        // Intentionally does not increment IndependentSaveCount / SaveChanges.
        return Task.CompletedTask;
    }

    public void Seed(Course course) => _courses.Add(course);
}

internal sealed class FakeEnrollmentRepository : IEnrollmentRepository
{
    private readonly List<Enrollment> _enrollments = [];

    public int AddCallCount { get; private set; }

    public int IndependentSaveCount { get; private set; }

    public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_enrollments.FirstOrDefault(enrollment => enrollment.Id == id));

    public Task<Enrollment?> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_enrollments.FirstOrDefault(enrollment =>
            enrollment.CourseId == courseId && enrollment.UserId == userId));

    public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _enrollments.Add(enrollment);
        return Task.CompletedTask;
    }
}

internal static class LearningServiceFactory
{
    public static CourseService CreateCourseService(
        FakeCourseRepository repository,
        FakeCourseQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock) =>
        new(
            repository,
            queries,
            unitOfWork,
            clock,
            new NoOpAnalyticsEventIngestor(),
            NullLogger<CourseService>.Instance);
}
