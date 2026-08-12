using HelpDev.Learning.Enrollment.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests;

public sealed class EnrollmentCompleteLessonTests
{
    private readonly FakeEnrollmentRepository _repository = new();
    private readonly FakeCourseLearningQueries _courseQueries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 6, 3, 11, 0, 0, DateTimeKind.Utc));
    private readonly EnrollmentService _sut;

    public EnrollmentCompleteLessonTests()
    {
        _sut = new EnrollmentService(
            _repository,
            new FakeEnrollmentQueries(),
            _courseQueries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task CompleteLesson_first_lesson_sets_partial_progress()
    {
        var lessonA = Guid.NewGuid();
        var lessonB = Guid.NewGuid();
        var (courseId, userId, enrollment) = SeedEnrollmentAndCourse(lessonA, lessonB);
        enrollment.DequeueDomainEvents();

        var dto = await _sut.CompleteLessonAsync(new CompleteLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonA,
        });

        Assert.Equal(50, dto.ProgressPercentage);
        Assert.Equal(nameof(EnrollmentStatus.Active), dto.Status);
        Assert.True(dto.LessonProgress.Single(p => p.LessonId == lessonA).IsCompleted);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Single(enrollment.DomainEvents.OfType<LessonCompletedDomainEvent>());
        Assert.Empty(enrollment.DomainEvents.OfType<CourseCompletedDomainEvent>());
    }

    [Fact]
    public async Task CompleteLesson_final_lesson_marks_course_completed_once()
    {
        var lessonA = Guid.NewGuid();
        var lessonB = Guid.NewGuid();
        var (courseId, userId, enrollment) = SeedEnrollmentAndCourse(lessonA, lessonB);
        enrollment.DequeueDomainEvents();

        await _sut.CompleteLessonAsync(new CompleteLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonA,
        });
        enrollment.DequeueDomainEvents();

        var dto = await _sut.CompleteLessonAsync(new CompleteLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonB,
        });

        Assert.Equal(100, dto.ProgressPercentage);
        Assert.Equal(nameof(EnrollmentStatus.Completed), dto.Status);
        Assert.Single(enrollment.DomainEvents.OfType<LessonCompletedDomainEvent>());
        Assert.Single(enrollment.DomainEvents.OfType<CourseCompletedDomainEvent>());
        Assert.Equal(2, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CompleteLesson_duplicate_completion_is_noop_for_events()
    {
        var lessonId = Guid.NewGuid();
        var (courseId, userId, enrollment) = SeedEnrollmentAndCourse(lessonId);
        enrollment.DequeueDomainEvents();

        await _sut.CompleteLessonAsync(new CompleteLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonId,
        });
        enrollment.DequeueDomainEvents();

        var dto = await _sut.CompleteLessonAsync(new CompleteLessonRequest
        {
            CourseId = courseId,
            UserId = userId,
            LessonId = lessonId,
        });

        Assert.Equal(100, dto.ProgressPercentage);
        Assert.Empty(enrollment.DomainEvents);
        Assert.Equal(2, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CompleteLesson_rejects_lesson_not_in_course()
    {
        var (courseId, userId, _) = SeedEnrollmentAndCourse(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() =>
            _sut.CompleteLessonAsync(new CompleteLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = Guid.NewGuid(),
            }));

        Assert.Equal(EnrollmentErrorCodes.LessonNotInCourse, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    private (Guid CourseId, Guid UserId, EnrollmentEntity Enrollment) SeedEnrollmentAndCourse(
        params Guid[] lessonIds)
    {
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _courseQueries.Seed(new CourseLearningStructure(courseId, CourseStatus.Published, lessonIds));
        var enrollment = EnrollmentEntity.Enroll(Guid.NewGuid(), courseId, userId, _clock.UtcNow);
        _repository.Seed(enrollment);
        return (courseId, userId, enrollment);
    }
}
