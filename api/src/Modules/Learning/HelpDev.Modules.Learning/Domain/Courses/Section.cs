using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Learning.Domain.Courses;

public sealed class Section
{
    private readonly List<Lesson> _lessons = [];

    /// <summary>Required for EF Core materialization.</summary>
    private Section()
    {
    }

    internal Section(Guid id, string title, int order)
    {
        Id = id;
        Title = title;
        Order = order;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public int Order { get; internal set; }

    public IReadOnlyList<Lesson> Lessons => _lessons.AsReadOnly();

    internal void Rename(string title) => Title = title;

    internal Lesson AddLesson(
        Guid lessonId,
        string title,
        Guid? contentId,
        string? videoUrl,
        int? durationMinutes,
        bool isPreview)
    {
        if (_lessons.Any(l => l.Id == lessonId))
        {
            throw new DomainException("Lesson already exists in this section.");
        }

        var lesson = new Lesson(
            lessonId,
            title,
            _lessons.Count + 1,
            contentId,
            videoUrl,
            durationMinutes,
            isPreview);

        _lessons.Add(lesson);
        return lesson;
    }

    internal Lesson GetLesson(Guid lessonId)
    {
        var lesson = _lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson is null)
        {
            throw new DomainException("Lesson was not found.");
        }

        return lesson;
    }

    internal void ReorderLesson(Guid lessonId, int newOrder)
    {
        if (newOrder < 1 || newOrder > _lessons.Count)
        {
            throw new DomainException("Lesson order is invalid.");
        }

        var lesson = GetLesson(lessonId);
        _lessons.Remove(lesson);
        _lessons.Insert(newOrder - 1, lesson);
        RenumberLessons();
    }

    internal void RenumberLessons()
    {
        for (var i = 0; i < _lessons.Count; i++)
        {
            _lessons[i].Order = i + 1;
        }
    }
}
