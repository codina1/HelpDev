using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class CoursePublishingTests
{
    [Fact]
    public void Publish_empty_course_fails()
    {
        var course = CourseCreationTests.CreateDraftCourse();

        Assert.Throws<DomainException>(() => course.Publish(DateTime.UtcNow));
        Assert.Equal(CourseStatus.Draft, course.Status);
    }

    [Fact]
    public void Publish_course_with_empty_section_fails()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        course.AddSection(Guid.NewGuid(), "Empty Section");

        Assert.Throws<DomainException>(() => course.Publish(DateTime.UtcNow));
        Assert.Equal(CourseStatus.Draft, course.Status);
    }

    [Fact]
    public void Publish_valid_course_succeeds_and_raises_course_and_lesson_events()
    {
        var course = CreatePublishableCourse();
        var publishedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc);

        course.Publish(publishedAt);

        Assert.Equal(CourseStatus.Published, course.Status);
        Assert.Equal(publishedAt, course.PublishedAt);
        Assert.Contains(course.DomainEvents, e => e is CoursePublishedDomainEvent);
        Assert.Contains(course.DomainEvents, e => e is LessonPublishedDomainEvent);
        var published = Assert.Single(course.DomainEvents.OfType<CoursePublishedDomainEvent>());
        Assert.Equal(course.Id, published.CourseId);
        Assert.Equal(course.Slug.Value, published.Slug);
    }

    [Fact]
    public void Second_publish_is_noop_and_raises_no_event()
    {
        var course = CreatePublishableCourse();
        course.Publish(DateTime.UtcNow);
        course.DequeueDomainEvents();

        course.Publish(DateTime.UtcNow.AddHours(1));

        Assert.Equal(CourseStatus.Published, course.Status);
        Assert.False(course.HasDomainEvents);
        Assert.Empty(course.DomainEvents);
    }

    private static Course CreatePublishableCourse()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section 1");
        course.AddLesson(sectionId, Guid.NewGuid(), "Lesson 1");
        return course;
    }
}
