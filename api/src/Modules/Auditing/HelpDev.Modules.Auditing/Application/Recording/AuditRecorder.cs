using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Auditing.Application.Recording;

public interface IAuditMetadataSanitizer
{
    IReadOnlyDictionary<string, string>? Sanitize(
        string action,
        IReadOnlyDictionary<string, string>? metadata);
}

public sealed class AuditMetadataSanitizer : IAuditMetadataSanitizer
{
    private static readonly string[] SensitiveKeyPatterns =
    [
        "password", "otp", "token", "secret", "apikey", "authorization", "cookie",
        "connectionstring", "privatekey", "credential", "phone", "email", "prompt",
        "template", "rendered", "input", "output", "body", "value", "embedding", "vector",
    ];

    private readonly AuditOptions _options;

    public AuditMetadataSanitizer(IOptions<AuditOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyDictionary<string, string>? Sanitize(
        string action,
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return null;
        }

        if (metadata.Count > _options.MaxMetadataEntries)
        {
            throw new AuditException("Audit metadata exceeds maximum entries.", AuditErrorCodes.MetadataInvalid);
        }

        var allowedKeys = AuditMetadataAllowList.GetAllowedKeys(action);
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var (key, value) in metadata)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > _options.MaxMetadataKeyLength)
            {
                throw new AuditException("Audit metadata key is invalid.", AuditErrorCodes.MetadataInvalid);
            }

            if (allowedKeys.Count > 0 && !allowedKeys.Contains(key))
            {
                throw new AuditException("Audit metadata key is not allowed for action.", AuditErrorCodes.MetadataInvalid);
            }

            if (IsSensitiveKey(key) && (allowedKeys.Count == 0 || !allowedKeys.Contains(key)))
            {
                throw new AuditException("Audit metadata contains sensitive key.", AuditErrorCodes.MetadataSensitive);
            }

            if (value is null || value.Length > _options.MaxMetadataValueLength)
            {
                throw new AuditException("Audit metadata value is invalid.", AuditErrorCodes.MetadataInvalid);
            }

            if (ContainsControlCharacters(key) || ContainsControlCharacters(value))
            {
                throw new AuditException("Audit metadata contains control characters.", AuditErrorCodes.MetadataInvalid);
            }

            sanitized[key] = value;
        }

        return sanitized;
    }

    private static bool IsSensitiveKey(string key)
    {
        foreach (var pattern in SensitiveKeyPatterns)
        {
            if (key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsControlCharacters(string value) =>
        value.Any(static c => char.IsControl(c) && c != '\t');
}

internal static class AuditMetadataAllowList
{
    public static IReadOnlySet<string> GetAllowedKeys(string action) =>
        action switch
        {
            AuditActions.AdministrationFeatureFlagCreated or AuditActions.AdministrationFeatureFlagUpdated or
            AuditActions.AdministrationFeatureFlagEnabled or AuditActions.AdministrationFeatureFlagDisabled =>
                new HashSet<string>(StringComparer.Ordinal) { "key", "previousState", "newState" },

            AuditActions.AdministrationSettingCreated or AuditActions.AdministrationSettingUpdated =>
                new HashSet<string>(StringComparer.Ordinal) { "key", "isPublic", "valueChanged" },

            AuditActions.AdministrationAnnouncementCreated or AuditActions.AdministrationAnnouncementUpdated or
            AuditActions.AdministrationAnnouncementPublished or AuditActions.AdministrationAnnouncementArchived =>
                new HashSet<string>(StringComparer.Ordinal) { "announcementId", "previousStatus", "newStatus" },

            AuditActions.ToolboxCategoryCreated or AuditActions.ToolboxCategoryUpdated or
            AuditActions.ToolboxCategoryActivated or AuditActions.ToolboxCategoryDeactivated or
            AuditActions.ToolboxToolCreated or AuditActions.ToolboxToolUpdated or
            AuditActions.ToolboxToolPublished or AuditActions.ToolboxToolUnpublished or
            AuditActions.ToolboxToolEnabled or AuditActions.ToolboxToolDisabled =>
                new HashSet<string>(StringComparer.Ordinal) { "toolId", "toolSlug", "previousState", "newState" },

            AuditActions.PromptLabCategoryCreated or AuditActions.PromptLabCategoryUpdated or
            AuditActions.PromptLabCategoryActivated or AuditActions.PromptLabCategoryDeactivated or
            AuditActions.PromptLabPromptCreated or AuditActions.PromptLabPromptUpdated or
            AuditActions.PromptLabVersionCreated or AuditActions.PromptLabVersionPublished or
            AuditActions.PromptLabPromptUnpublished or AuditActions.PromptLabPromptEnabled or
            AuditActions.PromptLabPromptDisabled =>
                new HashSet<string>(StringComparer.Ordinal) { "promptId", "promptSlug", "versionNumber", "previousState", "newState" },

            AuditActions.OutboxRetryRequested or AuditActions.OutboxDeadletterRecoveryRequested or
            AuditActions.OutboxMessageReprocessed =>
                new HashSet<string>(StringComparer.Ordinal) { "messageId", "operation", "previousStatus", "newStatus" },

            AuditActions.AuthenticationOtpVerificationFailed or AuditActions.AuthenticationLoginFailed =>
                new HashSet<string>(StringComparer.Ordinal) { "method", "failureCode" },

            AuditActions.AuthenticationOtpRequested or AuditActions.AuthenticationOtpVerified or
            AuditActions.AuthenticationLoginSucceeded or AuditActions.AuthenticationRateLimited =>
                new HashSet<string>(StringComparer.Ordinal) { "method" },

            AuditActions.SecurityRateLimitExceeded =>
                new HashSet<string>(StringComparer.Ordinal) { "policy" },

            AuditActions.AuthorizationAccessDenied =>
                new HashSet<string>(StringComparer.Ordinal) { "reasonCode" },

            AuditActions.ContentAiTaskRequested or AuditActions.ContentAiTaskFailed =>
                new HashSet<string>(StringComparer.Ordinal) { "taskType", "contentId", "failureCode" },

            AuditActions.SemanticSearchRequested or AuditActions.RagAnswerRequested =>
                new HashSet<string>(StringComparer.Ordinal) { "sourceCount", "errorCode" },

            AuditActions.LearningRecommendationRequested or AuditActions.LearningRoadmapGenerated =>
                new HashSet<string>(StringComparer.Ordinal) { "item_count", "generation_type" },

            _ => new HashSet<string>(StringComparer.Ordinal),
        };
}

public sealed class AuditRecorder : IAuditRecorder
{
    private const int MaxSubjectDisplayLength = 200;
    private const int MaxPathTemplateLength = 300;
    private const int MaxCorrelationIdLength = 100;

    private readonly IAuditRecordRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditMetadataSanitizer _sanitizer;
    private readonly IDateTimeProvider _clock;
    private readonly IOptions<AuditOptions> _options;
    private readonly IAuditPersistenceFailureInjector _failureInjector;
    private readonly ILogger<AuditRecorder> _logger;

    public AuditRecorder(
        IAuditRecordRepository repository,
        IUnitOfWork unitOfWork,
        IAuditMetadataSanitizer sanitizer,
        IDateTimeProvider clock,
        IOptions<AuditOptions> options,
        IAuditPersistenceFailureInjector failureInjector,
        ILogger<AuditRecorder> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _sanitizer = sanitizer;
        _clock = clock;
        _options = options;
        _failureInjector = failureInjector;
        _logger = logger;
    }

    public async Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!_options.Value.Enabled)
        {
            return;
        }

        try
        {
            _failureInjector.ThrowIfConfiguredToFail();

            var nowUtc = _clock.UtcNow;
            var metadata = _sanitizer.Sanitize(input.Action, input.Metadata);

            var record = AuditRecord.Create(
                Guid.NewGuid(),
                nowUtc,
                input.Category,
                input.Action,
                input.Outcome,
                input.ActorUserId,
                input.ActorType,
                input.SubjectId,
                input.SubjectType,
                input.SubjectDisplay,
                input.ReasonCode,
                input.CorrelationId,
                input.RequestMethod,
                input.RequestPathTemplate,
                metadata,
                nowUtc,
                _options.Value.MaxReasonLength,
                MaxSubjectDisplayLength,
                MaxPathTemplateLength,
                MaxCorrelationIdLength);

            await _repository.AddAsync(record, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(
                ex,
                "Event={Event} Action={Action} Category={Category}",
                "AuditPersistenceFailed",
                input.Action,
                input.Category);
        }
    }
}

public sealed class NoOpAuditRecorder : IAuditRecorder
{
    public Task RecordAsync(AuditRecordInput input, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
