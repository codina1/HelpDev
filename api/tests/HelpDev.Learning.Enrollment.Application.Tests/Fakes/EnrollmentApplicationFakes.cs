using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }
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

internal sealed class FakeCourseLearningQueries : ICourseLearningQueries
{
    private readonly Dictionary<Guid, CourseLearningStructure> _structures = new();

    public CourseLearningStructure? StructureToReturn { get; set; }

    public void Seed(CourseLearningStructure structure) => _structures[structure.CourseId] = structure;

    public Task<CourseLearningStructure?> GetStructureAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        if (_structures.TryGetValue(courseId, out var structure))
        {
            return Task.FromResult<CourseLearningStructure?>(structure);
        }

        return Task.FromResult(StructureToReturn);
    }
}

internal sealed class FakeEnrollmentQueries : IEnrollmentQueries
{
    public Guid? LastUserId { get; private set; }

    public int ListCallCount { get; private set; }

    public IReadOnlyList<EnrollmentListItemDto> ItemsToReturn { get; set; } = [];

    public Task<IReadOnlyList<EnrollmentListItemDto>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ListCallCount++;
        LastUserId = userId;
        return Task.FromResult(ItemsToReturn);
    }
}

internal sealed class FakeEnrollmentRepository : IEnrollmentRepository
{
    private readonly List<EnrollmentEntity> _enrollments = [];

    public int AddCallCount { get; private set; }

    public int IndependentSaveCount { get; private set; }

    public Guid? LastLookupCourseId { get; private set; }

    public Guid? LastLookupUserId { get; private set; }

    public IReadOnlyList<EnrollmentEntity> Enrollments => _enrollments;

    public Task<EnrollmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_enrollments.FirstOrDefault(enrollment => enrollment.Id == id));

    public Task<EnrollmentEntity?> GetByCourseAndUserAsync(
        Guid courseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        LastLookupCourseId = courseId;
        LastLookupUserId = userId;
        return Task.FromResult(_enrollments.FirstOrDefault(enrollment =>
            enrollment.CourseId == courseId && enrollment.UserId == userId));
    }

    public Task AddAsync(EnrollmentEntity enrollment, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _enrollments.Add(enrollment);
        return Task.CompletedTask;
    }

    public void Seed(EnrollmentEntity enrollment) => _enrollments.Add(enrollment);
}
