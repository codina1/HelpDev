using HelpDev.Learning.Enrollment.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests;

public sealed class EnrollmentCreationTests
{
    private readonly FakeEnrollmentRepository _repository = new();
    private readonly FakeEnrollmentQueries _queries = new();
    private readonly FakeCourseLearningQueries _courseQueries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc));
    private readonly EnrollmentService _sut;

    public EnrollmentCreationTests()
    {
        _sut = new EnrollmentService(_repository, _queries, _courseQueries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Enroll_valid_request_adds_once_commits_once_and_returns_dto()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        SeedPublishedCourse(courseId, lessonId);

        var dto = await _sut.EnrollAsync(new EnrollStudentRequest
        {
            CourseId = courseId,
            UserId = userId,
        });

        Assert.Equal(courseId, dto.CourseId);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(_clock.UtcNow, dto.EnrolledAt);
        Assert.Equal(nameof(EnrollmentStatus.Active), dto.Status);
        Assert.Equal(0, dto.ProgressPercentage);
        Assert.Empty(dto.LessonProgress);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(0, _repository.IndependentSaveCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        var enrollment = Assert.Single(_repository.Enrollments);
        Assert.Contains(enrollment.DomainEvents, e => e is StudentEnrolledDomainEvent);
    }

    [Fact]
    public async Task Enroll_rejects_empty_course_id()
    {
        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = Guid.Empty,
                UserId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.CourseInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Enroll_rejects_empty_user_id()
    {
        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = Guid.NewGuid(),
                UserId = Guid.Empty,
            }));

        Assert.Equal(EnrollmentErrorCodes.UserInvalid, ex.Code);
    }

    [Fact]
    public async Task Enroll_rejects_missing_course()
    {
        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.CourseNotFound, ex.Code);
    }

    [Fact]
    public async Task Enroll_rejects_unpublished_course()
    {
        var courseId = Guid.NewGuid();
        _courseQueries.Seed(new CourseLearningStructure(
            courseId,
            CourseStatus.Draft,
            [Guid.NewGuid()]));

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = courseId,
                UserId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.CourseNotPublished, ex.Code);
    }

    [Fact]
    public async Task Enroll_rejects_published_course_with_zero_lessons()
    {
        var courseId = Guid.NewGuid();
        _courseQueries.Seed(new CourseLearningStructure(
            courseId,
            CourseStatus.Published,
            Array.Empty<Guid>()));

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = courseId,
                UserId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.CourseHasNoLessons, ex.Code);
        Assert.Equal(0, _repository.AddCallCount);
    }

    [Fact]
    public async Task Enroll_rejects_duplicate_enrollment()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SeedPublishedCourse(courseId, Guid.NewGuid());
        _repository.Seed(EnrollmentEntity.Enroll(Guid.NewGuid(), courseId, userId, _clock.UtcNow));

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.EnrollAsync(new EnrollStudentRequest
            {
                CourseId = courseId,
                UserId = userId,
            }));

        Assert.Equal(EnrollmentErrorCodes.AlreadyExists, ex.Code);
        Assert.Equal(0, _repository.AddCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    private void SeedPublishedCourse(Guid courseId, Guid lessonId) =>
        _courseQueries.Seed(new CourseLearningStructure(
            courseId,
            CourseStatus.Published,
            [lessonId]));
}
