using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseSectionApplicationTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CourseService _sut;

    public CourseSectionApplicationTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(
            _repository,
            new FakeCourseQueries(),
            _unitOfWork,
            new FakeDateTimeProvider(DateTime.UtcNow));
    }

    [Fact]
    public async Task AddSection_appends_section_through_aggregate()
    {
        var course = SeedCourse();

        var dto = await _sut.AddSectionAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new AddSectionRequest { Title = "Intro" });

        Assert.Single(dto.Sections);
        Assert.Equal("Intro", dto.Sections[0].Title);
        Assert.Equal(1, dto.Sections[0].Order);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task RenameSection_updates_title()
    {
        var course = SeedCourse();
        var section = course.AddSection(Guid.NewGuid(), "Old");

        var dto = await _sut.RenameSectionAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new RenameSectionRequest { SectionId = section.Id, Title = "New" });

        Assert.Equal("New", dto.Sections.Single().Title);
    }

    [Fact]
    public async Task ReorderSection_reorders_through_aggregate()
    {
        var course = SeedCourse();
        var first = course.AddSection(Guid.NewGuid(), "A");
        var second = course.AddSection(Guid.NewGuid(), "B");

        var dto = await _sut.ReorderSectionAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new ReorderSectionRequest { SectionId = second.Id, NewOrder = 1 });

        Assert.Equal(new[] { second.Id, first.Id }, dto.Sections.Select(s => s.Id));
    }

    [Fact]
    public async Task RenameSection_wraps_missing_section()
    {
        var course = SeedCourse();

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.RenameSectionAsync(
                CourseActors.Owner(course.InstructorId),
                course.Id,
                new RenameSectionRequest { SectionId = Guid.NewGuid(), Title = "X" }));

        Assert.Equal(CourseErrorCodes.OperationInvalid, ex.Code);
    }

    private Course SeedCourse()
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create("section-course"),
            "Desc",
            Guid.NewGuid(),
            DateTime.UtcNow);
        _repository.Seed(course);
        return course;
    }
}
