using System.Text;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Search.Application.Rag;

public sealed class RagAnswerService : IRagAnswerService
{
    public const string TaskType = AiOperationNames.RagAnswer;

    private readonly IRagContextBuilder _contextBuilder;
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiUsageRecorder _usageRecorder;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<RagAnswerService> _logger;

    public RagAnswerService(
        IRagContextBuilder contextBuilder,
        IAiTextGenerator aiTextGenerator,
        IAiUsageRecorder usageRecorder,
        IAuditRecorder auditRecorder,
        IDateTimeProvider clock,
        ILogger<RagAnswerService> logger)
    {
        _contextBuilder = contextBuilder;
        _aiTextGenerator = aiTextGenerator;
        _usageRecorder = usageRecorder;
        _auditRecorder = auditRecorder;
        _clock = clock;
        _logger = logger;
    }

    public async Task<RagAnswerDto> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        var trimmed = (question ?? string.Empty).Trim();
        if (trimmed.Length is < 2 or > 1000)
        {
            throw new ArgumentException("Question length is invalid.", nameof(question));
        }

        var context = await _contextBuilder.BuildAsync(trimmed, cancellationToken);
        await TryAuditAsync(context.Chunks.Count, AuditOutcomes.Success, null, cancellationToken);

        if (context.Chunks.Count == 0)
        {
            return new RagAnswerDto(
                "در دانشنامه HelpDev مدرک مرتبطی برای این پرسش پیدا نشد.",
                [],
                _clock.UtcNow);
        }

        var assembled = AssembleContext(context);
        var result = await _aiTextGenerator.GenerateSafeAsync(
            new AiTextRequest(
                TaskType,
                """
                You answer using ONLY the provided HelpDev knowledge snippets.
                If the snippets are insufficient, say you do not know from HelpDev knowledge.
                Do not invent URLs, APIs, or facts. Do not browse the internet.
                Reply in the same language as the question when possible.
                """,
                $"Question:\n{trimmed}\n\nKnowledge:\n{assembled}",
                MaxTokens: 800),
            cancellationToken);

        await TryRecordUsageAsync(result, cancellationToken);

        if (!result.Success)
        {
            await TryAuditAsync(
                context.Chunks.Count,
                AuditOutcomes.Failure,
                result.ErrorCode,
                cancellationToken);

            _logger.LogWarning(
                "RAG answer generation failed. ErrorCode={ErrorCode} SourceCount={SourceCount}",
                result.ErrorCode,
                context.Chunks.Count);

            return new RagAnswerDto(
                "پاسخ‌گویی هوش مصنوعی در حال حاضر در دسترس نیست. لطفاً بعداً دوباره تلاش کنید.",
                context.Chunks
                    .Select(i => new RagSourceDto(i.Title, i.Url, i.SourceType, i.SourceId, i.Similarity))
                    .ToList(),
                _clock.UtcNow);
        }

        return new RagAnswerDto(
            result.Content!.Trim(),
            context.Chunks
                .Select(i => new RagSourceDto(i.Title, i.Url, i.SourceType, i.SourceId, i.Similarity))
                .ToList(),
            _clock.UtcNow);
    }

    private static string AssembleContext(RagContext context)
    {
        var sb = new StringBuilder();
        var n = 1;
        foreach (var item in context.Chunks)
        {
            sb.Append('[').Append(n++).Append("] ")
                .Append(item.Title)
                .Append(" (")
                .Append(item.SourceType)
                .Append(" · ")
                .Append(item.Url)
                .AppendLine(")")
                .AppendLine(item.Snippet)
                .AppendLine();
        }

        return sb.ToString();
    }

    private async Task TryRecordUsageAsync(AiGenerationResult result, CancellationToken cancellationToken)
    {
        try
        {
            await _usageRecorder.RecordAsync(
                new AiUsageRecordInput(
                    UserId: null,
                    TaskType,
                    result.Provider ?? "unknown",
                    result.Model ?? "unknown",
                    result.Usage?.InputTokens ?? 0,
                    result.Usage?.OutputTokens ?? 0,
                    ContentId: null,
                    result.Success,
                    (int)Math.Clamp(result.LatencyMs, 0, int.MaxValue),
                    result.ErrorCode),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAG usage recording skipped.");
        }
    }

    private async Task TryAuditAsync(
        int sourceCount,
        string outcome,
        string? errorCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceCount"] = sourceCount.ToString(),
            };
            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                metadata["errorCode"] = errorCode;
            }

            await _auditRecorder.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.SearchRag,
                    AuditActions.RagAnswerRequested,
                    outcome,
                    ActorUserId: null,
                    AuditActorTypes.Anonymous,
                    Metadata: metadata),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAG audit recording skipped.");
        }
    }
}
