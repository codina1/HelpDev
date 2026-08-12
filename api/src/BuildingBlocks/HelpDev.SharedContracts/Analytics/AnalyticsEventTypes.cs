namespace HelpDev.SharedContracts.Analytics;

public static class AnalyticsEventTypes
{
    public const string IdentityUserRegistered = "identity.user_registered";
    public const string IdentityUserLoginSucceeded = "identity.user_login_succeeded";

    public const string ContentItemCreated = "content.item_created";
    public const string ContentItemPublished = "content.item_published";
    public const string ContentItemViewed = "content.item_viewed";

    public const string LearningCourseCreated = "learning.course_created";
    public const string LearningCoursePublished = "learning.course_published";
    public const string LearningEnrollmentCreated = "learning.enrollment_created";
    public const string LearningLessonCompleted = "learning.lesson_completed";

    public const string LearningRecommendationRequested = "learning.recommendation_requested";
    public const string LearningRoadmapGenerated = "learning.roadmap_generated";

    public const string SearchExecuted = "search.executed";
    public const string SearchZeroResults = "search.zero_results";
    public const string SearchDocumentIndexed = "search.document_indexed";

    public const string ToolboxExecutionSucceeded = "toolbox.execution_succeeded";
    public const string ToolboxExecutionFailed = "toolbox.execution_failed";

    public const string PromptLabRenderSucceeded = "promptlab.render_succeeded";
    public const string PromptLabRenderFailed = "promptlab.render_failed";

    public static bool IsSupported(string eventType) =>
        eventType switch
        {
            IdentityUserRegistered or IdentityUserLoginSucceeded or
            ContentItemCreated or ContentItemPublished or ContentItemViewed or
            LearningCourseCreated or LearningCoursePublished or
            LearningEnrollmentCreated or LearningLessonCompleted or
            LearningRecommendationRequested or LearningRoadmapGenerated or
            SearchExecuted or SearchZeroResults or SearchDocumentIndexed or
            ToolboxExecutionSucceeded or ToolboxExecutionFailed or
            PromptLabRenderSucceeded or PromptLabRenderFailed => true,
            _ => false,
        };
}
