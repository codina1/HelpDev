using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.SharedContracts.Ai;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Learning;

[Collection(PostgreSqlCollection.Name)]
public sealed class PersonalizedLearningE2ETests : IntegrationTestClassBase
{
    public PersonalizedLearningE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Profile_enroll_complete_recommend_and_roadmap()
    {
        var userId = await SeedUserAsync();
        Guid courseId;
        Guid sectionId;
        Guid lessonId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var courses = scope.ServiceProvider.GetRequiredService<ICourseService>();
            var actor = new CourseManagementActor(userId, canManageAllCourses: true);
            var course = await courses.CreateAsync(
                actor,
                new CreateCourseRequest
                {
                    Title = "ASP.NET Core Advanced",
                    Slug = "aspnet-core-advanced-ai",
                    Description = "Learn ASP.NET Core and AI APIs for HelpDev developers.",
                },
                CancellationToken.None);
            courseId = course.Id;

            var withSection = await courses.AddSectionAsync(
                actor,
                courseId,
                new AddSectionRequest { Title = "Basics" },
                CancellationToken.None);
            sectionId = withSection.Sections[0].Id;

            var withLesson = await courses.AddLessonAsync(
                actor,
                courseId,
                new AddLessonRequest { SectionId = sectionId, Title = "Intro" },
                CancellationToken.None);
            lessonId = withLesson.Sections[0].Lessons[0].Id;
            await courses.PublishAsync(actor, courseId, CancellationToken.None);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var profiles = scope.ServiceProvider.GetRequiredService<ILearningProfileService>();
            await profiles.UpsertAsync(
                userId,
                new UpdateLearningProfileRequest(
                    "Intermediate",
                    "Become AI Developer",
                    "C#",
                    [new LearningPreferenceDto(".NET", 1, 5), new LearningPreferenceDto("AI", 2, 5)]));

            var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
            await enrollments.EnrollAsync(
                new EnrollStudentRequest { CourseId = courseId, UserId = userId },
                CancellationToken.None);
            await enrollments.StartLessonAsync(
                new StartLessonRequest { CourseId = courseId, UserId = userId, LessonId = lessonId },
                CancellationToken.None);
            await enrollments.CompleteLessonAsync(
                new CompleteLessonRequest { CourseId = courseId, UserId = userId, LessonId = lessonId },
                CancellationToken.None);

            var recommendations = scope.ServiceProvider.GetRequiredService<ILearningRecommendationService>();
            var recs = await recommendations.GetRecommendationsAsync(userId);
            Assert.False(string.IsNullOrWhiteSpace(recs.Reason));
            Assert.NotEmpty(recs.NextSteps);

            var roadmaps = scope.ServiceProvider.GetRequiredService<ILearningRoadmapService>();
            var roadmap = await roadmaps.GenerateAsync(
                userId,
                new GenerateLearningRoadmapRequest("Become AI Developer"));
            Assert.Equal("Suggested", roadmap.Status);
            Assert.NotEmpty(roadmap.Steps);

            var approved = await roadmaps.ApproveAsync(userId);
            Assert.Equal("Approved", approved.Status);

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.LearningProfiles.AnyAsync(p => p.UserId == userId));
            Assert.True(await db.LearningRoadmaps.AnyAsync(r => r.UserId == userId));
            Assert.True(await db.AiUsageRecords.AnyAsync(r =>
                r.TaskType == AiOperationNames.LearningRecommend
                || r.TaskType == AiOperationNames.LearningRoadmap));
        }
    }

    private async Task<Guid> SeedUserAsync()
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "Learning AI User",
            FirstName = "Learning",
            LastName = "AI",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
        return userId;
    }
}
