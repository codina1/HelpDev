using HelpDev.Modules.Learning.Domain.Enrollments;

namespace HelpDev.Learning.Tests;

public sealed class EnrollmentProgressTests
{
    [Fact]
    public void StartLesson_records_progress_entry()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessonId = Guid.NewGuid();
        var startedAt = new DateTime(2026, 3, 2, 9, 0, 0, DateTimeKind.Utc);

        enrollment.StartLesson(lessonId, startedAt);

        var progress = Assert.Single(enrollment.LessonProgressEntries);
        Assert.Equal(lessonId, progress.LessonId);
        Assert.Equal(startedAt, progress.StartedAt);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void CompleteLesson_records_lesson_once_and_raises_event()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessonId = Guid.NewGuid();
        var courseLessons = new[] { lessonId, Guid.NewGuid() };

        enrollment.CompleteLesson(lessonId, courseLessons, DateTime.UtcNow);

        Assert.True(enrollment.LessonProgressEntries.Single(p => p.LessonId == lessonId).IsCompleted);
        Assert.Equal(50, enrollment.ProgressPercentage.Value);
        Assert.Single(enrollment.DomainEvents.OfType<LessonCompletedDomainEvent>());
    }

    [Fact]
    public void Duplicate_completion_is_noop_for_lesson_event()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessonId = Guid.NewGuid();
        var courseLessons = new[] { lessonId, Guid.NewGuid() };

        enrollment.CompleteLesson(lessonId, courseLessons, DateTime.UtcNow);
        enrollment.DequeueDomainEvents();

        enrollment.CompleteLesson(lessonId, courseLessons, DateTime.UtcNow.AddMinutes(1));

        Assert.Empty(enrollment.DomainEvents);
        Assert.Equal(50, enrollment.ProgressPercentage.Value);
        Assert.Single(enrollment.LessonProgressEntries.Where(p => p.IsCompleted));
    }

    [Fact]
    public void Progress_for_zero_supplied_lessons_stays_at_zero()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessonId = Guid.NewGuid();

        enrollment.CompleteLesson(lessonId, Array.Empty<Guid>(), DateTime.UtcNow);

        Assert.Equal(0, enrollment.ProgressPercentage.Value);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Contains(enrollment.DomainEvents, e => e is LessonCompletedDomainEvent);
        Assert.DoesNotContain(enrollment.DomainEvents, e => e is CourseCompletedDomainEvent);
    }

    [Fact]
    public void Partial_progress_is_calculated_from_supplied_lesson_ids()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessons = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        enrollment.CompleteLesson(lessons[0], lessons, DateTime.UtcNow);

        Assert.Equal(33, enrollment.ProgressPercentage.Value);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
    }

    [Fact]
    public void Full_progress_reaches_one_hundred()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessons = new[] { Guid.NewGuid(), Guid.NewGuid() };

        enrollment.CompleteLesson(lessons[0], lessons, DateTime.UtcNow);
        enrollment.CompleteLesson(lessons[1], lessons, DateTime.UtcNow);

        Assert.Equal(100, enrollment.ProgressPercentage.Value);
    }

    [Fact]
    public void Progress_never_exceeds_one_hundred()
    {
        var enrollment = CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessons = new[] { Guid.NewGuid() };

        enrollment.CompleteLesson(lessons[0], lessons, DateTime.UtcNow);
        enrollment.CompleteLesson(Guid.NewGuid(), lessons, DateTime.UtcNow);

        Assert.Equal(100, enrollment.ProgressPercentage.Value);
        Assert.True(enrollment.ProgressPercentage.Value <= 100);
    }

    internal static Enrollment CreateEnrollment() =>
        Enrollment.Enroll(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
}
