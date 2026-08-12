using HelpDev.Learning.Enrollment.Application.Tests.Fakes;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Domain.Enrollments;
using EnrollmentEntity = HelpDev.Modules.Learning.Domain.Enrollments.Enrollment;

namespace HelpDev.Learning.Enrollment.Application.Tests;

public sealed class EnrollmentListingAndMappingTests
{
    [Fact]
    public async Task ListByUser_delegates_to_queries_with_user_id()
    {
        var userId = Guid.NewGuid();
        var queries = new FakeEnrollmentQueries
        {
            ItemsToReturn =
            [
                new EnrollmentListItemDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    userId,
                    DateTime.UtcNow,
                    nameof(EnrollmentStatus.Active),
                    0),
            ],
        };
        var sut = CreateService(queries: queries);

        var result = await sut.ListByUserAsync(userId);

        Assert.Equal(1, queries.ListCallCount);
        Assert.Equal(userId, queries.LastUserId);
        Assert.Same(queries.ItemsToReturn, result);
    }

    [Fact]
    public async Task ListByUser_rejects_empty_user_id()
    {
        var sut = CreateService();

        var ex = await Assert.ThrowsAsync<EnrollmentException>(() => sut.ListByUserAsync(Guid.Empty));

        Assert.Equal(EnrollmentErrorCodes.UserInvalid, ex.Code);
    }

    [Fact]
    public void Mapper_maps_progress_fields_without_domain_types()
    {
        var enrollment = EnrollmentEntity.Enroll(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTime(2026, 6, 4, 8, 0, 0, DateTimeKind.Utc));
        enrollment.DequeueDomainEvents();
        enrollment.StartLesson(Guid.NewGuid(), enrollment.EnrolledAt.AddMinutes(1));

        var dto = EnrollmentMapper.ToDto(enrollment);

        Assert.Equal(nameof(EnrollmentStatus.Active), dto.Status);
        Assert.Equal(0, dto.ProgressPercentage);
        Assert.All(typeof(EnrollmentDto).GetProperties(), property =>
            Assert.False(property.PropertyType.Namespace?.Contains(".Domain.") == true));
        Assert.All(typeof(LessonProgressDto).GetProperties(), property =>
            Assert.False(property.PropertyType.Namespace?.Contains(".Domain.") == true));

        var progress = Assert.Single(dto.LessonProgress);
        Assert.NotNull(progress.StartedAt);
        Assert.Null(progress.CompletedAt);
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public async Task Repository_AddAsync_does_not_commit_independently()
    {
        var repository = new FakeEnrollmentRepository();
        var unitOfWork = new FakeUnitOfWork();

        await repository.AddAsync(
            EnrollmentEntity.Enroll(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow));

        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(0, repository.IndependentSaveCount);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    private static EnrollmentService CreateService(
        FakeEnrollmentRepository? repository = null,
        FakeEnrollmentQueries? queries = null) =>
        new(
            repository ?? new FakeEnrollmentRepository(),
            queries ?? new FakeEnrollmentQueries(),
            new FakeCourseLearningQueries(),
            new FakeUnitOfWork(),
            new FakeDateTimeProvider(DateTime.UtcNow));
}
