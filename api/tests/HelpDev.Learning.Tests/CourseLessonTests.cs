using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class CourseLessonTests
{
    [Fact]
    public void AddLesson_appends_with_sequential_order_inside_section()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");

        var first = course.AddLesson(sectionId, Guid.NewGuid(), "Lesson 1");
        var second = course.AddLesson(
            sectionId,
            Guid.NewGuid(),
            "Lesson 2",
            contentId: Guid.NewGuid(),
            videoUrl: " https://cdn.example/video.mp4 ",
            durationMinutes: 12,
            isPreview: true);

        Assert.Equal(1, first.Order);
        Assert.Equal(2, second.Order);
        Assert.Equal("Lesson 2", second.Title);
        Assert.NotNull(second.ContentId);
        Assert.Equal("https://cdn.example/video.mp4", second.VideoUrl);
        Assert.Equal(12, second.DurationMinutes);
        Assert.True(second.IsPreview);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddLesson_rejects_empty_title(string? title)
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");

        Assert.Throws<DomainException>(() =>
            course.AddLesson(sectionId, Guid.NewGuid(), title!));
    }

    [Fact]
    public void AddLesson_throws_when_section_missing()
    {
        var course = CourseCreationTests.CreateDraftCourse();

        Assert.Throws<DomainException>(() =>
            course.AddLesson(Guid.NewGuid(), Guid.NewGuid(), "Lesson"));
    }

    [Fact]
    public void UpdateLesson_updates_supported_fields()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");
        course.AddLesson(sectionId, lessonId, "Old");

        course.UpdateLesson(
            sectionId,
            lessonId,
            "New Title",
            contentId: Guid.NewGuid(),
            videoUrl: "https://example.com/v",
            durationMinutes: 30,
            isPreview: true);

        var lesson = course.Sections.Single().Lessons.Single();
        Assert.Equal("New Title", lesson.Title);
        Assert.Equal(30, lesson.DurationMinutes);
        Assert.True(lesson.IsPreview);
    }

    [Fact]
    public void ReorderLesson_keeps_orders_unique_and_contiguous()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        course.AddSection(sectionId, "Section");
        course.AddLesson(sectionId, a, "A");
        course.AddLesson(sectionId, b, "B");
        course.AddLesson(sectionId, c, "C");

        course.ReorderLesson(sectionId, c, 1);

        var lessons = course.Sections.Single().Lessons;
        Assert.Equal(new[] { c, a, b }, lessons.Select(l => l.Id));
        Assert.Equal(new[] { 1, 2, 3 }, lessons.Select(l => l.Order));
    }
}
