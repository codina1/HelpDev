namespace HelpDev.Modules.Analytics.Domain;

public static class AnalyticsMetricKeys
{
    public const string UsersRegistered = "users.registered";
    public const string UsersLoginSucceeded = "users.login_succeeded";
    public const string UsersActive = "users.active";

    public const string ContentCreated = "content.created";
    public const string ContentPublished = "content.published";
    public const string ContentViews = "content.views";

    public const string LearningCoursesCreated = "learning.courses_created";
    public const string LearningCoursesPublished = "learning.courses_published";
    public const string LearningEnrollments = "learning.enrollments";
    public const string LearningLessonsCompleted = "learning.lessons_completed";
    public const string LearningRecommendationsRequested = "learning.recommendations_requested";
    public const string LearningRoadmapsGenerated = "learning.roadmaps_generated";

    public const string SearchExecutions = "search.executions";
    public const string SearchZeroResults = "search.zero_results";
    public const string SearchDocumentsIndexed = "search.documents_indexed";

    public const string ToolboxExecutions = "toolbox.executions";
    public const string ToolboxExecutionsSucceeded = "toolbox.executions_succeeded";
    public const string ToolboxExecutionsFailed = "toolbox.executions_failed";
    public const string ToolboxExecutionDuration = "toolbox.execution_duration";

    public const string PromptLabRenders = "promptlab.renders";
    public const string PromptLabRendersSucceeded = "promptlab.renders_succeeded";
    public const string PromptLabRendersFailed = "promptlab.renders_failed";
    public const string PromptLabRenderDuration = "promptlab.render_duration";

    public static bool IsSupported(string metricKey) =>
        metricKey switch
        {
            UsersRegistered or UsersLoginSucceeded or UsersActive or
            ContentCreated or ContentPublished or ContentViews or
            LearningCoursesCreated or LearningCoursesPublished or
            LearningEnrollments or LearningLessonsCompleted or
            LearningRecommendationsRequested or LearningRoadmapsGenerated or
            SearchExecutions or SearchZeroResults or SearchDocumentsIndexed or
            ToolboxExecutions or ToolboxExecutionsSucceeded or
            ToolboxExecutionsFailed or ToolboxExecutionDuration or
            PromptLabRenders or PromptLabRendersSucceeded or
            PromptLabRendersFailed or PromptLabRenderDuration => true,
            _ => false,
        };
}
