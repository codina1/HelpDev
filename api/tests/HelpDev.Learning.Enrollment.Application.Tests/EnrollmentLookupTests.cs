using HelpDev.Learning.Enrollment.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Enrollments;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests;

public sealed class EnrollmentLookupTests
{
    private readonly FakeEnrollmentRepository _repository = new();
    private readonly FakeDateTimeProvider _clock = new(DateTime.UtcNow);
    private readonly EnrollmentService _sut;

    public EnrollmentLookupTests()
    {
        _sut = new EnrollmentService(
            _repository,
            new FakeEnrollmentQueries(),
            new FakeCourseLearningQueries(),
            new FakeUnitOfWork(),
            _clock);
    }

    [Fact]
    public async Task GetByCourseAndUser_returns_existing_enrollment()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enrollment = EnrollmentEntity.Enroll(Guid.NewGuid(), courseId, userId, _clock.UtcNow);
        _repository.Seed(enrollment);

        var dto = await _sut.GetByCourseAndUserAsync(courseId, userId);

        Assert.Equal(enrollment.Id, dto.Id);
        Assert.Equal(courseId, _repository.LastLookupCourseId);
        Assert.Equal(userId, _repository.LastLookupUserId);
    }

    [Fact]
    public async Task GetByCourseAndUser_throws_when_missing()
    {
        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.GetByCourseAndUserAsync(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(EnrollmentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task GetById_returns_enrollment_for_same_user_only()
    {
        var ownerId = Guid.NewGuid();
        var enrollment = EnrollmentEntity.Enroll(Guid.NewGuid(), Guid.NewGuid(), ownerId, _clock.UtcNow);
        _repository.Seed(enrollment);

        var dto = await _sut.GetByIdAsync(enrollment.Id, ownerId);
        Assert.Equal(enrollment.Id, dto.Id);

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.GetByIdAsync(enrollment.Id, Guid.NewGuid()));
        Assert.Equal(EnrollmentErrorCodes.NotFound, ex.Code);
    }
}
