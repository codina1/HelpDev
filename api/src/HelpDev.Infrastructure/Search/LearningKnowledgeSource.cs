using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Infrastructure.Search;

/// <summary>Published courses and lessons → knowledge documents.</summary>
public sealed class LearningKnowledgeSource : ICourseSearchSource, ILessonSearchSource
{
    public const int SummaryMaxLength = 280;

    private readonly ILearningDbContext _learningDb;
    private readonly IContentDbContext _contentDb;

    public LearningKnowledgeSource(ILearningDbContext learningDb, IContentDbContext contentDb)
    {
        _learningDb = learningDb;
        _contentDb = contentDb;
    }

    public async Task<SearchSourceDocument?> GetByIdAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _learningDb.Courses.AsNoTracking()
            .Where(c => c.Id == courseId && c.Status == CourseStatus.Published)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Slug,
                c.Description,
                c.CreatedAt,
                c.PublishedAt,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return course is null
            ? null
            : MapCourse(
                course.Id,
                course.Title,
                course.Slug.Value,
                course.Description,
                course.CreatedAt,
                course.PublishedAt);
    }

    async Task<SearchSourceDocument?> ILessonSearchSource.GetByIdAsync(
        Guid lessonId,
        CancellationToken cancellationToken)
    {
        var course = await _learningDb.Courses.AsNoTracking()
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .FirstOrDefaultAsync(
                c => c.Sections.Any(s => s.Lessons.Any(l => l.Id == lessonId)),
                cancellationToken);

        if (course is null)
        {
            return null;
        }

        var lesson = course.Sections.SelectMany(s => s.Lessons).First(l => l.Id == lessonId);
        return await MapLessonAsync(course, lesson, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListIdsByCourseAsync(
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await _learningDb.Courses.AsNoTracking()
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .Where(c => c.Id == courseId && c.Status == CourseStatus.Published)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return [];
        }

        return course.Sections
            .SelectMany(s => s.Lessons)
            .OrderBy(l => l.Order)
            .Select(l => l.Id)
            .ToList();
    }

    public async Task<IReadOnlyList<SearchSourceDocument>> GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var query = _learningDb.Courses.AsNoTracking()
            .Where(c => c.Status == CourseStatus.Published);

        if (afterSourceId.HasValue)
        {
            var after = afterSourceId.Value;
            query = query.Where(c => c.Id > after);
        }

        var rows = await query
            .OrderBy(c => c.Id)
            .Take(take)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Slug,
                c.Description,
                c.CreatedAt,
                c.PublishedAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => MapCourse(
                row.Id,
                row.Title,
                row.Slug.Value,
                row.Description,
                row.CreatedAt,
                row.PublishedAt))
            .ToList();
    }

    async Task<IReadOnlyList<SearchSourceDocument>> ILessonSearchSource.GetPublishedBatchAsync(
        Guid? afterSourceId,
        int take,
        CancellationToken cancellationToken)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var courses = await _learningDb.Courses.AsNoTracking()
            .Include(c => c.Sections)
            .ThenInclude(s => s.Lessons)
            .Where(c => c.Status == CourseStatus.Published)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        var lessons = courses
            .SelectMany(c => c.Sections.SelectMany(s => s.Lessons.Select(l => (Course: c, Lesson: l))))
            .OrderBy(x => x.Lesson.Id)
            .AsEnumerable();

        if (afterSourceId.HasValue)
        {
            lessons = lessons.Where(x => x.Lesson.Id.CompareTo(afterSourceId.Value) > 0);
        }

        var page = lessons.Take(take).ToList();
        var result = new List<SearchSourceDocument>(page.Count);
        foreach (var item in page)
        {
            result.Add(await MapLessonAsync(item.Course, item.Lesson, cancellationToken));
        }

        return result;
    }

    private async Task<SearchSourceDocument> MapLessonAsync(
        Course course,
        Lesson lesson,
        CancellationToken cancellationToken)
    {
        var bodyParts = new List<string>
        {
            course.Title,
            lesson.Title,
        };

        if (lesson.ContentId is Guid contentId)
        {
            var content = await _contentDb.Contents.AsNoTracking()
                .Where(c => c.Id == contentId && c.Status == ContentStatus.Published)
                .Select(c => new { c.Title, c.Body })
                .FirstOrDefaultAsync(cancellationToken);

            if (content is not null)
            {
                bodyParts.Add(content.Title);
                bodyParts.Add(content.Body);
            }
        }

        var body = string.Join("\n\n", bodyParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        var updatedAt = course.PublishedAt ?? course.CreatedAt;

        return new SearchSourceDocument(
            lesson.Id,
            KnowledgeSourceType.Lesson,
            lesson.Title,
            $"{course.Slug.Value}/lessons/{lesson.Id:N}",
            Truncate(body),
            $"/courses/{course.Slug.Value}",
            IsPublished: true,
            PublishedAtUtc: course.PublishedAt,
            UpdatedAtUtc: updatedAt,
            Body: body);
    }

    private static SearchSourceDocument MapCourse(
        Guid id,
        string title,
        string slug,
        string description,
        DateTime createdAt,
        DateTime? publishedAt)
    {
        var updatedAt = publishedAt ?? createdAt;
        return new SearchSourceDocument(
            id,
            KnowledgeSourceType.Course,
            title,
            slug,
            Truncate(description),
            $"/courses/{slug}",
            IsPublished: true,
            PublishedAtUtc: publishedAt,
            UpdatedAtUtc: updatedAt,
            Body: description);
    }

    private static string Truncate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= SummaryMaxLength
            ? trimmed
            : trimmed[..SummaryMaxLength];
    }
}
