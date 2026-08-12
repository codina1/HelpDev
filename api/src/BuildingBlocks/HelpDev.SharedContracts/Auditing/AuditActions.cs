namespace HelpDev.SharedContracts.Auditing;

public static class AuditActions
{
    public const string AuthenticationOtpRequested = "authentication.otp_requested";
    public const string AuthenticationOtpVerified = "authentication.otp_verified";
    public const string AuthenticationOtpVerificationFailed = "authentication.otp_verification_failed";
    public const string AuthenticationRateLimited = "authentication.rate_limited";
    public const string AuthenticationLoginSucceeded = "authentication.login_succeeded";
    public const string AuthenticationLoginFailed = "authentication.login_failed";

    public const string AuthorizationAccessDenied = "authorization.access_denied";

    public const string AdministrationFeatureFlagCreated = "administration.feature_flag_created";
    public const string AdministrationFeatureFlagUpdated = "administration.feature_flag_updated";
    public const string AdministrationFeatureFlagEnabled = "administration.feature_flag_enabled";
    public const string AdministrationFeatureFlagDisabled = "administration.feature_flag_disabled";
    public const string AdministrationSettingCreated = "administration.setting_created";
    public const string AdministrationSettingUpdated = "administration.setting_updated";
    public const string AdministrationAnnouncementCreated = "administration.announcement_created";
    public const string AdministrationAnnouncementUpdated = "administration.announcement_updated";
    public const string AdministrationAnnouncementPublished = "administration.announcement_published";
    public const string AdministrationAnnouncementArchived = "administration.announcement_archived";

    public const string ToolboxCategoryCreated = "toolbox.category_created";
    public const string ToolboxCategoryUpdated = "toolbox.category_updated";
    public const string ToolboxCategoryActivated = "toolbox.category_activated";
    public const string ToolboxCategoryDeactivated = "toolbox.category_deactivated";
    public const string ToolboxToolCreated = "toolbox.tool_created";
    public const string ToolboxToolUpdated = "toolbox.tool_updated";
    public const string ToolboxToolPublished = "toolbox.tool_published";
    public const string ToolboxToolUnpublished = "toolbox.tool_unpublished";
    public const string ToolboxToolEnabled = "toolbox.tool_enabled";
    public const string ToolboxToolDisabled = "toolbox.tool_disabled";

    public const string PromptLabCategoryCreated = "promptlab.category_created";
    public const string PromptLabCategoryUpdated = "promptlab.category_updated";
    public const string PromptLabCategoryActivated = "promptlab.category_activated";
    public const string PromptLabCategoryDeactivated = "promptlab.category_deactivated";
    public const string PromptLabPromptCreated = "promptlab.prompt_created";
    public const string PromptLabPromptUpdated = "promptlab.prompt_updated";
    public const string PromptLabVersionCreated = "promptlab.version_created";
    public const string PromptLabVersionPublished = "promptlab.version_published";
    public const string PromptLabPromptUnpublished = "promptlab.prompt_unpublished";
    public const string PromptLabPromptEnabled = "promptlab.prompt_enabled";
    public const string PromptLabPromptDisabled = "promptlab.prompt_disabled";

    public const string OutboxRetryRequested = "outbox.retry_requested";
    public const string OutboxDeadletterRecoveryRequested = "outbox.deadletter_recovery_requested";
    public const string OutboxMessageReprocessed = "outbox.message_reprocessed";

    public const string SecurityRateLimitExceeded = "security.rate_limit_exceeded";
    public const string SecurityRequestRejectedTooLarge = "security.request_rejected_too_large";

    public const string ContentAiTaskRequested = "content.ai_task_requested";
    public const string ContentAiTaskFailed = "content.ai_task_failed";

    public const string SemanticSearchRequested = "search.semantic_search_requested";
    public const string RagAnswerRequested = "search.rag_answer_requested";

    public const string LearningRecommendationRequested = "learning.recommendation_requested";
    public const string LearningRoadmapGenerated = "learning.roadmap_generated";

    public static bool IsSupported(string action) =>
        action switch
        {
            AuthenticationOtpRequested or AuthenticationOtpVerified or AuthenticationOtpVerificationFailed or
            AuthenticationRateLimited or AuthenticationLoginSucceeded or AuthenticationLoginFailed or
            AuthorizationAccessDenied or
            AdministrationFeatureFlagCreated or AdministrationFeatureFlagUpdated or
            AdministrationFeatureFlagEnabled or AdministrationFeatureFlagDisabled or
            AdministrationSettingCreated or AdministrationSettingUpdated or
            AdministrationAnnouncementCreated or AdministrationAnnouncementUpdated or
            AdministrationAnnouncementPublished or AdministrationAnnouncementArchived or
            ToolboxCategoryCreated or ToolboxCategoryUpdated or ToolboxCategoryActivated or ToolboxCategoryDeactivated or
            ToolboxToolCreated or ToolboxToolUpdated or ToolboxToolPublished or ToolboxToolUnpublished or
            ToolboxToolEnabled or ToolboxToolDisabled or
            PromptLabCategoryCreated or PromptLabCategoryUpdated or PromptLabCategoryActivated or PromptLabCategoryDeactivated or
            PromptLabPromptCreated or PromptLabPromptUpdated or PromptLabVersionCreated or PromptLabVersionPublished or
            PromptLabPromptUnpublished or PromptLabPromptEnabled or PromptLabPromptDisabled or
            OutboxRetryRequested or OutboxDeadletterRecoveryRequested or OutboxMessageReprocessed or
            SecurityRateLimitExceeded or SecurityRequestRejectedTooLarge or
            ContentAiTaskRequested or ContentAiTaskFailed or
            SemanticSearchRequested or RagAnswerRequested or
            LearningRecommendationRequested or LearningRoadmapGenerated => true,
            _ => false,
        };
}

public static class AuditCategories
{
    public const string Authentication = "Authentication";
    public const string Authorization = "Authorization";
    public const string Administration = "Administration";
    public const string ToolboxManagement = "ToolboxManagement";
    public const string PromptManagement = "PromptManagement";
    public const string OutboxOperations = "OutboxOperations";
    public const string Security = "Security";
    public const string ContentAi = "ContentAi";
    public const string SearchRag = "SearchRag";
    public const string LearningAi = "LearningAi";

    public static bool IsSupported(string category) =>
        category switch
        {
            Authentication or Authorization or Administration or
            ToolboxManagement or PromptManagement or OutboxOperations or Security or ContentAi or SearchRag or LearningAi => true,
            _ => false,
        };
}

public static class AuditOutcomes
{
    public const string Success = "Success";
    public const string Failure = "Failure";
    public const string Denied = "Denied";

    public static bool IsSupported(string outcome) =>
        outcome is Success or Failure or Denied;
}

public static class AuditActorTypes
{
    public const string User = "User";
    public const string Anonymous = "Anonymous";
    public const string System = "System";
}
