namespace HelpDev.Modules.Learning.Domain.Enrollments;

public sealed class LessonProgress
{
    /// <summary>Required for EF Core materialization.</summary>
    private LessonProgress()
    {
    }

    private LessonProgress(Guid lessonId, DateTime? startedAt, DateTime? completedAt)
    {
        LessonId = lessonId;
        StartedAt = startedAt;
        CompletedAt = completedAt;
    }

    public Guid LessonId { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public bool IsCompleted => CompletedAt.HasValue;

    internal static LessonProgress Start(Guid lessonId, DateTime startedAtUtc) =>
        new(lessonId, startedAtUtc, completedAt: null);

    internal static LessonProgress CreateCompleted(Guid lessonId, DateTime completedAtUtc) =>
        new(lessonId, completedAtUtc, completedAtUtc);

    /// <summary>
    /// Marks the lesson completed. Returns true only when this call newly completes it.
    /// </summary>
    internal bool TryComplete(DateTime completedAtUtc)
    {
        if (IsCompleted)
        {
            return false;
        }

        CompletedAt = completedAtUtc;
        StartedAt ??= completedAtUtc;
        return true;
    }
}
