using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseOwnershipApplicationTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc));
    private readonly CourseService _sut;

    public CourseOwnershipApplicationTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(_repository, new FakeCourseQueries(), _unitOfWork, _clock);
    }

    [Fact]
    public void EnsureCanManage_allows_owner_and_admin_rejects_non_owner()
    {
        var course = SeedOwnedCourse(Guid.NewGuid());

        CourseService.EnsureCanManage(course, CourseActors.Owner(course.InstructorId));
        CourseService.EnsureCanManage(course, CourseActors.Admin(Guid.NewGuid()));

        var ex = Assert.Throws<CourseException>(() =>
            CourseService.EnsureCanManage(course, CourseActors.Owner(Guid.NewGuid())));

        Assert.Equal(CourseErrorCodes.NotFound, ex.Code);
    }

    [Fact]
    public async Task Owner_writer_can_update_publish_section_and_lesson()
    {
        var course = SeedPublishableCourse();
        var actor = CourseActors.Owner(course.InstructorId);

        await _sut.UpdateDetailsAsync(
            actor,
            course.Id,
            new UpdateCourseRequest
            {
                Title = "Owned",
                Slug = "owned-course",
                Description = "Desc",
            });
        await _sut.AddSectionAsync(actor, course.Id, new AddSectionRequest { Title = "Extra" });
        var extraSectionId = course.Sections.Last().Id;
        await _sut.AddLessonAsync(
            actor,
            course.Id,
            new AddLessonRequest { SectionId = extraSectionId, Title = "Extra lesson" });
        await _sut.PublishAsync(actor, course.Id);

        Assert.Equal(4, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Non_owner_writer_receives_not_found_and_does_not_commit_or_mutate()
    {
        var course = SeedPublishableCourse();
        var originalTitle = course.Title;
        var originalSectionCount = course.Sections.Count;
        var stranger = CourseActors.Owner(Guid.NewGuid());

        var updateEx = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.UpdateDetailsAsync(
                stranger,
                course.Id,
                new UpdateCourseRequest
                {
                    Title = "Hacked",
                    Slug = "hacked",
                    Description = "Nope",
                }));
        Assert.Equal(CourseErrorCodes.NotFound, updateEx.Code);

        var publishEx = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.PublishAsync(stranger, course.Id));
        Assert.Equal(CourseErrorCodes.NotFound, publishEx.Code);

        var sectionEx = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.AddSectionAsync(stranger, course.Id, new AddSectionRequest { Title = "X" }));
        Assert.Equal(CourseErrorCodes.NotFound, sectionEx.Code);

        var lessonEx = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.AddLessonAsync(
                stranger,
                course.Id,
                new AddLessonRequest
                {
                    SectionId = course.Sections.Single().Id,
                    Title = "X",
                }));
        Assert.Equal(CourseErrorCodes.NotFound, lessonEx.Code);

        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(originalTitle, course.Title);
        Assert.Equal(originalSectionCount, course.Sections.Count);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Empty(course.DomainEvents.OfType<CourseUpdatedDomainEvent>());
    }

    [Fact]
    public async Task Non_owner_cannot_update_published_course_or_raise_CourseUpdatedDomainEvent()
    {
        var course = SeedPublishableCourse();
        course.Publish(_clock.UtcNow);
        course.DequeueDomainEvents();
        var originalTitle = course.Title;
        var stranger = CourseActors.Owner(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.UpdateDetailsAsync(
                stranger,
                course.Id,
                new UpdateCourseRequest
                {
                    Title = "Hacked",
                    Slug = "hacked",
                    Description = "Nope",
                }));

        Assert.Equal(CourseErrorCodes.NotFound, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(originalTitle, course.Title);
        Assert.Empty(course.DomainEvents);
    }

    [Fact]
    public async Task Admin_can_manage_another_instructors_course()
    {
        var course = SeedPublishableCourse();
        var admin = CourseActors.Admin(Guid.NewGuid());

        var dto = await _sut.UpdateDetailsAsync(
            admin,
            course.Id,
            new UpdateCourseRequest
            {
                Title = "Admin Updated",
                Slug = "admin-updated",
                Description = "Desc",
            });

        Assert.Equal("Admin Updated", dto.Title);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        await _sut.PublishAsync(admin, course.Id);
        Assert.Equal(2, _unitOfWork.SaveChangesCount);
        Assert.Equal(CourseStatus.Published, course.Status);
    }

    [Fact]
    public async Task GetById_hides_cross_owner_course()
    {
        var course = SeedOwnedCourse(Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.GetByIdAsync(CourseActors.Owner(Guid.NewGuid()), course.Id));

        Assert.Equal(CourseErrorCodes.NotFound, ex.Code);
    }

    private Course SeedOwnedCourse(Guid instructorId)
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create($"course-{Guid.NewGuid():N}"[..20]),
            "Desc",
            instructorId,
            _clock.UtcNow);
        _repository.Seed(course);
        return course;
    }

    private Course SeedPublishableCourse()
    {
        var course = SeedOwnedCourse(Guid.NewGuid());
        var sectionId = Guid.NewGuid();
        course.AddSection(sectionId, "Section");
        course.AddLesson(sectionId, Guid.NewGuid(), "Lesson");
        return course;
    }
}
