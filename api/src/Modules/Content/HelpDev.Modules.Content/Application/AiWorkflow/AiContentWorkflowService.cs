using System.Text;
using System.Text.Json;
using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Application.Contents.Revisions;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Domain.AiWorkflow;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Exceptions;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Content.Application.AiWorkflow;

public sealed class AiContentWorkflowService : IAiContentWorkflowService, IAiResearchService
{
    private readonly IContentIdeaRepository _ideaRepository;
    private readonly IAiContentWorkflowSessionRepository _sessionRepository;
    private readonly IWorkflowKnowledgeRetriever _knowledgeRetriever;
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IContentAiFeatureGate _featureGate;
    private readonly IContentSeoAnalyzer _seoAnalyzer;
    private readonly IContentService _contentService;
    private readonly IContentRepository _contentRepository;
    private readonly IContentRevisionService _revisionService;
    private readonly IAiUsageRecorder _usageRecorder;
    private readonly IAuditRecorder _auditRecorder;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<AiContentWorkflowService> _logger;

    public AiContentWorkflowService(
        IContentIdeaRepository ideaRepository,
        IAiContentWorkflowSessionRepository sessionRepository,
        IWorkflowKnowledgeRetriever knowledgeRetriever,
        IAiTextGenerator aiTextGenerator,
        IContentAiFeatureGate featureGate,
        IContentSeoAnalyzer seoAnalyzer,
        IContentService contentService,
        IContentRepository contentRepository,
        IContentRevisionService revisionService,
        IAiUsageRecorder usageRecorder,
        IAuditRecorder auditRecorder,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<AiContentWorkflowService> logger)
    {
        _ideaRepository = ideaRepository;
        _sessionRepository = sessionRepository;
        _knowledgeRetriever = knowledgeRetriever;
        _aiTextGenerator = aiTextGenerator;
        _featureGate = featureGate;
        _seoAnalyzer = seoAnalyzer;
        _contentService = contentService;
        _contentRepository = contentRepository;
        _revisionService = revisionService;
        _usageRecorder = usageRecorder;
        _auditRecorder = auditRecorder;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<AiContentWorkflowSessionDto> CreateAsync(
        ContentManagementActor actor,
        CreateAiContentWorkflowRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(request);
        EnsureAiEnabled();

        var now = _clock.UtcNow;
        try
        {
            var idea = ContentIdea.Create(
                Guid.NewGuid(),
                request.Title,
                request.Description ?? string.Empty,
                request.TargetType ?? "Article",
                actor.UserId,
                now);
            var session = AiContentWorkflowSession.Create(Guid.NewGuid(), idea.Id, actor.UserId, now);

            await _ideaRepository.AddAsync(idea, cancellationToken);
            await _sessionRepository.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return AiWorkflowMapper.ToDto(session, idea);
        }
        catch (DomainException ex)
        {
            throw new ContentAiException(ex.Message, ContentAiErrorCodes.TaskNotAllowed);
        }
    }

    public async Task<IReadOnlyList<AiContentWorkflowListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var createdBy = actor.CanManageAllContent ? (Guid?)null : actor.UserId;
        var sessions = await _sessionRepository.ListByCreatorAsync(createdBy, cancellationToken);
        var items = new List<AiContentWorkflowListItemDto>(sessions.Count);
        foreach (var session in sessions)
        {
            var idea = await _ideaRepository.GetByIdAsync(session.IdeaId, cancellationToken);
            if (idea is null)
            {
                continue;
            }

            items.Add(AiWorkflowMapper.ToListItem(session, idea));
        }

        return items;
    }

    public async Task<AiContentWorkflowSessionDto> GetByIdAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken = default)
    {
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);
        return AiWorkflowMapper.ToDto(session, idea);
    }

    public Task<AiResearchResultDto> ResearchAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken = default) =>
        ((IAiResearchService)this).ResearchAsync(actor, workflowId, cancellationToken);

    async Task<AiResearchResultDto> IAiResearchService.ResearchAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);
        EnsureAiEnabled();

        var topic = string.IsNullOrWhiteSpace(idea.Description)
            ? idea.Title
            : $"{idea.Title}\n{idea.Description}";

        var knowledge = await _knowledgeRetriever.RetrieveAsync(topic, take: 8, cancellationToken);
        var knowledgeBlock = AssembleKnowledge(knowledge);
        var response = await GenerateAsync(
            actor.UserId,
            AiWorkflowTaskTypes.Research,
            """
            You summarize HelpDev knowledge for an editorial research brief.
            Use ONLY the provided knowledge snippets. If insufficient, say so clearly.
            Do not invent URLs or facts. No confidence scores.
            """,
            $"Topic:\n{topic}\n\nKnowledge:\n{knowledgeBlock}",
            contentId: null,
            cancellationToken);

        idea.MarkResearching(_clock.UtcNow);
        session.AdvanceTo(AiContentWorkflowStep.Research, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AiResearchResultDto(
            response.Text.Trim(),
            knowledge.Sources
                .Select(s => new AiResearchSourceDto(s.Title, s.Url, s.SourceType, s.Snippet))
                .ToList(),
            response.Model,
            response.Provider,
            _clock.UtcNow);
    }

    public async Task<ContentOutlineDto> GenerateOutlineAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateOutlineRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);
        EnsureAiEnabled();

        var research = string.IsNullOrWhiteSpace(request.ResearchSummary)
            ? "(no research summary provided)"
            : request.ResearchSummary.Trim();

        var response = await GenerateAsync(
            actor.UserId,
            AiWorkflowTaskTypes.Outline,
            """
            Produce a markdown article outline for HelpDev.
            Include a proposed title as the first line (# Title).
            Then H2/H3 sections only. No full paragraphs. No confidence scores.
            """,
            $"Topic: {idea.Title}\nType: {idea.TargetType}\n\nResearch:\n{research}",
            contentId: null,
            cancellationToken);

        var parsed = ParseOutline(response.Text, idea.Title);
        idea.MarkWriting(_clock.UtcNow);
        session.AdvanceTo(AiContentWorkflowStep.Outline, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return parsed with
        {
            Model = response.Model,
            Provider = response.Provider,
            CreatedAtUtc = _clock.UtcNow,
        };
    }

    public async Task<DraftSuggestionDto> GenerateDraftAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);
        EnsureAiEnabled();

        if (string.IsNullOrWhiteSpace(request.OutlineText))
        {
            throw new ContentAiException("Outline is required.", ContentAiErrorCodes.TaskNotAllowed);
        }

        var response = await GenerateAsync(
            actor.UserId,
            AiWorkflowTaskTypes.Draft,
            """
            Write a HelpDev article draft in Markdown from the outline.
            Do not invent product claims. Keep a practical developer tone.
            Do not include SEO scores or ranking claims.
            """,
            $"Topic: {idea.Title}\nOutline title: {request.OutlineTitle}\n\nOutline:\n{request.OutlineText}",
            contentId: null,
            cancellationToken);

        session.AdvanceTo(AiContentWorkflowStep.Draft, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var title = string.IsNullOrWhiteSpace(request.OutlineTitle) ? idea.Title : request.OutlineTitle.Trim();
        return new DraftSuggestionDto(
            title,
            response.Text.Trim(),
            response.Model,
            response.Provider,
            _clock.UtcNow);
    }

    public async Task<SeoOptimizationSuggestionDto> GenerateSeoAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateSeoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);
        EnsureAiEnabled();

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Title)
            : request.Slug.Trim();

        var analysis = _seoAnalyzer.Analyze(
            new SeoAnalysisInput(
                request.Title,
                slug,
                request.Body,
                string.Empty,
                null,
                idea.TargetType,
                null,
                null,
                null,
                null,
                request.FocusKeyword),
            _clock.UtcNow);

        var recommendations = analysis.Findings
            .Where(f => !f.Passed)
            .Select(f => f.Recommendation)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.Ordinal)
            .Take(12)
            .ToList()!;

        var keywords = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.FocusKeyword))
        {
            keywords.Add(request.FocusKeyword.Trim());
        }

        keywords.AddRange(
            idea.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(w => w.Length > 3)
                .Take(5));

        string? suggestedTitle = null;
        string? suggestedDescription = null;

        try
        {
            var response = await GenerateAsync(
                actor.UserId,
                AiWorkflowTaskTypes.Seo,
                """
                Suggest SEO title and meta description for HelpDev content.
                Reply as JSON only: {"title":"...","description":"...","keywords":["..."]}
                No scores. No ranking predictions.
                """,
                $"Title: {request.Title}\nBody excerpt:\n{Truncate(request.Body, 1200)}",
                contentId: session.LinkedContentId,
                cancellationToken);

            ParseSeoJson(response.Text, out suggestedTitle, out suggestedDescription, out var aiKeywords);
            keywords.AddRange(aiKeywords);
        }
        catch (ContentAiException)
        {
            // SEO analyzer suggestions still returned when AI SEO step fails.
        }

        session.AdvanceTo(AiContentWorkflowStep.Seo, _clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SeoOptimizationSuggestionDto(
            suggestedTitle,
            suggestedDescription,
            keywords.Distinct(StringComparer.OrdinalIgnoreCase).Take(10).ToList(),
            recommendations!,
            _clock.UtcNow);
    }

    public async Task<ApplyDraftResultDto> ApplyDraftAsync(
        ContentManagementActor actor,
        Guid workflowId,
        ApplyDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var (session, idea) = await GetManagedAsync(actor, workflowId, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ContentAiException("Title and body are required to apply.", ContentAiErrorCodes.TaskNotAllowed);
        }

        if (session.LinkedContentId.HasValue)
        {
            throw new ContentAiException("Draft already applied for this workflow.", ContentAiErrorCodes.TaskNotAllowed);
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Title)
            : request.Slug.Trim();
        var type = string.IsNullOrWhiteSpace(request.TargetType) ? idea.TargetType : request.TargetType.Trim();

        var created = await _contentService.CreateAsync(
            actor.UserId,
            new CreateContentRequest
            {
                Title = request.Title.Trim(),
                Slug = slug,
                Body = request.Body,
                Type = type,
                Status = ContentStatus.Draft.ToString(),
            },
            cancellationToken);

        var content = await _contentRepository.GetByIdAsync(created.Id, cancellationToken)
            ?? throw new ContentAiException("Content was not found after create.", ContentAiErrorCodes.NotFound);

        await _revisionService.AppendRevisionAsync(
            content,
            actor.UserId,
            "AI workflow draft applied",
            cancellationToken);

        session.LinkContent(content.Id, _clock.UtcNow);
        idea.MarkReview(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApplyDraftResultDto(
            session.Id,
            content.Id,
            RevisionVersion: 1,
            ContentStatus: content.Status.ToString());
    }

    private async Task<(AiContentWorkflowSession Session, ContentIdea Idea)> GetManagedAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var session = await _sessionRepository.GetByIdAsync(workflowId, cancellationToken);
        if (session is null)
        {
            throw new ContentAiException("Workflow was not found.", ContentAiErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(session.CreatedByUserId, actor);

        var idea = await _ideaRepository.GetByIdAsync(session.IdeaId, cancellationToken);
        if (idea is null)
        {
            throw new ContentAiException("Idea was not found.", ContentAiErrorCodes.NotFound);
        }

        return (session, idea);
    }

    private void EnsureAiEnabled()
    {
        if (!_featureGate.IsEnabled)
        {
            throw new ContentAiException("دستیار هوش مصنوعی غیرفعال است.", ContentAiErrorCodes.Disabled);
        }
    }

    private async Task<AiTextResponse> GenerateAsync(
        Guid userId,
        string taskType,
        string system,
        string input,
        Guid? contentId,
        CancellationToken cancellationToken)
    {
        await TryAuditAsync(taskType, userId, cancellationToken);

        var result = await _aiTextGenerator.GenerateSafeAsync(
            new AiTextRequest(taskType, system, input, MaxTokens: 1600),
            cancellationToken);

        await TryRecordUsageAsync(userId, taskType, result, contentId, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "AI workflow step failed. TaskType={TaskType} ErrorCode={ErrorCode}",
                taskType,
                result.ErrorCode);
            throw new ContentAiException(
                "تولید هوش مصنوعی ناموفق بود.",
                result.ErrorCode == AiErrorCodes.Disabled
                    ? ContentAiErrorCodes.Disabled
                    : ContentAiErrorCodes.ProviderFailed);
        }

        return new AiTextResponse(
            result.Content!,
            result.Model ?? "unknown",
            result.Provider ?? "unknown",
            result.Usage);
    }

    private async Task TryRecordUsageAsync(
        Guid userId,
        string taskType,
        AiGenerationResult result,
        Guid? contentId,
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
            _logger.LogWarning(ex, "AI usage recording skipped.");
        }
    }

    private async Task TryAuditAsync(string taskType, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _auditRecorder.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.ContentAi,
                    AuditActions.ContentAiTaskRequested,
                    AuditOutcomes.Success,
                    userId,
                    AuditActorTypes.User,
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["taskType"] = taskType,
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI workflow audit skipped.");
        }
    }

    private static string AssembleKnowledge(WorkflowKnowledgeContext knowledge)
    {
        if (knowledge.Sources.Count == 0)
        {
            return "(no knowledge snippets retrieved)";
        }

        var sb = new StringBuilder();
        var n = 1;
        foreach (var source in knowledge.Sources)
        {
            sb.Append('[').Append(n++).Append("] ")
                .Append(source.Title)
                .Append(" (")
                .Append(source.SourceType)
                .Append(" · ")
                .Append(source.Url)
                .AppendLine(")")
                .AppendLine(source.Snippet)
                .AppendLine();
        }

        return sb.ToString();
    }

    private static ContentOutlineDto ParseOutline(string raw, string fallbackTitle)
    {
        var lines = (raw ?? string.Empty).Replace("\r\n", "\n").Split('\n');
        var title = fallbackTitle;
        var sections = new List<ContentOutlineSectionDto>();
        string? currentHeading = null;
        var subheadings = new List<string>();

        void Flush()
        {
            if (currentHeading is null)
            {
                return;
            }

            sections.Add(new ContentOutlineSectionDto(currentHeading, subheadings.ToList()));
            subheadings.Clear();
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("# ", StringComparison.Ordinal) && !trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                title = trimmed[2..].Trim();
                continue;
            }

            if (trimmed.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                currentHeading = trimmed[3..].Trim();
                continue;
            }

            if (trimmed.StartsWith("### ", StringComparison.Ordinal) && currentHeading is not null)
            {
                subheadings.Add(trimmed[4..].Trim());
            }
        }

        Flush();
        if (sections.Count == 0)
        {
            sections.Add(new ContentOutlineSectionDto("Introduction", []));
        }

        return new ContentOutlineDto(title, sections, raw.Trim(), string.Empty, string.Empty, DateTime.UtcNow);
    }

    private static void ParseSeoJson(
        string text,
        out string? title,
        out string? description,
        out List<string> keywords)
    {
        title = null;
        description = null;
        keywords = [];
        var json = ExtractJsonObject(text);
        if (json is null)
        {
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
            {
                title = t.GetString();
            }

            if (doc.RootElement.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String)
            {
                description = d.GetString();
            }

            if (doc.RootElement.TryGetProperty("keywords", out var k) && k.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in k.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var value = item.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            keywords.Add(value.Trim());
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Ignore malformed AI JSON; analyzer recommendations still apply.
        }
    }

    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return text[start..(end + 1)];
    }

    private static string Slugify(string title)
    {
        var sb = new StringBuilder();
        foreach (var ch in title.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
            }
            else if (ch is ' ' or '-' or '_')
            {
                if (sb.Length > 0 && sb[^1] != '-')
                {
                    sb.Append('-');
                }
            }
        }

        var slug = sb.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"draft-{Guid.NewGuid():N}"[..20] : slug[..Math.Min(slug.Length, 80)];
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
