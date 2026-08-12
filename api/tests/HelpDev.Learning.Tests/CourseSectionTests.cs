using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Learning.Tests;

public sealed class CourseSectionTests
{
    [Fact]
    public void AddSection_appends_with_sequential_order()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        var first = course.AddSection(firstId, "Section A");
        var second = course.AddSection(secondId, "Section B");

        Assert.Equal(2, course.Sections.Count);
        Assert.Equal(1, first.Order);
        Assert.Equal(2, second.Order);
        Assert.Equal("Section A", first.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSection_rejects_empty_title(string? title)
    {
        var course = CourseCreationTests.CreateDraftCourse();

        Assert.Throws<DomainException>(() => course.AddSection(Guid.NewGuid(), title!));
    }

    [Fact]
    public void RenameSection_updates_title()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Old Title");

        course.RenameSection(sectionId, "  New Title  ");

        Assert.Equal("New Title", course.Sections.Single().Title);
    }

    [Fact]
    public void RenameSection_throws_when_section_missing()
    {
        var course = CourseCreationTests.CreateDraftCourse();

        Assert.Throws<DomainException>(() =>
            course.RenameSection(Guid.NewGuid(), "Anything"));
    }

    [Fact]
    public void ReorderSection_keeps_orders_unique_and_contiguous()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        course.AddSection(a, "A");
        course.AddSection(b, "B");
        course.AddSection(c, "C");

        course.ReorderSection(c, 1);

        Assert.Equal(new[] { c, a, b }, course.Sections.Select(s => s.Id));
        Assert.Equal(new[] { 1, 2, 3 }, course.Sections.Select(s => s.Order));
    }

    [Fact]
    public void ReorderSection_rejects_invalid_order()
    {
        var course = CourseCreationTests.CreateDraftCourse();
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Only");

        Assert.Throws<DomainException>(() => course.ReorderSection(sectionId, 0));
        Assert.Throws<DomainException>(() => course.ReorderSection(sectionId, 2));
    }
}
