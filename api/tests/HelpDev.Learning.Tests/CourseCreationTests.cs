using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class CourseCreationTests
{
    [Fact]
    public void CreateDraft_creates_draft_course_with_details()
    {
        var id = Guid.NewGuid();
        var instructorId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var course = Course.CreateDraft(
            id,
            "  C# Fundamentals  ",
            CourseSlug.Create("csharp-fundamentals"),
            "  Intro course  ",
            instructorId,
            createdAt);

        Assert.Equal(id, course.Id);
        Assert.Equal("C# Fundamentals", course.Title);
        Assert.Equal("csharp-fundamentals", course.Slug.Value);
        Assert.Equal("Intro course", course.Description);
        Assert.Equal(instructorId, course.InstructorId);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Equal(createdAt, course.CreatedAt);
        Assert.Null(course.PublishedAt);
        Assert.Empty(course.Sections);
        Assert.False(course.HasDomainEvents);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateDraft_rejects_invalid_title(string? title)
    {
        Assert.Throws<DomainException>(() =>
            Course.CreateDraft(
                Guid.NewGuid(),
                title!,
                CourseSlug.Create("valid-slug"),
                "Description",
                Guid.NewGuid(),
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateDraft_rejects_null_slug()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Course.CreateDraft(
                Guid.NewGuid(),
                "Title",
                null!,
                "Description",
                Guid.NewGuid(),
                DateTime.UtcNow));
    }

    [Fact]
    public void CreateDraft_rejects_empty_instructor_id()
    {
        Assert.Throws<DomainException>(() =>
            Course.CreateDraft(
                Guid.NewGuid(),
                "Title",
                CourseSlug.Create("valid-slug"),
                "Description",
                Guid.Empty,
                DateTime.UtcNow));
    }

    [Fact]
    public void UpdateDetails_updates_title_slug_and_description()
    {
        var course = CreateDraftCourse();

        course.UpdateDetails(
            "Updated Title",
            CourseSlug.Create("updated-slug"),
            "Updated description");

        Assert.Equal("Updated Title", course.Title);
        Assert.Equal("updated-slug", course.Slug.Value);
        Assert.Equal("Updated description", course.Description);
    }

    internal static Course CreateDraftCourse(
        string title = "Course Title",
        string slug = "course-title") =>
        Course.CreateDraft(
            Guid.NewGuid(),
            title,
            CourseSlug.Create(slug),
            "Description",
            Guid.NewGuid(),
            DateTime.UtcNow);
}
