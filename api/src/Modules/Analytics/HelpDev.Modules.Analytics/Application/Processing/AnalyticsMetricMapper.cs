using HelpDev.Modules.Analytics.Domain;
using HelpDev.Modules.Analytics.Domain.Metrics;
using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Modules.Analytics.Application.Processing;

public sealed record MetricIncrementPlan(
    DailyMetricIdentity Identity,
    bool IncrementSuccess,
    bool IncrementFailure,
    long? DurationMilliseconds);

public sealed record SubjectSnapshotPlan(
    string SubjectType,
    Guid SubjectId,
    string DisplayName,
    string? Slug);

public sealed record AnalyticsMappingResult(
    IReadOnlyList<MetricIncrementPlan> Metrics,
    bool MarkActiveUser,
    SubjectSnapshotPlan? Snapshot);

public static class AnalyticsMetricMapper
{
    public static AnalyticsMappingResult Map(AnalyticsEventEnvelope envelope)
    {
        var dateUtc = DateOnly.FromDateTime(envelope.OccurredAtUtc);
        var quantity = envelope.Quantity;
        var dimensions = envelope.Dimensions ?? new Dictionary<string, string>();
        var metrics = new List<MetricIncrementPlan>();
        SubjectSnapshotPlan? snapshot = null;
        var markActive = envelope.ActorUserId.HasValue && ShouldMarkActiveUser(envelope.EventType);

        switch (envelope.EventType)
        {
            case AnalyticsEventTypes.IdentityUserRegistered:
                metrics.Add(Global(AnalyticsMetricKeys.UsersRegistered, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.IdentityUserLoginSucceeded:
                metrics.Add(Global(AnalyticsMetricKeys.UsersLoginSucceeded, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.ContentItemCreated:
                metrics.Add(Global(AnalyticsMetricKeys.ContentCreated, dateUtc, quantity, true, false, null));
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.ContentItemPublished:
                metrics.Add(Global(AnalyticsMetricKeys.ContentPublished, dateUtc, quantity, true, false, null));
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.ContentItemViewed:
                metrics.Add(Global(AnalyticsMetricKeys.ContentViews, dateUtc, quantity, true, false, null));
                if (envelope.SubjectId.HasValue)
                {
                    metrics.Add(SubjectMetric(
                        AnalyticsMetricKeys.ContentViews,
                        dateUtc,
                        envelope.SubjectId,
                        AnalyticsSubjectTypes.Content,
                        quantity,
                        true,
                        false,
                        null,
                        dimensions));
                }

                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.LearningCourseCreated:
                metrics.Add(Global(AnalyticsMetricKeys.LearningCoursesCreated, dateUtc, quantity, true, false, null));
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.LearningCoursePublished:
                metrics.Add(Global(AnalyticsMetricKeys.LearningCoursesPublished, dateUtc, quantity, true, false, null));
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.LearningEnrollmentCreated:
                metrics.Add(Global(AnalyticsMetricKeys.LearningEnrollments, dateUtc, quantity, true, false, null));
                if (envelope.SubjectId.HasValue)
                {
                    metrics.Add(SubjectMetric(
                        AnalyticsMetricKeys.LearningEnrollments,
                        dateUtc,
                        envelope.SubjectId,
                        AnalyticsSubjectTypes.Course,
                        quantity,
                        true,
                        false,
                        null,
                        dimensions));
                }

                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.LearningLessonCompleted:
                metrics.Add(Global(AnalyticsMetricKeys.LearningLessonsCompleted, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.LearningRecommendationRequested:
                metrics.Add(Global(AnalyticsMetricKeys.LearningRecommendationsRequested, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.LearningRoadmapGenerated:
                metrics.Add(Global(AnalyticsMetricKeys.LearningRoadmapsGenerated, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.SearchExecuted:
                metrics.Add(DimensionMetric(
                    AnalyticsMetricKeys.SearchExecutions,
                    dateUtc,
                    quantity,
                    true,
                    false,
                    null,
                    dimensions));
                break;

            case AnalyticsEventTypes.SearchZeroResults:
                metrics.Add(DimensionMetric(
                    AnalyticsMetricKeys.SearchZeroResults,
                    dateUtc,
                    quantity,
                    true,
                    false,
                    null,
                    dimensions));
                break;

            case AnalyticsEventTypes.SearchDocumentIndexed:
                metrics.Add(Global(AnalyticsMetricKeys.SearchDocumentsIndexed, dateUtc, quantity, true, false, null));
                break;

            case AnalyticsEventTypes.ToolboxExecutionSucceeded:
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutions, dateUtc, quantity, false, false, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutionsSucceeded, dateUtc, quantity, true, false, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutionDuration, dateUtc, quantity, true, false, envelope.DurationMilliseconds));
                AddToolboxMetrics(metrics, envelope, dateUtc, quantity, succeeded: true);
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.ToolboxExecutionFailed:
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutions, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutionsFailed, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.ToolboxExecutionDuration, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                AddToolboxMetrics(metrics, envelope, dateUtc, quantity, succeeded: false);
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.PromptLabRenderSucceeded:
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRenders, dateUtc, quantity, false, false, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRendersSucceeded, dateUtc, quantity, true, false, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRenderDuration, dateUtc, quantity, true, false, envelope.DurationMilliseconds));
                AddPromptMetrics(metrics, envelope, dateUtc, quantity, succeeded: true);
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            case AnalyticsEventTypes.PromptLabRenderFailed:
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRenders, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRendersFailed, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                metrics.Add(Global(AnalyticsMetricKeys.PromptLabRenderDuration, dateUtc, quantity, false, true, envelope.DurationMilliseconds));
                AddPromptMetrics(metrics, envelope, dateUtc, quantity, succeeded: false);
                AddSubjectSnapshot(ref snapshot, envelope, dimensions);
                break;

            default:
                throw new AnalyticsException(
                    "Metric mapping was not found.",
                    AnalyticsApplicationErrorCodes.MetricMappingNotFound);
        }

        if (markActive && envelope.ActorUserId.HasValue)
        {
            // DAU is derived from DailyActiveUser markers, not metric sums.
        }

        return new AnalyticsMappingResult(metrics, markActive, snapshot);
    }

    private static bool ShouldMarkActiveUser(string eventType) =>
        eventType switch
        {
            AnalyticsEventTypes.IdentityUserLoginSucceeded or
            AnalyticsEventTypes.ContentItemViewed or
            AnalyticsEventTypes.LearningEnrollmentCreated or
            AnalyticsEventTypes.LearningLessonCompleted or
            AnalyticsEventTypes.SearchExecuted or
            AnalyticsEventTypes.ToolboxExecutionSucceeded or
            AnalyticsEventTypes.ToolboxExecutionFailed or
            AnalyticsEventTypes.PromptLabRenderSucceeded or
            AnalyticsEventTypes.PromptLabRenderFailed => true,
            _ => false,
        };

    private static void AddToolboxMetrics(
        List<MetricIncrementPlan> metrics,
        AnalyticsEventEnvelope envelope,
        DateOnly dateUtc,
        long quantity,
        bool succeeded)
    {
        if (!envelope.SubjectId.HasValue)
        {
            return;
        }

        metrics.Add(SubjectMetric(
            AnalyticsMetricKeys.ToolboxExecutions,
            dateUtc,
            envelope.SubjectId,
            AnalyticsSubjectTypes.Tool,
            quantity,
            succeeded,
            !succeeded,
            envelope.DurationMilliseconds,
            envelope.Dimensions ?? new Dictionary<string, string>()));
    }

    private static void AddPromptMetrics(
        List<MetricIncrementPlan> metrics,
        AnalyticsEventEnvelope envelope,
        DateOnly dateUtc,
        long quantity,
        bool succeeded)
    {
        if (!envelope.SubjectId.HasValue)
        {
            return;
        }

        metrics.Add(SubjectMetric(
            AnalyticsMetricKeys.PromptLabRenders,
            dateUtc,
            envelope.SubjectId,
            AnalyticsSubjectTypes.Prompt,
            quantity,
            succeeded,
            !succeeded,
            envelope.DurationMilliseconds,
            envelope.Dimensions ?? new Dictionary<string, string>()));
    }

    private static MetricIncrementPlan Global(
        string metricKey,
        DateOnly dateUtc,
        long quantity,
        bool success,
        bool failure,
        long? duration) =>
        new(
            new DailyMetricIdentity(dateUtc, metricKey, null, null, string.Empty, string.Empty, string.Empty, string.Empty),
            success,
            failure,
            duration);

    private static MetricIncrementPlan SubjectMetric(
        string metricKey,
        DateOnly dateUtc,
        Guid? subjectId,
        string subjectType,
        long quantity,
        bool success,
        bool failure,
        long? duration,
        IReadOnlyDictionary<string, string> dimensions) =>
        new(
            new DailyMetricIdentity(
                dateUtc,
                metricKey,
                subjectId,
                subjectType,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty),
            success,
            failure,
            duration);

    private static MetricIncrementPlan DimensionMetric(
        string metricKey,
        DateOnly dateUtc,
        long quantity,
        bool success,
        bool failure,
        long? duration,
        IReadOnlyDictionary<string, string> dimensions)
    {
        dimensions.TryGetValue(AnalyticsDimensionKeys.ResultBucket, out var bucket);
        dimensions.TryGetValue(AnalyticsDimensionKeys.IsAuthenticated, out var isAuthenticated);

        return new MetricIncrementPlan(
            new DailyMetricIdentity(
                dateUtc,
                metricKey,
                null,
                null,
                AnalyticsDimensionKeys.ResultBucket,
                bucket ?? string.Empty,
                AnalyticsDimensionKeys.IsAuthenticated,
                isAuthenticated ?? string.Empty),
            success,
            failure,
            duration);
    }

    private static void AddSubjectSnapshot(
        ref SubjectSnapshotPlan? snapshot,
        AnalyticsEventEnvelope envelope,
        IReadOnlyDictionary<string, string> dimensions)
    {
        if (!envelope.SubjectId.HasValue || string.IsNullOrWhiteSpace(envelope.SubjectType))
        {
            return;
        }

        var displayName = envelope.SubjectDisplayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            dimensions.TryGetValue(AnalyticsDimensionKeys.ToolSlug, out var toolSlug);
            dimensions.TryGetValue(AnalyticsDimensionKeys.PromptSlug, out var promptSlug);
            displayName = toolSlug ?? promptSlug ?? envelope.SubjectType;
        }

        var slug = envelope.SubjectSlug
            ?? dimensions.GetValueOrDefault(AnalyticsDimensionKeys.ToolSlug)
            ?? dimensions.GetValueOrDefault(AnalyticsDimensionKeys.PromptSlug);

        snapshot = new SubjectSnapshotPlan(
            envelope.SubjectType,
            envelope.SubjectId.Value,
            displayName,
            slug);
    }
}
