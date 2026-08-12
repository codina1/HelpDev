using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Infrastructure.Outbox;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Workflow;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Identity.Application.Auth.Dtos;
using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Application.Courses.Dtos;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Enrollments.Dtos;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.PostgreSQL;
using HelpDev.Testing.PostgreSQL.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Certification;

/// <summary>
/// Sprint 46 — production platform certification journey on real PostgreSQL.
/// Covers Identity → Profile → Content → Search → Learning → AI → Analytics → Audit.
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "ProductionCertification")]
public sealed class ProductionPlatformCertificationE2ETests : IntegrationTestClassBase
{
    private readonly string _mediaRoot =
        Path.Combine(Path.GetTempPath(), "helpdev-s46-media-" + Guid.NewGuid().ToString("N"));

    public ProductionPlatformCertificationE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    protected override IReadOnlyDictionary<string, string?>? ConfigurationOverrides
    {
        get
        {
            Directory.CreateDirectory(_mediaRoot);
            return new Dictionary<string, string?>
            {
                ["Media:LocalStorageRoot"] = _mediaRoot,
            };
        }
    }

    [PostgreSqlFact]
    public async Task Certifies_complete_production_platform_journey()
    {
        // Identity — register + OTP verify + JWT
        var mobile = $"+98912{Random.Shared.Next(1000000, 9999999)}";
        var sendResponse = await Client.PostAsJsonAsync("/api/v1/auth/send-otp", new SendOtpRequest { Mobile = mobile });
        Assert.Equal(HttpStatusCode.OK, sendResponse.StatusCode);
        var sendPayload = await sendResponse.Content.ReadFromJsonAsync<SendOtpResponse>();
        Assert.False(string.IsNullOrWhiteSpace(sendPayload?.Otp));

        var verifyResponse = await Client.PostAsJsonAsync("/api/v1/auth/verify-otp", new VerifyOtpRequest
        {
            Mobile = mobile,
            Code = sendPayload!.Otp!,
        });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        using var authDoc = JsonDocument.Parse(await verifyResponse.Content.ReadAsStringAsync());
        var accessToken = authDoc.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        var learnerId = await ResolveUserIdByMobileAsync(mobile);
        var adminId = await SeedAdminAsync();
        var writerId = await SeedWriterAsync();

        // Profile — learning preferences
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var profiles = scope.ServiceProvider.GetRequiredService<ILearningProfileService>();
            var profile = await profiles.UpsertAsync(
                learnerId,
                new UpdateLearningProfileRequest(
                    "Intermediate",
                    "Become AI Developer",
                    "C#, ASP.NET",
                    [
                        new LearningPreferenceDto(".NET", 1, 5),
                        new LearningPreferenceDto("AI", 2, 5),
                        new LearningPreferenceDto("Architecture", 3, 4),
                    ]));
            Assert.Equal("Become AI Developer", profile.LearningGoals);
            Assert.Equal(3, profile.PreferredTopics.Count);
        }

        // Content — create, update, SEO, media, workflow, publish
        Guid contentId;
        string coverUrl;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var writerActor = new ContentManagementActor(writerId, canManageAllContent: false);
            var created = await content.CreateAsync(
                writerId,
                new CreateContentRequest
                {
                    Title = "Production Certification Article",
                    Slug = $"prod-cert-{Guid.NewGuid():N}"[..40],
                    Body = "HelpDev production certification uses PostgreSQL and outbox-driven search.",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                });
            contentId = created.Id;

            var png = HelpDev.Media.Tests.ImageTestFixtures.CreatePngBytes();
            var media = scope.ServiceProvider.GetRequiredService<IMediaAssetService>();
            await using var stream = new MemoryStream(png);
            var uploaded = await media.UploadAsync(
                new MediaManagementActor(writerId, canManageAllAssets: false),
                new UploadMediaAssetRequest
                {
                    Content = stream,
                    OriginalFileName = "cert-cover.png",
                    DeclaredContentType = "image/png",
                    SizeBytes = png.Length,
                    AltText = "Certification cover",
                });
            coverUrl = uploaded.PublicUrl;

            await content.UpdateAsync(
                writerActor,
                contentId,
                new UpdateContentRequest
                {
                    Title = "Production Certification Article",
                    Slug = created.Slug,
                    Type = nameof(ContentType.Article),
                    Body = "HelpDev production certification uses PostgreSQL and outbox-driven search. Updated.",
                    Excerpt = "Production certification excerpt",
                    CoverImage = coverUrl,
                });

            await content.UpdateSeoMetadataAsync(
                writerActor,
                contentId,
                new UpdateSeoMetadataRequest
                {
                    SeoTitle = "Production Certification SEO",
                    SeoDescription = "Release certification SEO metadata",
                    FocusKeyword = "postgresql",
                });
        }

        var workflow = Factory.Services.GetRequiredService<IContentWorkflowService>();
        await workflow.SubmitForReviewAsync(
            new ContentManagementActor(writerId, canManageAllContent: false),
            contentId);
        await workflow.ApproveAsync(
            new ContentManagementActor(adminId, canManageAllContent: true),
            contentId);
        await workflow.PublishAsync(
            new ContentManagementActor(adminId, canManageAllContent: true),
            contentId);

        // Search — outbox → lexical + semantic
        var processor = Factory.Services.GetRequiredService<OutboxProcessor>();
        await processor.ProcessBatchAsync(CancellationToken.None);
        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.SearchDocuments.AnyAsync(
                d => d.SourceType == SearchSourceTypes.Content && d.SourceId == contentId && d.IsPublished));
            Assert.True(await db.SearchChunks.AnyAsync(c => c.SourceId == contentId));
            Assert.True(await db.SearchVectors.AnyAsync());
            Assert.True(await db.OutboxMessages.AnyAsync(
                m => m.Type == "content.published.v1" && m.ProcessedAtUtc != null));
        }

        // Learning — enroll, progress, recommendation, roadmap
        Guid courseId;
        Guid lessonId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var courses = scope.ServiceProvider.GetRequiredService<ICourseService>();
            var actor = new CourseManagementActor(adminId, canManageAllCourses: true);
            var course = await courses.CreateAsync(
                actor,
                new CreateCourseRequest
                {
                    Title = "Production Certification Course",
                    Slug = $"prod-course-{Guid.NewGuid():N}"[..40],
                    Description = "ASP.NET Core certification path",
                });
            courseId = course.Id;
            var withSection = await courses.AddSectionAsync(
                actor,
                courseId,
                new AddSectionRequest { Title = "Intro" });
            var sectionId = withSection.Sections[0].Id;
            var withLesson = await courses.AddLessonAsync(
                actor,
                courseId,
                new AddLessonRequest { SectionId = sectionId, Title = "Lesson 1" });
            lessonId = withLesson.Sections[0].Lessons[0].Id;
            await courses.PublishAsync(actor, courseId);

            var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
            await enrollments.EnrollAsync(new EnrollStudentRequest { CourseId = courseId, UserId = learnerId });
            await enrollments.StartLessonAsync(new StartLessonRequest
            {
                CourseId = courseId,
                UserId = learnerId,
                LessonId = lessonId,
            });
            await enrollments.CompleteLessonAsync(new CompleteLessonRequest
            {
                CourseId = courseId,
                UserId = learnerId,
                LessonId = lessonId,
            });
        }

        await processor.ProcessBatchAsync(CancellationToken.None);

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var enrollmentsBefore = await scope.ServiceProvider
                .GetRequiredService<IEnrollmentService>()
                .ListByUserAsync(learnerId);

            var recommendations = scope.ServiceProvider.GetRequiredService<ILearningRecommendationService>();
            var recs = await recommendations.GetRecommendationsAsync(learnerId);
            Assert.False(string.IsNullOrWhiteSpace(recs.Reason));
            Assert.NotEmpty(recs.NextSteps);

            var roadmaps = scope.ServiceProvider.GetRequiredService<ILearningRoadmapService>();
            var roadmap = await roadmaps.GenerateAsync(
                learnerId,
                new GenerateLearningRoadmapRequest("Become AI Developer"));
            Assert.Equal("Suggested", roadmap.Status);
            var approved = await roadmaps.ApproveAsync(learnerId);
            Assert.Equal("Approved", approved.Status);

            var enrollmentsAfter = await scope.ServiceProvider
                .GetRequiredService<IEnrollmentService>()
                .ListByUserAsync(learnerId);
            Assert.Equal(enrollmentsBefore.Count, enrollmentsAfter.Count);
        }

        // AI — content assistant + workflow draft + usage tracking
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var assistant = scope.ServiceProvider.GetRequiredService<IContentAiAssistantService>();
            var writerActor = new ContentManagementActor(writerId, canManageAllContent: false);
            var analysis = await assistant.AnalyzeContentAsync(writerActor, contentId);
            Assert.False(string.IsNullOrWhiteSpace(analysis.GeneratedText));
            Assert.Equal("ContentAnalysis", analysis.TaskType);

            var aiWorkflow = scope.ServiceProvider.GetRequiredService<IAiContentWorkflowService>();
            var session = await aiWorkflow.CreateAsync(
                writerActor,
                new CreateAiContentWorkflowRequest("AI Certification Draft", "Validation", "Article"));
            var research = await aiWorkflow.ResearchAsync(writerActor, session.Id);
            var outline = await aiWorkflow.GenerateOutlineAsync(
                writerActor,
                session.Id,
                new GenerateOutlineRequest(research.Summary));
            var draft = await aiWorkflow.GenerateDraftAsync(
                writerActor,
                session.Id,
                new GenerateDraftRequest(outline.Title, outline.RawText));
            var applied = await aiWorkflow.ApplyDraftAsync(
                writerActor,
                session.Id,
                new ApplyDraftRequest(draft.Title, draft.BodyMarkdown, $"ai-cert-{Guid.NewGuid():N}"[..36], "Article"));
            Assert.True(applied.RevisionVersion >= 1);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var rag = scope.ServiceProvider.GetRequiredService<IRagAnswerService>();
            var answer = await rag.AskAsync("How does HelpDev use PostgreSQL?");
            Assert.False(string.IsNullOrWhiteSpace(answer.Answer));
        }

        // Analytics + Audit + schema evidence
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.AnalyticsEventReceipts.AnyAsync());
            Assert.True(await db.AiUsageRecords.AnyAsync(r =>
                r.TaskType == AiOperationNames.ContentAssistant
                || r.TaskType == AiOperationNames.LearningRecommend
                || r.TaskType == AiOperationNames.LearningRoadmap
                || r.TaskType == AiOperationNames.WorkflowResearch
                || r.TaskType == AiOperationNames.RagAnswer));
            Assert.True(await db.AiUsageRecords.AnyAsync(r => r.TaskType == AiOperationNames.ContentAssistant));
            Assert.True(await db.AuditRecords.AnyAsync(r =>
                r.Action == AuditActions.AuthenticationOtpRequested
                || r.Action == AuditActions.AuthenticationOtpVerified
                || r.Action == AuditActions.LearningRecommendationRequested
                || r.Action == AuditActions.LearningRoadmapGenerated
                || r.Action == AuditActions.ContentAiTaskRequested));

            var migrations = await db.Database.GetAppliedMigrationsAsync();
            Assert.Equal(PostgreSqlDatabaseHelper.ExpectedMigrationCount, migrations.Count());

            var tables = await PostgreSqlDatabaseHelper.GetExistingModuleTablesAsync(ConnectionString);
            Assert.Equal(PostgreSqlDatabaseHelper.ExpectedModuleTables.Count, tables.Count);
        }

        SensitiveLogAssertionHelper.AssertSentinelsAbsent(
            CapturedLogs,
            "sk-",
            "Bearer ",
            "ApiKey",
            accessToken!,
            sendPayload.Otp!);
    }

    private async Task<Guid> ResolveUserIdByMobileAsync(string mobile)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var normalized = mobile.StartsWith("+98", StringComparison.Ordinal)
            ? "0" + mobile[3..]
            : mobile;
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Mobile == normalized || u.Mobile == mobile);
        return user.Id;
    }

    private async Task<Guid> SeedAdminAsync()
    {
        var (_, userId) = await AuthClients.CreateAdminClientWithIdAsync();
        return userId;
    }

    private async Task<Guid> SeedWriterAsync()
    {
        var (_, userId) = await AuthClients.CreateWriterClientWithIdAsync();
        return userId;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_mediaRoot))
        {
            try
            {
                Directory.Delete(_mediaRoot, recursive: true);
            }
            catch
            {
                // Best-effort cleanup for temp media root.
            }
        }
    }
}
