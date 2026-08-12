using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Learning.Domain.Enrollments;

public sealed class Enrollment : AggregateRoot<Guid>
{
    private readonly List<LessonProgress> _lessonProgress = [];

    /// <summary>Required for EF Core materialization. Does not raise domain events.</summary>
    private Enrollment()
    {
    }

    private Enrollment(Guid id)
        : base(id)
    {
    }

    public Guid CourseId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime EnrolledAt { get; private set; }

    public EnrollmentStatus Status { get; private set; } = EnrollmentStatus.Active;

    public ProgressPercentage ProgressPercentage { get; private set; } = ProgressPercentage.Zero;

    public IReadOnlyList<LessonProgress> LessonProgressEntries => _lessonProgress.AsReadOnly();

    public static Enrollment Enroll(
        Guid id,
        Guid courseId,
        Guid userId,
        DateTime enrolledAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Enrollment id must not be empty.");
        }

        if (courseId == Guid.Empty)
        {
            throw new DomainException("Course id must not be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("User id must not be empty.");
        }

        var enrollment = new Enrollment(id)
        {
            CourseId = courseId,
            UserId = userId,
            EnrolledAt = enrolledAtUtc,
            Status = EnrollmentStatus.Active,
            ProgressPercentage = ProgressPercentage.Zero,
        };

        enrollment.AddDomainEvent(new StudentEnrolledDomainEvent(id, courseId, userId));
        return enrollment;
    }

    public void StartLesson(Guid lessonId, DateTime startedAtUtc)
    {
        if (lessonId == Guid.Empty)
        {
            throw new DomainException("Lesson id must not be empty.");
        }

        if (FindProgress(lessonId) is not null)
        {
            return;
        }

        _lessonProgress.Add(LessonProgress.Start(lessonId, startedAtUtc));
    }

    public void CompleteLesson(
        Guid lessonId,
        IReadOnlyCollection<Guid> courseLessonIds,
        DateTime completedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(courseLessonIds);

        if (lessonId == Guid.Empty)
        {
            throw new DomainException("Lesson id must not be empty.");
        }

        var progress = FindProgress(lessonId);
        if (progress is null)
        {
            progress = LessonProgress.CreateCompleted(lessonId, completedAtUtc);
            _lessonProgress.Add(progress);
            AddDomainEvent(new LessonCompletedDomainEvent(Id, lessonId, UserId));
        }
        else if (progress.TryComplete(completedAtUtc))
        {
            AddDomainEvent(new LessonCompletedDomainEvent(Id, lessonId, UserId));
        }

        RecalculateProgress(courseLessonIds);
    }

    private LessonProgress? FindProgress(Guid lessonId) =>
        _lessonProgress.FirstOrDefault(progress => progress.LessonId == lessonId);

    private void RecalculateProgress(IReadOnlyCollection<Guid> courseLessonIds)
    {
        var distinctLessonIds = courseLessonIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (distinctLessonIds.Length == 0)
        {
            ProgressPercentage = ProgressPercentage.Zero;
            return;
        }

        var completedCount = distinctLessonIds.Count(id =>
            FindProgress(id) is { IsCompleted: true });

        var percent = completedCount * 100 / distinctLessonIds.Length;
        ProgressPercentage = ProgressPercentage.From(percent);

        if (completedCount == distinctLessonIds.Length && Status == EnrollmentStatus.Active)
        {
            Status = EnrollmentStatus.Completed;
            AddDomainEvent(new CourseCompletedDomainEvent(Id, CourseId, UserId));
        }
    }
}
