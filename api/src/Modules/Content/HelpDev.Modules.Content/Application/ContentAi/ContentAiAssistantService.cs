using HelpDev.Modules.Content.Application.Common;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Content.Application.ContentAi;

public sealed class ContentAiAssistantService : IContentAiAssistantService
{
    private readonly IContentService _contentService;
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IContentAiFeatureGate _featureGate;
    private readonly IAiUsageRecorder _usageRecorder;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ContentAiAssistantService> _logger;

    public ContentAiAssistantService(
        IContentService contentService,
        IAiTextGenerator aiTextGenerator,
        IContentAiFeatureGate featureGate,
        IAiUsageRecorder usageRecorder,
        IAuditRecorder auditRecorder,
        IDateTimeProvider clock,
        ILogger<ContentAiAssistantService> logger)
    {
        _contentService = contentService;
        _aiTextGenerator = aiTextGenerator;
        _featureGate = featureGate;
        _usageRecorder = usageRecorder;
        _auditRecorder = auditRecorder;
        _clock = clock;
        _logger = logger;
    }

    public Task<ContentAiResultDto> AnalyzeContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(actor, contentId, ContentAiTaskType.ContentAnalysis, cancellationToken);

    public Task<ContentAiResultDto> GenerateTitleSuggestionsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(actor, contentId, ContentAiTaskType.TitleSuggestion, cancellationToken);

    public Task<ContentAiResultDto> GenerateMetaDescriptionAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(actor, contentId, ContentAiTaskType.MetaDescription, cancellationToken);

    public Task<ContentAiResultDto> GenerateOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(actor, contentId, ContentAiTaskType.OutlineGeneration, cancellationToken);

    public Task<ContentAiResultDto> GenerateFaqAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(actor, contentId, ContentAiTaskType.FaqGeneration, cancellationToken);

    private async Task<ContentAiResultDto> ExecuteAsync(
        ContentManagementActor actor,
        Guid contentId,
        ContentAiTaskType taskType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (!_featureGate.IsEnabled)
        {
            throw new ContentAiException("دستیار هوش مصنوعی غیرفعال است.", ContentAiErrorCodes.Disabled);
        }

        if (!_featureGate.IsTaskAllowed(taskType))
        {
            throw new ContentAiException("این وظیفه مجاز نیست.", ContentAiErrorCodes.TaskNotAllowed);
        }

        var detail = await _contentService.GetManagedByIdAsync(actor, contentId, cancellationToken);
        var taskName = ContentAiTaskTypeCatalog.ToWireName(taskType);

        await TryAuditAsync(
            AuditActions.ContentAiTaskRequested,
            AuditOutcomes.Success,
            actor.UserId,
            contentId,
            taskName,
            failureCode: null,
            cancellationToken);

        var request = new AiTextRequest(
            taskName,
            BuildSystemInstruction(taskType),
            BuildInputText(detail),
            MaxTokens: 1024);

        var result = await _aiTextGenerator.GenerateSafeAsync(request, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Content AI task {TaskType} failed for content {ContentId} with {ErrorCode}",
                taskName,
                contentId,
                result.ErrorCode);

            await TryRecordUsageAsync(
                actor.UserId,
                AiOperationNames.ContentAssistant,
                result,
                contentId,
                cancellationToken);

            await TryAuditAsync(
                AuditActions.ContentAiTaskFailed,
                AuditOutcomes.Failure,
                actor.UserId,
                contentId,
                taskName,
                failureCode: result.ErrorCode ?? ContentAiErrorCodes.ProviderFailed,
                cancellationToken);

            throw new ContentAiException(
                "تولید پیشنهاد ناموفق بود.",
                MapContentAiError(result.ErrorCode));
        }

        await TryRecordUsageAsync(
            actor.UserId,
            AiOperationNames.ContentAssistant,
            result,
            contentId,
            cancellationToken);

        return new ContentAiResultDto(
            taskName,
            result.Content!,
            _clock.UtcNow,
            result.Model ?? "unknown",
            result.Provider ?? "unknown");
    }

    private static string MapContentAiError(string? errorCode) =>
        errorCode switch
        {
            AiErrorCodes.Timeout => ContentAiErrorCodes.ProviderFailed,
            AiErrorCodes.ProviderUnavailable => ContentAiErrorCodes.ProviderFailed,
            AiErrorCodes.Disabled => ContentAiErrorCodes.Disabled,
            _ => ContentAiErrorCodes.ProviderFailed,
        };

    private static string BuildSystemInstruction(ContentAiTaskType taskType) =>
        taskType switch
        {
            ContentAiTaskType.ContentAnalysis =>
                "You are an editorial assistant. Analyze structure and clarity. Do not invent rankings or scores.",
            ContentAiTaskType.TitleSuggestion =>
                "Suggest up to 5 concise Persian/English titles. No ranking scores.",
            ContentAiTaskType.MetaDescription =>
                "Suggest one SEO meta description under 160 characters. No keyword volume claims.",
            ContentAiTaskType.OutlineGeneration =>
                "Propose a hierarchical article outline with H2/H3 headings.",
            ContentAiTaskType.FaqGeneration =>
                "Propose 3-5 FAQ Q&A pairs grounded in the provided content.",
            _ => "Assist with controlled editorial generation.",
        };

    private static string BuildInputText(AdminContentDetailDto detail)
    {
        // Truncate body for provider safety; still never log this string.
        var body = detail.Body ?? string.Empty;
        if (body.Length > 8000)
        {
            body = body[..8000];
        }

        return $"""
            Title: {detail.Title}
            Slug: {detail.Slug}
            Type: {detail.ContentType}
            Excerpt: {detail.Excerpt}
            SeoTitle: {detail.Seo.SeoTitle}
            SeoDescription: {detail.Seo.SeoDescription}
            FocusKeyword: {detail.Seo.FocusKeyword}
            Body:
            {body}
            """;
    }

    private async Task TryRecordUsageAsync(
        Guid userId,
        string taskType,
        AiGenerationResult result,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _usageRecorder.RecordAsync(
                new AiUsageRecordInput(
                    userId,
                    taskType,
                    result.Provider ?? "unknown",
                    result.Model ?? "unknown",
                    result.Usage?.InputTokens ?? 0,
                    result.Usage?.OutputTokens ?? 0,
                    contentId,
                    result.Success,
                    (int)Math.Clamp(result.LatencyMs, 0, int.MaxValue),
                    result.ErrorCode),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI usage recording failed for task {TaskType}", taskType);
        }
    }

    private async Task TryAuditAsync(
        string action,
        string outcome,
        Guid userId,
        Guid contentId,
        string taskType,
        string? failureCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["taskType"] = taskType,
                ["contentId"] = contentId.ToString("D"),
            };
            if (!string.IsNullOrWhiteSpace(failureCode))
            {
                metadata["failureCode"] = failureCode;
            }

            await _auditRecorder.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.ContentAi,
                    action,
                    outcome,
                    userId,
                    AuditActorTypes.User,
                    SubjectId: contentId,
                    SubjectType: "Content",
                    Metadata: metadata),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI audit recording failed for action {Action}", action);
        }
    }
}
