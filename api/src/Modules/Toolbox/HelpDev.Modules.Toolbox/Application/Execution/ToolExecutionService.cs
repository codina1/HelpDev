using System.Diagnostics;
using System.Text.Json;
using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Execution;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Toolbox.Application.Execution;

public sealed class ToolExecutionService : IToolExecutionService
{
    private readonly IToolDefinitionRepository _toolRepository;
    private readonly IToolExecutionRecordRepository _executionRepository;
    private readonly IToolExecutorRegistry _executorRegistry;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAnalyticsEventIngestor _analyticsIngestor;
    private readonly ILogger<ToolExecutionService> _logger;

    public ToolExecutionService(
        IToolDefinitionRepository toolRepository,
        IToolExecutionRecordRepository executionRepository,
        IToolExecutorRegistry executorRegistry,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IAnalyticsEventIngestor analyticsIngestor,
        ILogger<ToolExecutionService> logger)
    {
        _toolRepository = toolRepository;
        _executionRepository = executionRepository;
        _executorRegistry = executorRegistry;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _analyticsIngestor = analyticsIngestor;
        _logger = logger;
    }

    public async Task<ToolExecutionResultDto> ExecuteAsync(
        string slug,
        ExecuteToolRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ToolboxException(
                "Tool was not found.",
                ToolboxApplicationErrorCodes.ToolNotFound);
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var tool = await _toolRepository.GetBySlugAsync(normalizedSlug, cancellationToken);
        if (tool is null || !tool.IsPublished)
        {
            throw new ToolboxException(
                "Tool was not found.",
                ToolboxApplicationErrorCodes.ToolNotFound);
        }

        if (!tool.IsEnabled)
        {
            throw new ToolboxException(
                "Tool is disabled.",
                ToolboxApplicationErrorCodes.ToolDisabled);
        }

        if (tool.RequiresAuthentication && userId is null)
        {
            throw new ToolboxException(
                "Tool requires authentication.",
                ToolboxApplicationErrorCodes.ToolRequiresAuthentication);
        }

        ValidateInputSize(request.Input);

        var executor = _executorRegistry.GetRequired(tool.Type);
        var stopwatch = Stopwatch.StartNew();
        var recordHistory = tool.AllowHistory && userId.HasValue;
        Guid? executionId = null;

        try
        {
            var output = await executor.ExecuteAsync(
                new ToolExecutionInput(request.Input),
                cancellationToken);
            stopwatch.Stop();

            var duration = ToNonNegativeDuration(stopwatch.ElapsedMilliseconds);
            var completedAt = _clock.UtcNow;

            if (recordHistory)
            {
                var record = ToolExecutionRecord.Create(
                    Guid.NewGuid(),
                    tool.Id,
                    userId!.Value,
                    tool.Type,
                    succeeded: true,
                    duration,
                    TruncatePreview(request.Input.GetRawText()),
                    TruncatePreview(output.Payload.GetRawText()),
                    errorCode: null,
                    completedAt);

                await _executionRepository.AddAsync(record, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                executionId = record.Id;

                _logger.LogInformation(
                    "Toolbox tool executed. Operation={Operation} ToolId={ToolId} Slug={Slug} ExecutionId={ExecutionId} DurationMs={DurationMs}",
                    "tool_executed",
                    tool.Id,
                    tool.Slug.Value,
                    executionId,
                    duration);
            }

            await TryIngestExecutionAsync(
                tool,
                userId,
                succeeded: true,
                duration,
                errorCode: null,
                cancellationToken);

            return new ToolExecutionResultDto(
                executionId,
                tool.Slug.Value,
                tool.Type.ToString(),
                Succeeded: true,
                output.Payload,
                ErrorCode: null,
                ErrorMessage: null,
                duration,
                output.IsTruncated,
                completedAt);
        }
        catch (ToolboxException ex)
        {
            stopwatch.Stop();
            var duration = ToNonNegativeDuration(stopwatch.ElapsedMilliseconds);

            if (recordHistory)
            {
                try
                {
                    var record = ToolExecutionRecord.Create(
                        Guid.NewGuid(),
                        tool.Id,
                        userId!.Value,
                        tool.Type,
                        succeeded: false,
                        duration,
                        TruncatePreview(SafeRawText(request.Input)),
                        outputPreview: null,
                        ex.Code,
                        _clock.UtcNow);

                    await _executionRepository.AddAsync(record, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (DomainException)
                {
                    // History persistence must not mask the original execution failure.
                }
            }

            await TryIngestExecutionAsync(
                tool,
                userId,
                succeeded: false,
                duration,
                ex.Code,
                cancellationToken);

            throw;
        }
        catch (DomainException ex)
        {
            throw Wrap(ex);
        }
    }

    private static void ValidateInputSize(JsonElement input)
    {
        if (input.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ToolboxException(
                "Execution input is invalid.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        string raw;
        try
        {
            raw = input.GetRawText();
        }
        catch (InvalidOperationException)
        {
            throw new ToolboxException(
                "Execution input is invalid.",
                ToolboxApplicationErrorCodes.ExecutionInputInvalid);
        }

        if (raw.Length > ToolboxLimits.MaxRequestBytes || raw.Length > ToolboxLimits.MaxJsonLength)
        {
            throw new ToolboxException(
                "Execution input is too large.",
                ToolboxApplicationErrorCodes.ExecutionInputTooLarge);
        }
    }

    private static string? TruncatePreview(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= ToolboxLimits.MaxHistoryInputPreview)
        {
            return trimmed;
        }

        return trimmed[..ToolboxLimits.MaxHistoryInputPreview];
    }

    private static string? SafeRawText(JsonElement element)
    {
        try
        {
            return element.GetRawText();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
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

    private static ToolboxException Wrap(DomainException ex) =>
        new(ex.Message, ex.Code ?? ToolboxApplicationErrorCodes.ExecutionFailed, ex);

    private async Task TryIngestExecutionAsync(
        ToolDefinition tool,
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
                [AnalyticsDimensionKeys.ToolType] = tool.Type.ToString(),
                [AnalyticsDimensionKeys.IsAuthenticated] = userId.HasValue ? "true" : "false",
            };

            if (!string.IsNullOrWhiteSpace(tool.Slug.Value))
            {
                dimensions[AnalyticsDimensionKeys.ToolSlug] = tool.Slug.Value;
            }

            if (!succeeded && !string.IsNullOrWhiteSpace(errorCode))
            {
                dimensions[AnalyticsDimensionKeys.ErrorCode] = errorCode;
            }

            await _analyticsIngestor.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    succeeded
                        ? AnalyticsEventTypes.ToolboxExecutionSucceeded
                        : AnalyticsEventTypes.ToolboxExecutionFailed,
                    _clock.UtcNow,
                    userId,
                    tool.Id,
                    "Tool",
                    dimensions,
                    DurationMilliseconds: duration,
                    SubjectDisplayName: tool.Name,
                    SubjectSlug: tool.Slug.Value),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Analytics toolbox execution ingestion skipped.");
        }
    }
}
