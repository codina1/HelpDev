using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseUpdateTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeCourseQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
    private readonly CourseService _sut;

    public CourseUpdateTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task UpdateDetails_updates_fields_and_commits_once()
    {
        var course = SeedCourse("old-slug");

        var dto = await _sut.UpdateDetailsAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new UpdateCourseRequest
            {
                Title = "Updated Title",
                Slug = "new-slug",
                Description = "Updated description",
            });

        Assert.Equal("Updated Title", dto.Title);
        Assert.Equal("new-slug", dto.Slug);
        Assert.Equal("Updated description", dto.Description);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.DoesNotContain(course.DomainEvents, e => e is CourseUpdatedDomainEvent);
    }

    [Fact]
    public async Task UpdateDetails_on_published_course_raises_CourseUpdatedDomainEvent_and_commits_once()
    {
        var course = SeedPublishedCourse("published-slug");
        course.DequeueDomainEvents();

        await _sut.UpdateDetailsAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new UpdateCourseRequest
            {
                Title = "Published Updated",
                Slug = "published-updated",
                Description = "New summary",
            });

        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        var updated = Assert.Single(course.DomainEvents.OfType<CourseUpdatedDomainEvent>());
        Assert.Equal(course.Id, updated.CourseId);
    }

    [Fact]
    public async Task UpdateDetails_invalid_title_does_not_commit_or_raise_event()
    {
        var course = SeedPublishedCourse("valid-slug");
        course.DequeueDomainEvents();

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.UpdateDetailsAsync(
                CourseActors.Owner(course.InstructorId),
                course.Id,
                new UpdateCourseRequest
                {
                    Title = " ",
                    Slug = "valid-slug",
                    Description = "Desc",
                }));

        Assert.Equal(CourseErrorCodes.OperationInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Empty(course.DomainEvents);
    }

    [Fact]
    public async Task UpdateDetails_rejects_duplicate_changed_slug()
    {
        var course = SeedCourse("course-a");
        SeedCourse("course-b");

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.UpdateDetailsAsync(
                CourseActors.Owner(course.InstructorId),
                course.Id,
                new UpdateCourseRequest
                {
                    Title = "Title",
                    Slug = "course-b",
                    Description = "Desc",
                }));

        Assert.Equal(CourseErrorCodes.SlugDuplicate, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Empty(course.DomainEvents.OfType<CourseUpdatedDomainEvent>());
    }

    [Fact]
    public async Task UpdateDetails_allows_unchanged_slug_for_same_course()
    {
        var course = SeedCourse("same-slug");

        var dto = await _sut.UpdateDetailsAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new UpdateCourseRequest
            {
                Title = "Retitled",
                Slug = "same-slug",
                Description = "Desc",
            });

        Assert.Equal("Retitled", dto.Title);
        Assert.Equal("same-slug", dto.Slug);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
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

    private Course SeedPublishedCourse(string slug)
    {
        var course = SeedCourse(slug);
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");
        course.AddLesson(sectionId, Guid.NewGuid(), "Lesson");
        course.Publish(_clock.UtcNow);
        return course;
    }
}
