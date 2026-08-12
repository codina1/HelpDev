using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Domain.Entities;
using HelpDev.Modules.Identity.Domain.Enums;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Search.Domain;
using HelpDev.Testing.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Outbox;

/// <summary>
/// Sprint 44 — validates domain event → outbox → processor → handlers without duplicate processing.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Outbox")]
public sealed class OutboxPipelineE2ETests : IntegrationTestClassBase
{
    public OutboxPipelineE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Content_publish_and_update_drive_search_analytics_without_duplicate_processing()
    {
        var authorId = await SeedUserAsync(UserRole.Admin);
        Guid contentId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await content.CreateAsync(
                authorId,
                new CreateContentRequest
                {
                    Title = "Outbox Pipeline Content",
                    Slug = $"outbox-pipeline-{Guid.NewGuid():N}"[..40],
                    Body = "Body for outbox pipeline validation with PostgreSQL.",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                });
            contentId = created.Id;
        }

        var workflow = Factory.Services.GetRequiredService<IContentWorkflowService>();
        var actor = new ContentManagementActor(authorId, canManageAllContent: true);
        await workflow.SubmitForReviewAsync(actor, contentId);
        await workflow.ApproveAsync(actor, contentId);
        await workflow.PublishAsync(actor, contentId);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(1, await db.OutboxMessages.CountAsync(m =>
                m.Type == "content.published.v1" && m.ProcessedAtUtc == null));
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);
        await processor.ProcessBatchAsync(CancellationToken.None); // second pass must not reprocess

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var published = await db.OutboxMessages
                .Where(m => m.Type == "content.published.v1")
                .ToListAsync();
            Assert.Single(published);
            Assert.NotNull(published[0].ProcessedAtUtc);
            Assert.Null(published[0].Error);

            Assert.True(await db.SearchDocuments.AnyAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId && d.IsPublished));
            Assert.True(await db.SearchChunks.AnyAsync(c => c.SourceId == contentId));
            Assert.True(await db.AnalyticsEventReceipts.AnyAsync());
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var detail = await content.GetPublishedBySlugAsync(
                (await content.ListPublishedAsync()).Single(c => c.Id == contentId).Slug);
            await content.UpdateAsync(
                actor,
                contentId,
                new UpdateContentRequest
                {
                    Title = detail.Title + " Updated",
                    Slug = detail.Slug,
                    Type = nameof(ContentType.Article),
                    Body = detail.Body + " Updated for outbox.",
                    Excerpt = "updated",
                });
        }

        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updated = await db.OutboxMessages
                .Where(m => m.Type == "content.updated.v1")
                .ToListAsync();
            Assert.NotEmpty(updated);
            Assert.All(updated, m => Assert.NotNull(m.ProcessedAtUtc));

            var doc = await db.SearchDocuments.SingleAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId);
            Assert.Contains("Updated", doc.Title, StringComparison.Ordinal);
        }
    }

    [PostgreSqlFact]
    public async Task Learning_enrollment_events_are_processed_exactly_once()
    {
        var userId = await SeedUserAsync(UserRole.Admin);
        Guid courseId;
        Guid lessonId;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var courses = scope.ServiceProvider.GetRequiredService<ICourseService>();
            var actor = new CourseManagementActor(userId, canManageAllCourses: true);
            var course = await courses.CreateAsync(
                actor,
                new CreateCourseRequest
                {
                    Title = "Outbox Learning Course",
                    Slug = $"outbox-learning-{Guid.NewGuid():N}"[..40],
                    Description = "Course progress outbox validation",
                });
            courseId = course.Id;
            var sectioned = await courses.AddSectionAsync(actor, courseId, new AddSectionRequest { Title = "S1" });
            var lessoned = await courses.AddLessonAsync(
                actor,
                courseId,
                new AddLessonRequest { SectionId = sectioned.Sections[0].Id, Title = "L1" });
            lessonId = lessoned.Sections[0].Lessons[0].Id;
            await courses.PublishAsync(actor, courseId);

            var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
            await enrollments.EnrollAsync(new EnrollStudentRequest { CourseId = courseId, UserId = userId });
            await enrollments.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = lessonId,
            });
            await enrollments.CompleteLessonAsync(new CompleteLessonRequest
            {
                CourseId = courseId,
                UserId = userId,
                LessonId = lessonId,
            });
        }

        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enrolled = await db.OutboxMessages
                .Where(m => m.Type == "learning.student-enrolled.v1")
                .ToListAsync();
            Assert.Single(enrolled);
            Assert.NotNull(enrolled[0].ProcessedAtUtc);

            var completed = await db.OutboxMessages
                .Where(m => m.Type == "learning.lesson-completed.v1")
                .ToListAsync();
            Assert.Single(completed);
            Assert.NotNull(completed[0].ProcessedAtUtc);
            Assert.True(await db.AnalyticsEventReceipts.AnyAsync());
        }
    }

    private async Task<Guid> SeedUserAsync(UserRole role)
    {
        var userId = Guid.NewGuid();
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Users.Add(new User
        {
            Id = userId,
            Mobile = TestIds.Truncate($"09{Guid.NewGuid():N}", 11),
            FullName = "Outbox User",
            FirstName = "Outbox",
            LastName = "User",
            Role = role,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        return userId;
    }
}
