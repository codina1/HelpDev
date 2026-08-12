using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseLookupTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeCourseQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
    private readonly CourseService _sut;

    public CourseLookupTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task GetById_returns_existing_course_for_owner()
    {
        var course = SeedCourse("lookup-course");

        var dto = await _sut.GetByIdAsync(CourseActors.Owner(course.InstructorId), course.Id);

        Assert.Equal(course.Id, dto.Id);
        Assert.Equal("lookup-course", dto.Slug);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task GetById_throws_when_missing()
    {
        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.GetByIdAsync(CourseActors.Owner(Guid.NewGuid()), Guid.NewGuid()));

        Assert.Equal(CourseErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task GetBySlug_returns_existing_course()
    {
        SeedCourse("by-slug");

        var dto = await _sut.GetBySlugAsync("  By-Slug  ");

        Assert.Equal("by-slug", dto.Slug);
    }

    [Fact]
    public async Task GetBySlug_rejects_invalid_slug()
    {
        var ex = await Assert.ThrowsAsync<CourseException>(() => _sut.GetBySlugAsync("nope!"));

        Assert.Equal(CourseErrorCodes.SlugInvalid, ex.Code);
    }

    private Course SeedCourse(string slug)
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create(slug),
            "Description",
            Guid.NewGuid(),
            _clock.UtcNow);
        _repository.Seed(course);
        return course;
    }
}
