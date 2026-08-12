using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseLessonApplicationTests
{
    private readonly FakeCourseRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly CourseService _sut;

    public CourseLessonApplicationTests()
    {
        _sut = LearningServiceFactory.CreateCourseService(
            _repository,
            new FakeCourseQueries(),
            _unitOfWork,
            new FakeDateTimeProvider(DateTime.UtcNow));
    }

    [Fact]
    public async Task AddLesson_adds_through_aggregate()
    {
        var (course, sectionId) = SeedCourseWithSection();

        var dto = await _sut.AddLessonAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new AddLessonRequest
            {
                SectionId = sectionId,
                Title = "Lesson 1",
                ContentId = Guid.NewGuid(),
                VideoUrl = "https://cdn.example/v.mp4",
                DurationMinutes = 15,
                IsPreview = true,
            });

        var lesson = Assert.Single(dto.Sections.Single().Lessons);
        Assert.Equal("Lesson 1", lesson.Title);
        Assert.Equal(15, lesson.DurationMinutes);
        Assert.True(lesson.IsPreview);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateLesson_updates_fields()
    {
        var (course, sectionId) = SeedCourseWithSection();
        var lesson = course.AddLesson(sectionId, Guid.NewGuid(), "Old");

        var dto = await _sut.UpdateLessonAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new UpdateLessonRequest
            {
                SectionId = sectionId,
                LessonId = lesson.Id,
                Title = "New",
                DurationMinutes = 20,
                IsPreview = true,
            });

        Assert.Equal("New", dto.Sections.Single().Lessons.Single().Title);
        Assert.Equal(20, dto.Sections.Single().Lessons.Single().DurationMinutes);
    }

    [Fact]
    public async Task ReorderLesson_reorders_through_aggregate()
    {
        var (course, sectionId) = SeedCourseWithSection();
        var a = course.AddLesson(sectionId, Guid.NewGuid(), "A");
        var b = course.AddLesson(sectionId, Guid.NewGuid(), "B");

        var dto = await _sut.ReorderLessonAsync(
            CourseActors.Owner(course.InstructorId),
            course.Id,
            new ReorderLessonRequest
            {
                SectionId = sectionId,
                LessonId = b.Id,
                NewOrder = 1,
            });

        Assert.Equal(new[] { b.Id, a.Id }, dto.Sections.Single().Lessons.Select(l => l.Id));
    }

    [Fact]
    public async Task AddLesson_wraps_missing_section()
    {
        var course = SeedCourse();

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.AddLessonAsync(
                CourseActors.Owner(course.InstructorId),
                course.Id,
                new AddLessonRequest { SectionId = Guid.NewGuid(), Title = "Lesson" }));

        Assert.Equal(CourseErrorCodes.OperationInvalid, ex.Code);
    }

    [Fact]
    public async Task UpdateLesson_wraps_missing_lesson()
    {
        var (course, sectionId) = SeedCourseWithSection();

        var ex = await Assert.ThrowsAsync<CourseException>(() =>
            _sut.UpdateLessonAsync(
                CourseActors.Owner(course.InstructorId),
                course.Id,
                new UpdateLessonRequest
                {
                    SectionId = sectionId,
                    LessonId = Guid.NewGuid(),
                    Title = "X",
                }));

        Assert.Equal(CourseErrorCodes.OperationInvalid, ex.Code);
    }

    private Course SeedCourse()
    {
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create("lesson-course"),
            "Desc",
            Guid.NewGuid(),
            DateTime.UtcNow);
        _repository.Seed(course);
        return course;
    }

    private (Course Course, Guid SectionId) SeedCourseWithSection()
    {
        var course = SeedCourse();
        var section = course.AddSection(Guid.NewGuid(), "Section");
        return (course, section.Id);
    }
}
