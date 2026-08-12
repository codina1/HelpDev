using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

public sealed class CourseQueryApplicationTests
{
    [Fact]
    public async Task List_writer_passes_own_instructor_id_to_queries()
    {
        var queries = new FakeCourseQueries
        {
            ItemsToReturn =
            [
                new CourseListItemDto(
                    Guid.NewGuid(),
                    "Title",
                    "slug",
                    nameof(CourseStatus.Published),
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    SectionCount: 1,
                    LessonCount: 2),
            ],
        };
        var sut = LearningServiceFactory.CreateCourseService(
            new FakeCourseRepository(),
            queries,
            new FakeUnitOfWork(),
            new FakeDateTimeProvider(DateTime.UtcNow));
        var writerId = Guid.NewGuid();

        var result = await sut.ListAsync(CourseActors.Owner(writerId), CourseStatus.Published);

        Assert.Equal(1, queries.ListCallCount);
        Assert.Equal(CourseStatus.Published, queries.LastStatusFilter);
        Assert.Equal(writerId, queries.LastInstructorIdFilter);
        Assert.Same(queries.ItemsToReturn, result);
    }

    [Fact]
    public async Task List_admin_passes_null_instructor_filter()
    {
        var queries = new FakeCourseQueries();
        var sut = LearningServiceFactory.CreateCourseService(
            new FakeCourseRepository(),
            queries,
            new FakeUnitOfWork(),
            new FakeDateTimeProvider(DateTime.UtcNow));

        await sut.ListAsync(CourseActors.Admin(Guid.NewGuid()));

        Assert.Null(queries.LastStatusFilter);
        Assert.Null(queries.LastInstructorIdFilter);
    }
}
