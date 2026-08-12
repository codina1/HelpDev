namespace HelpDev.SharedContracts.Ai;

/// <summary>Stable, non-sensitive AI failure codes for callers and telemetry.</summary>
public static class AiErrorCodes
{
    public const string ProviderUnavailable = "ai_provider_unavailable";
    public const string GenerationFailed = "ai_generation_failed";
    public const string Timeout = "ai_timeout";
    public const string InvalidResponse = "ai_invalid_response";
    public const string Disabled = "ai_disabled";
    public const string Unauthorized = "ai_unauthorized";
    public const string ValidationFailed = "ai_validation_failed";

    public static bool IsTransient(string? code) =>
        code is ProviderUnavailable or Timeout or GenerationFailed;
}

/// <summary>Canonical operation names for usage analytics (never prompts).</summary>
public static class AiOperationNames
{
    public const string ContentAssistant = "ContentAssistant";
    public const string WorkflowResearch = "WorkflowResearch";
    public const string WorkflowOutline = "WorkflowOutline";
    public const string WorkflowDraft = "WorkflowDraft";
    public const string WorkflowSeo = "WorkflowSeo";
    public const string LearningRecommend = "LearningRecommend";
    public const string LearningRoadmap = "LearningRoadmap";
    public const string RagAnswer = "RagAnswer";
}

/// <summary>
/// Safe generation outcome. Never contains API keys. Content is returned only to the caller
/// and must not be persisted by telemetry layers.
/// </summary>
public sealed record AiGenerationResult(
    bool Success,
    string? Content,
    string? ErrorCode,
    long LatencyMs,
    string? Model = null,
    string? Provider = null,
    AiTokenUsage? Usage = null)
{
    public static AiGenerationResult Ok(
        string content,
        long latencyMs,
        string model,
        string provider,
        AiTokenUsage? usage) =>
        new(true, content, null, latencyMs, model, provider, usage);

    public static AiGenerationResult Fail(
        string errorCode,
        long latencyMs,
        string? provider = null,
        string? model = null) =>
        new(false, null, errorCode, latencyMs, model, provider, null);
}
