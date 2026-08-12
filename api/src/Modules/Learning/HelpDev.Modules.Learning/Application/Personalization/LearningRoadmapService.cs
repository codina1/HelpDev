using System.Text;
using System.Text.Json;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Personalization;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Learning.Application.Personalization;

public sealed class LearningRoadmapService : ILearningRoadmapService
{
    private readonly ILearningRoadmapRepository _roadmapRepository;
    private readonly ILearningProfileService _profileService;
    private readonly ILearningSignalsService _signalsService;
    private readonly ILearningDbContext _db;
    private readonly ILearningKnowledgeRetriever _knowledgeRetriever;
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiUsageRecorder _usageRecorder;
    private readonly IAnalyticsEventIngestor _analytics;
    private readonly IAuditRecorder _audit;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<LearningRoadmapService> _logger;

    public LearningRoadmapService(
        ILearningRoadmapRepository roadmapRepository,
        ILearningProfileService profileService,
        ILearningSignalsService signalsService,
        ILearningDbContext db,
        ILearningKnowledgeRetriever knowledgeRetriever,
        IAiTextGenerator aiTextGenerator,
        IAiUsageRecorder usageRecorder,
        IAnalyticsEventIngestor analytics,
        IAuditRecorder audit,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<LearningRoadmapService> logger)
    {
        _roadmapRepository = roadmapRepository;
        _profileService = profileService;
        _signalsService = signalsService;
        _db = db;
        _knowledgeRetriever = knowledgeRetriever;
        _aiTextGenerator = aiTextGenerator;
        _usageRecorder = usageRecorder;
        _analytics = analytics;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LearningRoadmapDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roadmap = await _roadmapRepository.GetByUserIdAsync(userId, cancellationToken);
        return roadmap is null ? null : Map(roadmap);
    }

    public async Task<LearningRoadmapDto> GenerateAsync(
        Guid userId,
        GenerateLearningRoadmapRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var profile = await _profileService.GetAsync(userId, cancellationToken);
        var signals = await _signalsService.GetAsync(userId, cancellationToken);

        var goal = string.IsNullOrWhiteSpace(request.Goal)
            ? (string.IsNullOrWhiteSpace(profile.LearningGoals) ? "Become a better HelpDev engineer" : profile.LearningGoals)
            : request.Goal!.Trim();

        var knowledge = await _knowledgeRetriever.RetrieveAsync(goal, take: 8, cancellationToken);
        var publishedRows = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Status == CourseStatus.Published)
            .Select(c => new { c.Id, c.Title, Slug = c.Slug.Value })
            .ToListAsync(cancellationToken);
        var published = publishedRows
            .Select(c => (c.Id, c.Title, c.Slug))
            .ToList();

        var steps = await GenerateStepsAsync(userId, goal, profile, signals, knowledge, published, cancellationToken);
        var now = _clock.UtcNow;
        var existing = await _roadmapRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            var created = LearningRoadmap.CreateSuggested(Guid.NewGuid(), userId, goal, steps, now);
            await _roadmapRepository.AddAsync(created, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await TryTrackAsync(userId, created.Steps.Count, cancellationToken);
            return Map(created);
        }

        existing.ReplaceSuggestion(goal, steps, now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await TryTrackAsync(userId, existing.Steps.Count, cancellationToken);
        return Map(existing);
    }

    public async Task<LearningRoadmapDto> ApproveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roadmap = await _roadmapRepository.GetByUserIdAsync(userId, cancellationToken);
        if (roadmap is null)
        {
            throw new LearningPersonalizationException(
                "نقشه راهی برای تأیید وجود ندارد.",
                LearningPersonalizationErrorCodes.RoadmapNotFound);
        }

        roadmap.Approve(_clock.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(roadmap);
    }

    private async Task<IReadOnlyList<LearningRoadmapStepInput>> GenerateStepsAsync(
        Guid userId,
        string goal,
        LearningProfileDto profile,
        LearningSignalsDto signals,
        LearningKnowledgeContext knowledge,
        IReadOnlyList<(Guid Id, string Title, string Slug)> catalog,
        CancellationToken cancellationToken)
    {
        var fallback = BuildFallbackSteps(goal, profile, catalog);

        var sb = new StringBuilder();
        sb.AppendLine($"Goal: {goal}");
        sb.AppendLine($"Experience: {profile.ExperienceLevel}");
        sb.AppendLine($"Skills: {profile.CurrentSkills}");
        sb.AppendLine($"Topics: {string.Join(", ", profile.PreferredTopics.Select(t => t.Topic))}");
        sb.AppendLine($"Completed lessons: {signals.CompletedLessonCount}");
        sb.AppendLine("Catalog courses:");
        foreach (var course in catalog.Take(20))
        {
            sb.AppendLine($"- {course.Title} ({course.Id:D})");
        }

        sb.AppendLine("Knowledge:");
        foreach (var source in knowledge.Sources.Take(8))
        {
            sb.AppendLine($"- {source.Title}: {source.Snippet}");
        }

        var result = await _aiTextGenerator.GenerateSafeAsync(
            new AiTextRequest(
                LearningAiTaskTypes.Roadmap,
                """
                Suggest a learning roadmap for HelpDev. Use ONLY provided catalog/knowledge.
                Do not enroll users or invent URLs. No confidence scores.
                Return JSON only:
                {"steps":[{"title":"...","description":"...","relatedCourseId":null}]}
                3 to 8 steps. relatedCourseId may be a catalog course UUID or null.
                """,
                sb.ToString(),
                MaxTokens: 900),
            cancellationToken);

        await TryRecordUsageAsync(userId, result, cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Content))
        {
            return fallback;
        }

        var parsed = TryParseSteps(result.Content!, catalog);
        return parsed.Count > 0 ? parsed : fallback;
    }

    private static IReadOnlyList<LearningRoadmapStepInput> BuildFallbackSteps(
        string goal,
        LearningProfileDto profile,
        IReadOnlyList<(Guid Id, string Title, string Slug)> catalog)
    {
        var steps = new List<LearningRoadmapStepInput>
        {
            new("تقویت مبانی", $"سطح فعلی: {profile.ExperienceLevel}. مهارت‌ها را مرور کنید.", null),
            new("تمرکز روی هدف", goal, null),
        };

        foreach (var topic in profile.PreferredTopics.OrderByDescending(t => t.Priority).Take(3))
        {
            var match = catalog.FirstOrDefault(c =>
                c.Title.Contains(topic.Topic, StringComparison.OrdinalIgnoreCase));
            steps.Add(new LearningRoadmapStepInput(
                $"مسیر {topic.Topic}",
                match.Title is null
                    ? $"موضوع ترجیحی: {topic.Topic}"
                    : $"دوره مرتبط در کاتالوگ: {match.Title}",
                match.Id == Guid.Empty ? null : match.Id));
        }

        steps.Add(new LearningRoadmapStepInput("پروژه عملی", "آموخته‌ها را در یک پروژه کوچک HelpDev تمرین کنید.", null));
        return steps;
    }

    private static IReadOnlyList<LearningRoadmapStepInput> TryParseSteps(
        string content,
        IReadOnlyList<(Guid Id, string Title, string Slug)> catalog)
    {
        try
        {
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return [];
            }

            using var doc = JsonDocument.Parse(content[jsonStart..(jsonEnd + 1)]);
            if (!doc.RootElement.TryGetProperty("steps", out var stepsEl) || stepsEl.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var catalogIds = catalog.Select(c => c.Id).ToHashSet();
            var steps = new List<LearningRoadmapStepInput>();
            foreach (var el in stepsEl.EnumerateArray().Take(8))
            {
                var title = el.TryGetProperty("title", out var t) ? t.GetString() : null;
                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                var description = el.TryGetProperty("description", out var d) ? d.GetString() : null;
                Guid? related = null;
                if (el.TryGetProperty("relatedCourseId", out var c)
                    && c.ValueKind == JsonValueKind.String
                    && Guid.TryParse(c.GetString(), out var courseId)
                    && catalogIds.Contains(courseId))
                {
                    related = courseId;
                }

                steps.Add(new LearningRoadmapStepInput(title!, description, related));
            }

            return steps;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static LearningRoadmapDto Map(LearningRoadmap roadmap) =>
        new(
            roadmap.Id,
            roadmap.Goal,
            roadmap.Status.ToString(),
            roadmap.Steps
                .OrderBy(s => s.StepOrder)
                .Select(s => new LearningRoadmapStepDto(s.StepOrder, s.Title, s.Description, s.RelatedCourseId))
                .ToList(),
            roadmap.CreatedAtUtc,
            roadmap.UpdatedAtUtc,
            roadmap.ApprovedAtUtc);

    private async Task TryTrackAsync(Guid userId, int itemCount, CancellationToken cancellationToken)
    {
        try
        {
            await _analytics.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.LearningRoadmapGenerated,
                    _clock.UtcNow,
                    userId,
                    SubjectId: null,
                    SubjectType: null,
                    Dimensions: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["item_count"] = itemCount.ToString(),
                        ["generation_type"] = "roadmap",
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Learning roadmap analytics skipped.");
        }

        try
        {
            await _audit.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.LearningAi,
                    AuditActions.LearningRoadmapGenerated,
                    AuditOutcomes.Success,
                    userId,
                    AuditActorTypes.User,
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["item_count"] = itemCount.ToString(),
                        ["generation_type"] = "roadmap",
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Learning roadmap audit skipped.");
        }
    }

    private async Task TryRecordUsageAsync(Guid userId, AiGenerationResult result, CancellationToken cancellationToken)
    {
        try
        {
            await _usageRecorder.RecordAsync(
                new AiUsageRecordInput(
                    userId,
                    AiOperationNames.LearningRoadmap,
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
            _logger.LogWarning(ex, "Learning roadmap usage recording skipped.");
        }
    }
}
