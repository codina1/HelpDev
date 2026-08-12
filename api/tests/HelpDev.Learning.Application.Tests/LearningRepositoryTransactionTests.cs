using HelpDev.Learning.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;

namespace HelpDev.Learning.Application.Tests;

public sealed class LearningRepositoryTransactionTests
{
    [Fact]
    public async Task CourseRepository_AddAsync_does_not_commit_independently()
    {
        var repository = new FakeCourseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var course = Course.CreateDraft(
            Guid.NewGuid(),
            "Title",
            CourseSlug.Create("tx-course"),
            "Desc",
            Guid.NewGuid(),
            DateTime.UtcNow);

        await repository.AddAsync(course);

        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(0, repository.IndependentSaveCount);
        Assert.Equal(0, unitOfWork.SaveChangesCount);

        await unitOfWork.SaveChangesAsync();

        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Equal(0, repository.IndependentSaveCount);
    }

    [Fact]
    public async Task EnrollmentRepository_AddAsync_does_not_commit_independently()
    {
        var repository = new FakeEnrollmentRepository();
        var unitOfWork = new FakeUnitOfWork();

        await repository.AddAsync(
            Enrollment.Enroll(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));

        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(0, repository.IndependentSaveCount);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CourseService_create_commits_only_through_unit_of_work()
    {
        var repository = new FakeCourseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sut = LearningServiceFactory.CreateCourseService(
            repository,
            new FakeCourseQueries(),
            unitOfWork,
            new FakeDateTimeProvider(DateTime.UtcNow));

        await sut.CreateAsync(CourseActors.Owner(Guid.NewGuid()), new CreateCourseRequest
        {
            Title = "Title",
            Slug = "service-tx",
        });

        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(0, repository.IndependentSaveCount);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }
}
