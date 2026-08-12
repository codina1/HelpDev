using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Learning.Domain.Personalization;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Learning.Tests.Personalization;

public sealed class LearningProfileDomainTests
{
    [Fact]
    public void Profile_update_replaces_preferences_without_ai_side_effects()
    {
        var profile = LearningProfile.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ExperienceLevel.Beginner,
            "Learn .NET",
            "C#",
            DateTime.UtcNow);

        profile.Update(
            ExperienceLevel.Intermediate,
            "Become AI Developer",
            "C#, ASP.NET",
            [new LearningPreferenceInput("AI", 1, 5), new LearningPreferenceInput(".NET", 2, 4)],
            DateTime.UtcNow);

        Assert.Equal(ExperienceLevel.Intermediate, profile.ExperienceLevel);
        Assert.Equal(2, profile.Preferences.Count);
        Assert.Contains(profile.Preferences, p => p.Topic == "AI");
    }

    [Fact]
    public void Roadmap_requires_user_approval()
    {
        var roadmap = LearningRoadmap.CreateSuggested(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Become AI Developer",
            [
                new LearningRoadmapStepInput("C# Advanced", null, null),
                new LearningRoadmapStepInput("ASP.NET Core", null, null),
            ],
            DateTime.UtcNow);

        Assert.Equal(LearningRoadmapStatus.Suggested, roadmap.Status);
        roadmap.Approve(DateTime.UtcNow);
        Assert.Equal(LearningRoadmapStatus.Approved, roadmap.Status);
        Assert.NotNull(roadmap.ApprovedAtUtc);
    }
}

public sealed class LearningProfileServiceTests
{
    [Fact]
    public async Task Upsert_and_get_roundtrip()
    {
        var userId = Guid.NewGuid();
        var repo = new InMemoryProfileRepository();
        var service = new LearningProfileService(repo, new StubUnitOfWork(), new FixedClock());

        var saved = await service.UpsertAsync(
            userId,
            new UpdateLearningProfileRequest(
                "Advanced",
                "Architecture",
                "DDD",
                [new LearningPreferenceDto("Architecture", 1, 5)]));

        Assert.Equal("Advanced", saved.ExperienceLevel);
        Assert.Single(saved.PreferredTopics);

        var loaded = await service.GetAsync(userId);
        Assert.Equal("Architecture", loaded.LearningGoals);
        Assert.Equal("Architecture", loaded.PreferredTopics[0].Topic);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = new(2026, 7, 23, 12, 0, 0, DateTimeKind.Utc);
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class InMemoryProfileRepository : ILearningProfileRepository
    {
        private LearningProfile? _profile;

        public Task<LearningProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_profile?.UserId == userId ? _profile : null);

        public Task AddAsync(LearningProfile profile, CancellationToken cancellationToken = default)
        {
            _profile = profile;
            return Task.CompletedTask;
        }
    }
}

public sealed class LearningPersonalizationArchitectureTests
{
    [Fact]
    public void Recommendation_dto_has_no_ranking_or_confidence()
    {
        var names = typeof(LearningRecommendationDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.Contains("RecommendedItems", names);
        Assert.Contains("Reason", names);
        Assert.Contains("NextSteps", names);
        Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Confidence", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(names, n => n.Contains("Prompt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Learning_module_does_not_reference_infrastructure_ai()
    {
        var refs = typeof(LearningProfileService).Assembly.GetReferencedAssemblies().Select(a => a.Name ?? "");
        Assert.DoesNotContain(refs, n => n.Equals("HelpDev.Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(refs, n => n.Contains("OpenAI", StringComparison.OrdinalIgnoreCase));
    }
}
