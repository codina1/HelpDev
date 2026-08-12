using HelpDev.Learning.Enrollment.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests;

public sealed class EnrollmentStartLessonTests
{
    private readonly FakeEnrollmentRepository _repository = new();
    private readonly FakeCourseLearningQueries _courseQueries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc));
    private readonly EnrollmentService _sut;

    public EnrollmentStartLessonTests()
    {
        _sut = new EnrollmentService(
            _repository,
            new FakeEnrollmentQueries(),
            _courseQueries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task StartLesson_valid_request_commits_once()
    {
        var (courseId, userId, lessonId) = SeedEnrollmentAndCourse();

        var dto = await _sut.StartLessonAsync(new StartLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonId,
        });

        var progress = Assert.Single(dto.LessonProgress);
        Assert.Equal(lessonId, progress.LessonId);
        Assert.Equal(_clock.UtcNow, progress.StartedAt);
        Assert.False(progress.IsCompleted);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task StartLesson_missing_enrollment_throws()
    {
        var courseId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        SeedPublishedCourse(courseId, lessonId);

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = Guid.NewGuid(),
                LessonId = lessonId,
            }));

        Assert.Equal(EnrollmentErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task StartLesson_missing_course_throws()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repository.Seed(EnrollmentEntity.Enroll(Guid.NewGuid(), courseId, userId, _clock.UtcNow));

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.CourseNotFound, ex.Code);
    }

    [Fact]
    public async Task StartLesson_rejects_lesson_not_in_course()
    {
        var (courseId, userId, _) = SeedEnrollmentAndCourse();

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.LessonNotInCourse, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task StartLesson_rejects_empty_lesson_id()
    {
        var (courseId, userId, _) = SeedEnrollmentAndCourse();

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = Guid.Empty,
            }));

        Assert.Equal(EnrollmentErrorCodes.LessonInvalid, ex.Code);
    }

    private (Guid CourseId, Guid UserId, Guid LessonId) SeedEnrollmentAndCourse()
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        SeedPublishedCourse(courseId, lessonId);
        _repository.Seed(EnrollmentEntity.Enroll(Guid.NewGuid(), courseId, userId, _clock.UtcNow));
        return (courseId, userId, lessonId);
    }

    private void SeedPublishedCourse(Guid courseId, params Guid[] lessonIds) =>
        _courseQueries.Seed(new CourseLearningStructure(courseId, CourseStatus.Published, lessonIds));
}
