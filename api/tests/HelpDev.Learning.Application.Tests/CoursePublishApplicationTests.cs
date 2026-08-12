using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CoursePublishApplicationTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc));
    private readonly CourseService _sut;

    public CoursePublishApplicationTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(_repository, new FakeCourseQueries(), _unitOfWork, _clock);
    }

    [Fact]
    public async Task Publish_valid_course_commits_and_returns_published_dto()
    {
        var course = SeedPublishableCourse();
        course.DequeueDomainEvents();

        var dto = await _sut.PublishAsync(CourseActors.Owner(course.InstructorId), course.Id);

        Assert.Equal(nameof(CourseStatus.Published), dto.Status);
        Assert.Equal(_clock.UtcNow, dto.PublishedAt);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Contains(course.DomainEvents, e => e is CoursePublishedDomainEvent);
    }

    [Fact]
    public async Task Publish_empty_course_wraps_domain_failure()
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Empty",
            CourseSlug.Create("empty-course"),
            "Desc",
            Guid.NewGuid(),
            _clock.UtcNow);
        _repository.Seed(course);

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.PublishAsync(CourseActors.Owner(course.InstructorId), course.Id));

        Assert.Equal(CourseErrorCodes.OperationInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Publish_already_published_is_idempotent()
    {
        var course = SeedPublishableCourse();
        course.Publish(_clock.UtcNow);
        course.DequeueDomainEvents();

        var dto = await _sut.PublishAsync(CourseActors.Owner(course.InstructorId), course.Id);

        Assert.Equal(nameof(CourseStatus.Published), dto.Status);
        Assert.Empty(course.DomainEvents);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    private Course SeedPublishableCourse()
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create("publish-course"),
            "Desc",
            Guid.NewGuid(),
            _clock.UtcNow);
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");
        course.AddLesson(sectionId, Guid.NewGuid(), "Lesson");
        _repository.Seed(course);
        return course;
    }
}
