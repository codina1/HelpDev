using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class CourseUpdatedDomainEventTests
{
    [Fact]
    public void Updating_published_course_searchable_details_raises_CourseUpdatedDomainEvent()
    {
        var course = CreatePublishedCourse("original-slug");
        course.DequeueDomainEvents();

        course.UpdateDetails(
            "Fresh Title",
            CourseSlug.Create("fresh-slug"),
            "Fresh description");

        Assert.Equal("Fresh Title", course.Title);
        Assert.Equal("fresh-slug", course.Slug.Value);
        Assert.Equal("Fresh description", course.Description);

        Assert.Contains(course.DomainEvents, e => e is CourseUpdatedDomainEvent);
        Assert.Contains(course.DomainEvents, e => e is LessonPublishedDomainEvent);
        var updated = Assert.Single(course.DomainEvents.OfType<CourseUpdatedDomainEvent>());
        Assert.Equal(course.Id, updated.CourseId);
    }

    [Fact]
    public void Draft_course_update_does_not_raise_CourseUpdatedDomainEvent()
    {
        var course = CourseCreationTests.CreateDraftCourse();

        course.UpdateDetails(
            "Draft Title",
            CourseSlug.Create("draft-title"),
            "Draft description");

        Assert.Equal("Draft Title", course.Title);
        Assert.Empty(course.DomainEvents);
        Assert.DoesNotContain(course.DomainEvents, e => e is CourseUpdatedDomainEvent);
    }

    [Fact]
    public void Invalid_update_does_not_raise_event_or_mutate_title()
    {
        var course = CreatePublishedCourse("keep-slug");
        course.DequeueDomainEvents();
        var originalTitle = course.Title;

        Assert.Throws<DomainException>(() =>
            course.UpdateDetails("  ", CourseSlug.Create("keep-slug"), "Desc"));

        Assert.Equal(originalTitle, course.Title);
        Assert.Empty(course.DomainEvents);
    }

    [Fact]
    public void Unchanged_published_details_do_not_raise_event()
    {
        var course = CreatePublishedCourse("same-slug");
        course.DequeueDomainEvents();

        course.UpdateDetails(course.Title, course.Slug, course.Description);

        Assert.Empty(course.DomainEvents);
    }

    [Fact]
    public void Publish_then_update_raises_events_in_order()
    {
        var course = CreatePublishableDraft();
        var publishedAt = new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc);

        course.Publish(publishedAt);
        course.UpdateDetails(
            "After Publish",
            CourseSlug.Create("after-publish"),
            "Updated after publish");

        Assert.Contains(course.DomainEvents, e => e is CoursePublishedDomainEvent);
        Assert.Contains(course.DomainEvents, e => e is CourseUpdatedDomainEvent);
        Assert.Contains(course.DomainEvents, e => e is LessonPublishedDomainEvent);
        Assert.Equal(course.Id, Assert.Single(course.DomainEvents.OfType<CourseUpdatedDomainEvent>()).CourseId);
    }

    private static Course CreatePublishedCourse(string slug)
    {
        var course = CreatePublishableDraft(slug);
        course.Publish(new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc));
        return course;
    }

    private static Course CreatePublishableDraft(string slug = "course-title")
    {
        var course = CourseCreationTests.CreateDraftCourse(slug: slug);
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section 1");
        course.AddLesson(sectionId, Guid.NewGuid(), "Lesson 1");
        return course;
    }
}
