using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Learning.Domain.Courses;

public sealed class Course : AggregateRoot<Guid>
{
    private readonly List<Section> _sections = [];

    /// <summary>Required for EF Core materialization. Does not raise domain events.</summary>
    private Course()
    {
    }

    private Course(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public CourseSlug Slug { get; private set; } = null!;

    public string Description { get; private set; } = string.Empty;

    public Guid InstructorId { get; private set; }

    public CourseStatus Status { get; private set; } = CourseStatus.Draft;

    public IReadOnlyList<Section> Sections => _sections.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public static Course CreateDraft(
        Guid id,
        string title,
        CourseSlug slug,
        string description,
        Guid instructorId,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(slug);

        if (id == Guid.Empty)
        {
            throw new DomainException("Course id must not be empty.");
        }

        if (instructorId == Guid.Empty)
        {
            throw new DomainException("Instructor id must not be empty.");
        }

        var course = new Course(id)
        {
            InstructorId = instructorId,
            CreatedAt = createdAtUtc,
            Status = CourseStatus.Draft,
        };

        course.ApplyDetails(title, slug, description, raiseUpdatedEventWhenPublished: false);
        return course;
    }

    public void UpdateDetails(string title, CourseSlug slug, string description)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ApplyDetails(title, slug, description, raiseUpdatedEventWhenPublished: true);
    }

    public Section AddSection(Guid sectionId, string title)
    {
        if (sectionId == Guid.Empty)
        {
            throw new DomainException("Section id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Section title must not be empty.");
        }

        if (_sections.Any(s => s.Id == sectionId))
        {
            throw new DomainException("Section already exists.");
        }

        var section = new Section(sectionId, title.Trim(), _sections.Count + 1);
        _sections.Add(section);
        return section;
    }

    public void RenameSection(Guid sectionId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Section title must not be empty.");
        }

        GetSection(sectionId).Rename(title.Trim());
    }

    public void ReorderSection(Guid sectionId, int newOrder)
    {
        if (newOrder < 1 || newOrder > _sections.Count)
        {
            throw new DomainException("Section order is invalid.");
        }

        var section = GetSection(sectionId);
        _sections.Remove(section);
        _sections.Insert(newOrder - 1, section);
        RenumberSections();
    }

    public Lesson AddLesson(
        Guid sectionId,
        Guid lessonId,
        string title,
        Guid? contentId = null,
        string? videoUrl = null,
        int? durationMinutes = null,
        bool isPreview = false)
    {
        if (lessonId == Guid.Empty)
        {
            throw new DomainException("Lesson id must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Lesson title must not be empty.");
        }

        if (durationMinutes is < 0)
        {
            throw new DomainException("Lesson duration must not be negative.");
        }

        var normalizedVideoUrl = string.IsNullOrWhiteSpace(videoUrl) ? null : videoUrl.Trim();

        var lesson = GetSection(sectionId).AddLesson(
            lessonId,
            title.Trim(),
            contentId == Guid.Empty ? null : contentId,
            normalizedVideoUrl,
            durationMinutes,
            isPreview);

        RaiseLessonPublishedIfCoursePublished(lesson.Id);
        return lesson;
    }

    public void UpdateLesson(
        Guid sectionId,
        Guid lessonId,
        string title,
        Guid? contentId = null,
        string? videoUrl = null,
        int? durationMinutes = null,
        bool isPreview = false)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Lesson title must not be empty.");
        }

        if (durationMinutes is < 0)
        {
            throw new DomainException("Lesson duration must not be negative.");
        }

        var lesson = GetSection(sectionId).GetLesson(lessonId);
        var normalizedVideoUrl = string.IsNullOrWhiteSpace(videoUrl) ? null : videoUrl.Trim();

        lesson.Update(
            title.Trim(),
            contentId == Guid.Empty ? null : contentId,
            normalizedVideoUrl,
            durationMinutes,
            isPreview);

        RaiseLessonPublishedIfCoursePublished(lesson.Id);
    }

    public void ReorderLesson(Guid sectionId, Guid lessonId, int newOrder) =>
        GetSection(sectionId).ReorderLesson(lessonId, newOrder);

    public void Publish(DateTime publishedAtUtc)
    {
        if (Status == CourseStatus.Published)
        {
            return;
        }

        if (_sections.Count == 0)
        {
            throw new DomainException("Cannot publish a course with no sections.");
        }

        if (_sections.Any(s => s.Lessons.Count == 0))
        {
            throw new DomainException("Cannot publish a course that contains an empty section.");
        }

        Status = CourseStatus.Published;
        PublishedAt = publishedAtUtc;
        AddDomainEvent(new CoursePublishedDomainEvent(Id, Slug.Value));
        RaiseLessonPublishedEvents();
    }

    private void RaiseLessonPublishedEvents()
    {
        foreach (var lesson in _sections.SelectMany(section => section.Lessons))
        {
            AddDomainEvent(new LessonPublishedDomainEvent(lesson.Id, Id, Slug.Value));
        }
    }

    private void RaiseLessonPublishedIfCoursePublished(Guid lessonId)
    {
        if (Status != CourseStatus.Published)
        {
            return;
        }

        AddDomainEvent(new LessonPublishedDomainEvent(lessonId, Id, Slug.Value));
        AddDomainEvent(new CourseUpdatedDomainEvent(Id));
    }

    private Section GetSection(Guid sectionId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
        {
            throw new DomainException("Section was not found.");
        }

        return section;
    }

    private void RenumberSections()
    {
        for (var i = 0; i < _sections.Count; i++)
        {
            _sections[i].Order = i + 1;
        }
    }

    private void ApplyDetails(
        string title,
        CourseSlug slug,
        string description,
        bool raiseUpdatedEventWhenPublished)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Course title must not be empty.");
        }

        var normalizedTitle = title.Trim();
        var normalizedDescription = description?.Trim() ?? string.Empty;

        var changed =
            !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || Slug is null
            || Slug != slug
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal);

        Title = normalizedTitle;
        Slug = slug;
        Description = normalizedDescription;

        if (raiseUpdatedEventWhenPublished && changed && Status == CourseStatus.Published)
        {
            AddDomainEvent(new CourseUpdatedDomainEvent(Id));
            RaiseLessonPublishedEvents();
        }
    }
}
