using HelpDev.Modules.Learning.Domain.Enrollments;

namespace HelpDev.Learning.Tests;

public sealed class EnrollmentCompletionTests
{
    [Fact]
    public void Full_completion_raises_course_completed_event_once()
    {
        var enrollment = EnrollmentProgressTests.CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessons = new[] { Guid.NewGuid(), Guid.NewGuid() };

        enrollment.CompleteLesson(lessons[0], lessons, DateTime.UtcNow);
        enrollment.CompleteLesson(lessons[1], lessons, DateTime.UtcNow);

        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.Equal(100, enrollment.ProgressPercentage.Value);
        Assert.Single(enrollment.DomainEvents.OfType<CourseCompletedDomainEvent>());
        Assert.Equal(2, enrollment.DomainEvents.OfType<LessonCompletedDomainEvent>().Count());
    }

    [Fact]
    public void Recalculation_after_completion_does_not_duplicate_course_completed_event()
    {
        var enrollment = EnrollmentProgressTests.CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessons = new[] { Guid.NewGuid(), Guid.NewGuid() };

        enrollment.CompleteLesson(lessons[0], lessons, DateTime.UtcNow);
        enrollment.CompleteLesson(lessons[1], lessons, DateTime.UtcNow);
        enrollment.DequeueDomainEvents();

        enrollment.CompleteLesson(lessons[1], lessons, DateTime.UtcNow.AddMinutes(5));

        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.Empty(enrollment.DomainEvents);
    }

    [Fact]
    public void Lesson_completed_event_is_raised_only_once_per_lesson()
    {
        var enrollment = EnrollmentProgressTests.CreateEnrollment();
        enrollment.DequeueDomainEvents();
        var lessonId = Guid.NewGuid();
        var lessons = new[] { lessonId, Guid.NewGuid() };

        enrollment.StartLesson(lessonId, DateTime.UtcNow);
        enrollment.CompleteLesson(lessonId, lessons, DateTime.UtcNow);
        enrollment.CompleteLesson(lessonId, lessons, DateTime.UtcNow.AddSeconds(1));

        Assert.Single(enrollment.DomainEvents.OfType<LessonCompletedDomainEvent>());
    }
}
