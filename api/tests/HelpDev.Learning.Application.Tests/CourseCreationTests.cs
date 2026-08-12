using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseCreationTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeCourseQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
    private readonly CourseService _sut;

    public CourseCreationTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_valid_course_persists_once_and_returns_detail_dto()
    {
        var instructorId = Guid.NewGuid();
        var request = new CreateCourseRequest
        {
            Title = "  C# Basics  ",
            Slug = "  Csharp-Basics  ",
            Description = "Intro",
        };

        var dto = await _sut.CreateAsync(CourseActors.Owner(instructorId), request);

        Assert.Equal("C# Basics", dto.Title);
        Assert.Equal("csharp-basics", dto.Slug);
        Assert.Equal("Intro", dto.Description);
        Assert.Equal(instructorId, dto.InstructorId);
        Assert.Equal(nameof(CourseStatus.Draft), dto.Status);
        Assert.Equal(_clock.UtcNow, dto.CreatedAt);
        Assert.Empty(dto.Sections);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(0, _repository.IndependentSaveCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Single(_repository.Courses);
    }

    [Fact]
    public async Task Create_uses_actor_user_id_even_when_admin()
    {
        var instructorId = Guid.NewGuid();

        var dto = await _sut.CreateAsync(
            CourseActors.Admin(instructorId),
            new CreateCourseRequest { Title = "Title", Slug = "admin-create" });

        Assert.Equal(instructorId, dto.InstructorId);
    }

    [Fact]
    public void Create_rejects_empty_actor_user_id()
    {
        Assert.Throws<ArgumentException>(() => CourseActors.Owner(Guid.Empty));
    }

    [Fact]
    public async Task Create_rejects_invalid_slug()
    {
        var request = new CreateCourseRequest
        {
            Title = "Title",
            Slug = "Bad Slug",
        };

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.CreateAsync(CourseActors.Owner(Guid.NewGuid()), request));

        Assert.Equal(CourseErrorCodes.SlugInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Create_rejects_duplicate_slug()
    {
        _repository.Seed(Course.CreateDraft(
            Guid.NewGuid(),
            "Existing",
            CourseSlug.Create("taken-slug"),
            "Desc",
            Guid.NewGuid(),
            _clock.UtcNow));

        var request = new CreateCourseRequest
        {
            Title = "New",
            Slug = "taken-slug",
        };

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.CreateAsync(CourseActors.Owner(Guid.NewGuid()), request));

        Assert.Equal(CourseErrorCodes.SlugDuplicate, ex.Code);
        Assert.Equal(0, _repository.AddCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
