using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class EnrollmentCreationTests
{
    [Fact]
    public void Enroll_creates_active_enrollment_and_raises_enrolled_event()
    {
        var id = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var enrolledAt = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);

        var enrollment = Enrollment.Enroll(id, courseId, userId, enrolledAt);

        Assert.Equal(id, enrollment.Id);
        Assert.Equal(courseId, enrollment.CourseId);
        Assert.Equal(userId, enrollment.UserId);
        Assert.Equal(enrolledAt, enrollment.EnrolledAt);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Equal(0, enrollment.ProgressPercentage.Value);
        Assert.Empty(enrollment.LessonProgressEntries);

        var domainEvent = Assert.Single(enrollment.DomainEvents);
        var enrolled = Assert.IsType<StudentEnrolledDomainEvent>(domainEvent);
        Assert.Equal(id, enrolled.EnrollmentId);
        Assert.Equal(courseId, enrolled.CourseId);
        Assert.Equal(userId, enrolled.UserId);
    }

    [Fact]
    public void Enroll_rejects_default_course_id()
    {
        Assert.Throws<DomainException>(() =>
            Enrollment.Enroll(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void Enroll_rejects_default_user_id()
    {
        Assert.Throws<DomainException>(() =>
            Enrollment.Enroll(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, DateTime.UtcNow));
    }
}
