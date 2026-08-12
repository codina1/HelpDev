namespace HelpDev.Modules.Learning.Domain.Courses;

public sealed class Lesson
{
    /// <summary>Required for EF Core materialization.</summary>
    private Lesson()
    {
    }

    internal Lesson(
        Guid id,
        string title,
        int order,
        Guid? contentId,
        string? videoUrl,
        int? durationMinutes,
        bool isPreview)
    {
        Id = id;
        Title = title;
        Order = order;
        ContentId = contentId;
        VideoUrl = videoUrl;
        DurationMinutes = durationMinutes;
        IsPreview = isPreview;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public int Order { get; internal set; }

    public Guid? ContentId { get; private set; }

    public string? VideoUrl { get; private set; }

    public int? DurationMinutes { get; private set; }

    public bool IsPreview { get; private set; }

    internal void Update(
        string title,
        Guid? contentId,
        string? videoUrl,
        int? durationMinutes,
        bool isPreview)
    {
        Title = title;
        ContentId = contentId;
        VideoUrl = videoUrl;
        DurationMinutes = durationMinutes;
        IsPreview = isPreview;
    }
}
