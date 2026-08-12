using System.Net;
using System.Net.Http.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Learning.Application.Enrollments;
using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Testing.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Security;

/// <summary>
/// Sprint 44 — automated security matrix across Anonymous / User / Writer / Admin (+ AI non-mutation).
/// </summary>
[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class SecurityMatrixE2ETests : IntegrationTestClassBase
{
    public SecurityMatrixE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    public static IEnumerable<object[]> RoleMatrix()
    {
        // role, method, path, expected
        yield return ["Anonymous", "GET", "/api/v1/profile/me", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/profile/me", HttpStatusCode.OK];
        yield return ["Writer", "GET", "/api/v1/profile/me", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/v1/profile/me", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/v1/admin/audit", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/admin/audit", HttpStatusCode.Forbidden];
        yield return ["Writer", "GET", "/api/v1/admin/audit", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/v1/admin/audit", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/v1/admin/features", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/admin/features", HttpStatusCode.Forbidden];
        yield return ["Writer", "GET", "/api/v1/admin/features", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/v1/admin/features", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/v1/admin/learning/personalization", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/admin/learning/personalization", HttpStatusCode.Forbidden];
        yield return ["Writer", "GET", "/api/v1/admin/learning/personalization", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/v1/admin/learning/personalization", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/v1/me/learning-profile", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/me/learning-profile", HttpStatusCode.OK];
        yield return ["Writer", "GET", "/api/v1/me/learning-profile", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/v1/me/learning-profile", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/v1/admin/content", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/v1/admin/content", HttpStatusCode.Forbidden];
        yield return ["Writer", "GET", "/api/v1/admin/content", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/v1/admin/content", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/health/live", HttpStatusCode.OK];
        yield return ["User", "GET", "/api/v1/search?q=helpdev", HttpStatusCode.OK];
        yield return ["Writer", "GET", "/api/v1/content", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/v1/content", HttpStatusCode.OK];
    }

    [Theory]
    [MemberData(nameof(RoleMatrix))]
    public async Task Role_matrix_returns_expected_status(
        string role,
        string method,
        string path,
        HttpStatusCode expected)
    {
        using var client = role switch
        {
            "Anonymous" => AuthClients.CreateAnonymousClient(),
            "User" => await AuthClients.CreateUserClientAsync(),
            "Writer" => await AuthClients.CreateWriterClientAsync(),
            "Admin" => await AuthClients.CreateAdminClientAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };

        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Writer_cannot_read_or_update_another_writers_content()
    {
        var (writerA, writerAId) = await AuthClients.CreateWriterClientWithIdAsync();
        var (writerB, writerBId) = await AuthClients.CreateWriterClientWithIdAsync();

        Guid contentId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var created = await content.CreateAsync(
                writerAId,
                new CreateContentRequest
                {
                    Title = "Writer A Private Draft",
                    Slug = $"writer-a-{Guid.NewGuid():N}"[..36],
                    Body = "Owned by writer A",
                    Type = nameof(ContentType.Article),
                    Status = nameof(ContentStatus.Draft),
                });
            contentId = created.Id;
        }

        using (writerA)
        using (writerB)
        {
            using var ownerRead = await writerA.GetAsync($"/api/v1/admin/content/{contentId}");
            Assert.Equal(HttpStatusCode.OK, ownerRead.StatusCode);

            using var otherRead = await writerB.GetAsync($"/api/v1/admin/content/{contentId}");
            Assert.Equal(HttpStatusCode.NotFound, otherRead.StatusCode);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var content = scope.ServiceProvider.GetRequiredService<IContentService>();
            var ex = await Assert.ThrowsAsync<ContentException>(() =>
                content.UpdateAsync(
                    new ContentManagementActor(writerBId, canManageAllContent: false),
                    contentId,
                    new UpdateContentRequest
                    {
                        Title = "Hijacked",
                        Slug = $"hijacked-{Guid.NewGuid():N}"[..36],
                        Type = nameof(ContentType.Article),
                        Body = "should fail",
                    }));
            Assert.Equal(ContentErrorCodes.NotFound, ex.Code);
        }
    }

    [PostgreSqlFact]
    public async Task Admin_has_full_management_access_to_features()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.matrix.{Guid.NewGuid():N}", 40);

        using var create = await admin.PostAsJsonAsync("/api/v1/admin/features", new
        {
            key,
            isEnabled = true,
            description = "security matrix",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var list = await admin.GetAsync("/api/v1/admin/features");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Ai_recommendation_and_roadmap_do_not_mutate_enrollments_or_profile_skills()
    {
        var userId = (await AuthClients.CreateUserClientWithIdAsync()).UserId;

        await using var scope = Factory.Services.CreateAsyncScope();
        var profiles = scope.ServiceProvider.GetRequiredService<ILearningProfileService>();
        await profiles.UpsertAsync(
            userId,
            new UpdateLearningProfileRequest(
                "Beginner",
                "Learn C#",
                "C#",
                [new LearningPreferenceDto(".NET", 1, 5)]));

        var beforeProfile = await profiles.GetAsync(userId);
        var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        var beforeEnrollments = await enrollments.ListByUserAsync(userId);

        var recommendations = scope.ServiceProvider.GetRequiredService<ILearningRecommendationService>();
        _ = await recommendations.GetRecommendationsAsync(userId);

        var roadmaps = scope.ServiceProvider.GetRequiredService<ILearningRoadmapService>();
        _ = await roadmaps.GenerateAsync(userId, new GenerateLearningRoadmapRequest("Learn C#"));

        var afterProfile = await profiles.GetAsync(userId);
        var afterEnrollments = await enrollments.ListByUserAsync(userId);

        Assert.Equal(beforeProfile.CurrentSkills, afterProfile.CurrentSkills);
        Assert.Equal(beforeProfile.LearningGoals, afterProfile.LearningGoals);
        Assert.Equal(beforeEnrollments.Count, afterEnrollments.Count);
    }
}
