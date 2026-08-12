using HelpDev.SharedContracts.Analytics;

namespace HelpDev.Modules.Analytics.Application.Processing;

public static class AnalyticsDimensionRules
{
    public static IReadOnlyDictionary<string, bool> GetAllowedDimensions(string eventType) =>
        eventType switch
        {
            AnalyticsEventTypes.IdentityUserRegistered => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.RegistrationMethod] = true,
            },
            AnalyticsEventTypes.IdentityUserLoginSucceeded => new Dictionary<string, bool>(),
            AnalyticsEventTypes.ContentItemCreated => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.ContentType] = true,
            },
            AnalyticsEventTypes.ContentItemPublished => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.ContentType] = false,
            },
            AnalyticsEventTypes.ContentItemViewed => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.ContentType] = true,
                [AnalyticsDimensionKeys.IsAuthenticated] = true,
            },
            AnalyticsEventTypes.LearningCourseCreated or AnalyticsEventTypes.LearningCoursePublished => new Dictionary<string, bool>(),
            AnalyticsEventTypes.LearningEnrollmentCreated => new Dictionary<string, bool>(),
            AnalyticsEventTypes.LearningLessonCompleted => new Dictionary<string, bool>(),
            AnalyticsEventTypes.SearchExecuted or AnalyticsEventTypes.SearchZeroResults => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.ResultBucket] = true,
                [AnalyticsDimensionKeys.IsAuthenticated] = true,
            },
            AnalyticsEventTypes.SearchDocumentIndexed => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.SourceType] = true,
            },
            AnalyticsEventTypes.ToolboxExecutionSucceeded or AnalyticsEventTypes.ToolboxExecutionFailed => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.ToolType] = true,
                [AnalyticsDimensionKeys.ToolSlug] = false,
                [AnalyticsDimensionKeys.IsAuthenticated] = true,
                [AnalyticsDimensionKeys.ErrorCode] = eventType == AnalyticsEventTypes.ToolboxExecutionFailed,
            },
            AnalyticsEventTypes.PromptLabRenderSucceeded or AnalyticsEventTypes.PromptLabRenderFailed => new Dictionary<string, bool>
            {
                [AnalyticsDimensionKeys.Purpose] = true,
                [AnalyticsDimensionKeys.PromptSlug] = false,
                [AnalyticsDimensionKeys.IsAuthenticated] = true,
                [AnalyticsDimensionKeys.VersionNumber] = false,
                [AnalyticsDimensionKeys.ErrorCode] = eventType == AnalyticsEventTypes.PromptLabRenderFailed,
            },
            _ => new Dictionary<string, bool>(),
        };
}
