using HelpDev.Modules.Analytics.Application.Processing;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Analytics.Tests;

public sealed class AnalyticsMetricMapperTests
{
    private static AnalyticsEventEnvelope Build(
        string eventType,
        Guid? actorUserId = null,
        Guid? subjectId = null,
        string? subjectType = null,
        IReadOnlyDictionary<string, string>? dimensions = null,
        long? durationMs = null) =>
        new(
            EventId: Guid.NewGuid(),
            EventType: eventType,
            OccurredAtUtc: new DateTime(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc),
            ActorUserId: actorUserId,
            SubjectId: subjectId,
            SubjectType: subjectType,
            Dimensions: dimensions,
            Quantity: 1,
            DurationMilliseconds: durationMs,
            SubjectDisplayName: null,
            SubjectSlug: null,
            SchemaVersion: 1);

    [Fact]
    public void UserRegistered_produces_single_global_metric()
    {
        var result = AnalyticsMetricMapper.Map(Build(AnalyticsEventTypes.IdentityUserRegistered));

        Assert.Single(result.Metrics);
        Assert.False(result.MarkActiveUser);
        Assert.Null(result.Snapshot);
    }

    [Fact]
    public void LoginSucceeded_marks_active_user_for_authenticated_actor()
    {
        var userId = Guid.NewGuid();
        var result = AnalyticsMetricMapper.Map(Build(AnalyticsEventTypes.IdentityUserLoginSucceeded, actorUserId: userId));

        Assert.True(result.MarkActiveUser);
    }

    [Fact]
    public void LoginSucceeded_does_not_mark_active_user_for_anonymous()
    {
        var result = AnalyticsMetricMapper.Map(Build(AnalyticsEventTypes.IdentityUserLoginSucceeded, actorUserId: null));

        Assert.False(result.MarkActiveUser);
    }

    [Fact]
    public void ToolboxSucceeded_produces_multiple_metrics_and_marks_active_user()
    {
        var userId = Guid.NewGuid();
        var dims = new Dictionary<string, string>
        {
            [AnalyticsDimensionKeys.ToolType] = "encoder",
            [AnalyticsDimensionKeys.IsAuthenticated] = "true",
        };
        var result = AnalyticsMetricMapper.Map(
            Build(AnalyticsEventTypes.ToolboxExecutionSucceeded, actorUserId: userId, dimensions: dims, durationMs: 100));

        Assert.True(result.Metrics.Count >= 3);
        Assert.True(result.MarkActiveUser);
    }

    [Fact]
    public void ToolboxSucceeded_with_subject_produces_subject_metric()
    {
        var subjectId = Guid.NewGuid();
        var dims = new Dictionary<string, string>
        {
            [AnalyticsDimensionKeys.ToolType] = "encoder",
            [AnalyticsDimensionKeys.IsAuthenticated] = "true",
        };
        var result = AnalyticsMetricMapper.Map(
            Build(AnalyticsEventTypes.ToolboxExecutionSucceeded,
                actorUserId: Guid.NewGuid(),
                subjectId: subjectId,
                subjectType: "tool",
                dimensions: dims));

        Assert.Contains(result.Metrics, m => m.Identity.SubjectId == subjectId);
    }

    [Fact]
    public void PromptLabRenderFailed_increments_failures()
    {
        var dims = new Dictionary<string, string>
        {
            [AnalyticsDimensionKeys.Purpose] = "codeReview",
            [AnalyticsDimensionKeys.IsAuthenticated] = "true",
            [AnalyticsDimensionKeys.ErrorCode] = "timeout",
        };
        var result = AnalyticsMetricMapper.Map(
            Build(AnalyticsEventTypes.PromptLabRenderFailed, actorUserId: Guid.NewGuid(), dimensions: dims));

        Assert.Contains(result.Metrics, m => m.IncrementFailure);
        Assert.False(result.Metrics.All(m => m.IncrementSuccess));
    }

    [Fact]
    public void Unknown_event_type_throws()
    {
        var envelope = Build("not.a.real.event");

        Assert.Throws<Modules.Analytics.Application.AnalyticsException>(
            () => AnalyticsMetricMapper.Map(envelope));
    }
}
