using System.Diagnostics;
using System.Text.Json;
using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.Modules.PromptLab.Domain.Rendering;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.PromptLab.Application.Rendering;

public sealed class PromptRenderService : IPromptRenderService
{
    private readonly IPromptDefinitionRepository _promptRepository;
    private readonly IPromptCategoryRepository _categoryRepository;
    private readonly IPromptRenderRecordRepository _renderRepository;
    private readonly IPromptRenderer _renderer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAnalyticsEventIngestor _analyticsIngestor;
    private readonly ILogger<PromptRenderService> _logger;

    public PromptRenderService(
        IPromptDefinitionRepository promptRepository,
        IPromptCategoryRepository categoryRepository,
        IPromptRenderRecordRepository renderRepository,
        IPromptRenderer renderer,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAnalyticsEventIngestor analyticsIngestor,
        ILogger<PromptRenderService> logger)
    {
        _promptRepository = promptRepository;
        _categoryRepository = categoryRepository;
        _renderRepository = renderRepository;
        _renderer = renderer;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _analyticsIngestor = analyticsIngestor;
        _logger = logger;
    }

    public async Task<PromptRenderResultDto> RenderAsync(
        string slug,
        RenderPromptRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Values);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var prompt = await _promptRepository.GetBySlugAsync(normalizedSlug, cancellationToken);
        if (prompt is null || !prompt.IsPublished || prompt.PublishedVersionNumber is null)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        if (!prompt.IsEnabled)
        {
            throw new PromptLabException(
                "Prompt is disabled.",
                PromptLabApplicationErrorCodes.PromptDisabled);
        }

        if (RequiresAuthentication(prompt) && userId is null)
        {
            throw new PromptLabException(
                "Prompt requires authentication.",
                PromptLabApplicationErrorCodes.RenderRequiresAuthentication);
        }

        var category = await _categoryRepository.GetByIdAsync(prompt.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
        {
            throw new PromptLabException(
                "Prompt was not found.",
                PromptLabApplicationErrorCodes.PromptNotFound);
        }

        PromptVersion version;
        try
        {
            version = prompt.GetPublishedVersion();
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }

        var snapshot = new PromptVersionSnapshot(
            version.Id,
            version.VersionNumber,
            version.Template,
            version.Variables
                .Select(variable => new PromptVariableSnapshot(
                    variable.Name,
                    variable.Type,
                    variable.IsRequired,
                    variable.DefaultValue,
                    variable.MinLength,
                    variable.MaxLength,
                    variable.MinValue,
                    variable.MaxValue,
                    variable.ValidationPattern,
                    variable.AllowedValues.ToList()))
                .ToList());

        var stopwatch = Stopwatch.StartNew();
        var recordHistory = prompt.AllowHistory && userId.HasValue;
        Guid? renderId = null;

        try
        {
            var output = _renderer.Render(snapshot, request.Values);
            stopwatch.Stop();

            var duration = ToNonNegativeDuration(stopwatch.ElapsedMilliseconds);
            var renderedAt = _clock.UtcNow;

            if (recordHistory)
            {
                var record = PromptRenderRecord.Create(
                    Guid.NewGuid(),
                    prompt.Id,
                    version.Id,
                    version.VersionNumber,
                    userId!.Value,
                    succeeded: true,
                    duration,
                    BuildInputPreview(request.Values),
                    TruncatePreview(output.RenderedText, PromptLabLimits.MaxHistoryOutputPreview),
                    errorCode: null,
                    renderedAt);

                await _renderRepository.AddAsync(record, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                renderId = record.Id;
            }

            _logger.LogInformation(
                "PromptLab prompt rendered. Operation={Operation} PromptId={PromptId} PromptVersionId={PromptVersionId} VersionNumber={VersionNumber} Succeeded={Succeeded} DurationMs={DurationMs} Authenticated={Authenticated}",
                "prompt_rendered",
                prompt.Id,
                version.Id,
                version.VersionNumber,
                true,
                duration,
                userId.HasValue);

            await TryIngestRenderAsync(
                prompt,
                version,
                userId,
                succeeded: true,
                duration,
                errorCode: null,
                cancellationToken);

            return new PromptRenderResultDto(
                renderId,
                prompt.Slug.Value,
                version.VersionNumber,
                Succeeded: true,
                output.RenderedText,
                ErrorCode: null,
                ErrorMessage: null,
                duration,
                renderedAt);
        }
        catch (PromptLabException ex)
        {
            stopwatch.Stop();
            await TryRecordFailureAsync(
                prompt,
                version,
                userId,
                recordHistory,
                stopwatch.ElapsedMilliseconds,
                request.Values,
                ex.Code,
                cancellationToken);
            throw;
        }
        catch (DomainException ex)
        {
            stopwatch.Stop();
            var wrapped = Wrap(ex);
            await TryRecordFailureAsync(
                prompt,
                version,
                userId,
                recordHistory,
                stopwatch.ElapsedMilliseconds,
                request.Values,
                wrapped.Code,
                cancellationToken);
            throw wrapped;
        }
    }

    private async Task TryRecordFailureAsync(
        PromptDefinition prompt,
        PromptVersion version,
        Guid? userId,
        bool recordHistory,
        long elapsedMilliseconds,
        IReadOnlyDictionary<string, JsonElement> values,
        string errorCode,
        CancellationToken cancellationToken)
    {
        var duration = ToNonNegativeDuration(elapsedMilliseconds);

        _logger.LogInformation(
            "PromptLab prompt render failed. Operation={Operation} PromptId={PromptId} PromptVersionId={PromptVersionId} VersionNumber={VersionNumber} Succeeded={Succeeded} DurationMs={DurationMs} ErrorCode={ErrorCode} Authenticated={Authenticated}",
            "prompt_render_failed",
            prompt.Id,
            version.Id,
            version.VersionNumber,
            false,
            duration,
            errorCode,
            userId.HasValue);

        if (!recordHistory || userId is null)
        {
            await TryIngestRenderAsync(
                prompt,
                version,
                userId,
                succeeded: false,
                duration,
                errorCode,
                cancellationToken);
            return;
        }

        try
        {
            var record = PromptRenderRecord.Create(
                Guid.NewGuid(),
                prompt.Id,
                version.Id,
                version.VersionNumber,
                userId.Value,
                succeeded: false,
                duration,
                BuildInputPreview(values),
                renderedPreview: null,
                errorCode,
                _clock.UtcNow);

            await _renderRepository.AddAsync(record, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DomainException)
        {
            // History persistence must not mask the original render failure.
        }

        await TryIngestRenderAsync(
            prompt,
            version,
            userId,
            succeeded: false,
            duration,
            errorCode,
            cancellationToken);
    }

    private static bool RequiresAuthentication(PromptDefinition prompt) =>
        prompt.Visibility == PromptVisibility.Authenticated || prompt.RequiresAuthentication;

    private static string? BuildInputPreview(IReadOnlyDictionary<string, JsonElement> values)
    {
        try
        {
            var keys = string.Join(',', values.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase));
            return TruncatePreview($"keys:{keys}", PromptLabLimits.MaxHistoryInputPreview);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static string? TruncatePreview(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= maxLength)
        {
            return trimmed;
        }

        return trimmed[..maxLength];
    }

    private static int ToNonNegativeDuration(long elapsedMilliseconds)
    {
        if (elapsedMilliseconds <= 0)
        {
            return 0;
        }

        return elapsedMilliseconds > int.MaxValue
            ? int.MaxValue
            : (int)elapsedMilliseconds;
    }

    private static PromptLabException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? PromptLabApplicationErrorCodes.RenderFailed, ex);

    private async Task TryIngestRenderAsync(
        PromptDefinition prompt,
        PromptVersion version,
        Guid? userId,
        bool succeeded,
        int duration,
        string? errorCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var dimensions = new Dictionary<string, string>
            {
                [AnalyticsDimensionKeys.Purpose] = prompt.Purpose.ToString(),
                [AnalyticsDimensionKeys.IsAuthenticated] = userId.HasValue ? "true" : "false",
                [AnalyticsDimensionKeys.VersionNumber] = version.VersionNumber.ToString(),
            };

            if (!string.IsNullOrWhiteSpace(prompt.Slug.Value))
            {
                dimensions[AnalyticsDimensionKeys.PromptSlug] = prompt.Slug.Value;
            }

            if (!succeeded && !string.IsNullOrWhiteSpace(errorCode))
            {
                dimensions[AnalyticsDimensionKeys.ErrorCode] = errorCode;
            }

            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    succeeded
                        ? AnalyticsEventTypes.PromptLabRenderSucceeded
                        : AnalyticsEventTypes.PromptLabRenderFailed,
                    _clock.UtcNow,
                    userId,
                    prompt.Id,
                    "Prompt",
                    dimensions,
                    DurationMilliseconds: duration,
                    SubjectDisplayName: prompt.Name,
                    SubjectSlug: prompt.Slug.Value),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics prompt render ingestion skipped.");
        }
    }
}
