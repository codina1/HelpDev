using System.Text;
using HelpDev.Modules.Learning.Application.Persistence;
using HelpDev.Modules.Learning.Domain.Courses;
using HelpDev.Modules.Learning.Domain.Enrollments;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedContracts.Analytics;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HelpDev.Modules.Learning.Application.Personalization;

public sealed class LearningRecommendationService : ILearningRecommendationService
{
    private readonly ILearningProfileService _profileService;
    private readonly ILearningSignalsService _signalsService;
    private readonly ILearningDbContext _db;
    private readonly ILearningKnowledgeRetriever _knowledgeRetriever;
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiUsageRecorder _usageRecorder;
    private readonly IAnalyticsEventIngestor _analytics;
    private readonly IAuditRecorder _audit;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<LearningRecommendationService> _logger;

    public LearningRecommendationService(
        ILearningProfileService profileService,
        ILearningSignalsService signalsService,
        ILearningDbContext db,
        ILearningKnowledgeRetriever knowledgeRetriever,
        IAiTextGenerator aiTextGenerator,
        IAiUsageRecorder usageRecorder,
        IAnalyticsEventIngestor analytics,
        IAuditRecorder audit,
        IDateTimeProvider clock,
        ILogger<LearningRecommendationService> logger)
    {
        _profileService = profileService;
        _signalsService = signalsService;
        _db = db;
        _knowledgeRetriever = knowledgeRetriever;
        _aiTextGenerator = aiTextGenerator;
        _usageRecorder = usageRecorder;
        _analytics = analytics;
        _audit = audit;
        _clock = clock;
        _logger = logger;
    }

    public async Task<LearningRecommendationDto> GetRecommendationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileService.GetAsync(userId, cancellationToken);
        var signals = await _signalsService.GetAsync(userId, cancellationToken);
        var items = await BuildDeterministicItemsAsync(profile, signals, cancellationToken);

        var topic = string.IsNullOrWhiteSpace(profile.LearningGoals)
            ? string.Join(", ", profile.PreferredTopics.Select(t => t.Topic).DefaultIfEmpty("software development"))
            : profile.LearningGoals;
        var knowledge = await _knowledgeRetriever.RetrieveAsync(topic, take: 6, cancellationToken);

        var explanation = await ExplainAsync(userId, profile, signals, items, knowledge, cancellationToken);

        await TryTrackAsync(userId, items.Count, cancellationToken);

        return new LearningRecommendationDto(
            items,
            explanation.Reason,
            explanation.NextSteps,
            _clock.UtcNow);
    }

    private async Task<IReadOnlyList<RecommendedLearningItemDto>> BuildDeterministicItemsAsync(
        LearningProfileDto profile,
        LearningSignalsDto signals,
        CancellationToken cancellationToken)
    {
        var items = new List<RecommendedLearningItemDto>();
        var enrolledIds = signals.Enrollments.Select(e => e.CourseId).ToHashSet();

        foreach (var enrollment in signals.Enrollments
                     .Where(e => e.Status == nameof(EnrollmentStatus.Active) && e.ProgressPercentage is > 0 and < 100)
                     .Take(3))
        {
            items.Add(new RecommendedLearningItemDto(
                "ContinueCourse",
                enrollment.CourseId,
                enrollment.CourseTitle,
                null,
                "ادامه مسیر ثبت‌نام‌شده بر اساس پیشرفت واقعی."));
        }

        var published = await _db.Courses
            .AsNoTracking()
            .Where(c => c.Status == CourseStatus.Published)
            .Select(c => new { c.Id, c.Title, Slug = c.Slug.Value, c.Description })
            .ToListAsync(cancellationToken);

        var topics = profile.PreferredTopics
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.InterestLevel)
            .Select(t => t.Topic)
            .ToList();

        foreach (var course in published.Where(c => !enrolledIds.Contains(c.Id)))
        {
            if (items.Count >= 8)
            {
                break;
            }

            var match = topics.FirstOrDefault(topic =>
                course.Title.Contains(topic, StringComparison.OrdinalIgnoreCase)
                || course.Description.Contains(topic, StringComparison.OrdinalIgnoreCase));

            if (match is null && topics.Count > 0)
            {
                continue;
            }

            items.Add(new RecommendedLearningItemDto(
                "Course",
                course.Id,
                course.Title,
                course.Slug,
                match is null
                    ? "دوره منتشرشده در کاتالوگ HelpDev."
                    : $"هم‌راستا با علاقه ثبت‌شده: {match}"));
        }

        if (items.Count == 0)
        {
            foreach (var course in published.Take(5))
            {
                items.Add(new RecommendedLearningItemDto(
                    "Course",
                    course.Id,
                    course.Title,
                    course.Slug,
                    "پیشنهاد از کاتالوگ منتشرشده HelpDev (بدون امتیاز ساختگی)."));
            }
        }

        return items;
    }

    private async Task<(string Reason, IReadOnlyList<string> NextSteps)> ExplainAsync(
        Guid userId,
        LearningProfileDto profile,
        LearningSignalsDto signals,
        IReadOnlyList<RecommendedLearningItemDto> items,
        LearningKnowledgeContext knowledge,
        CancellationToken cancellationToken)
    {
        var fallbackReason =
            "پیشنهادها بر اساس پروفایل یادگیری، ثبت‌نام‌ها و دانش منتشرشده HelpDev ساخته شده‌اند. AI فقط توضیح می‌دهد و تغییری در پیشرفت ایجاد نمی‌کند.";
        var fallbackSteps = items
            .Take(5)
            .Select(i => i.Kind == "ContinueCourse" ? $"ادامه «{i.Title}»" : $"بررسی دوره «{i.Title}»")
            .ToList();

        if (fallbackSteps.Count == 0)
        {
            fallbackSteps.Add("پروفایل یادگیری و علاقه‌ها را تکمیل کنید.");
            fallbackSteps.Add("در یک دوره منتشرشده ثبت‌نام کنید.");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Experience: {profile.ExperienceLevel}");
        sb.AppendLine($"Goals: {profile.LearningGoals}");
        sb.AppendLine($"Skills: {profile.CurrentSkills}");
        sb.AppendLine($"Topics: {string.Join(", ", profile.PreferredTopics.Select(t => t.Topic))}");
        sb.AppendLine(
            $"Signals: enrolled={signals.EnrolledCourseCount}, active={signals.ActiveEnrollmentCount}, completedLessons={signals.CompletedLessonCount}, contentLinked={signals.ContentLinkedLessonCompletions}");
        sb.AppendLine("Recommended:");
        foreach (var item in items)
        {
            sb.AppendLine($"- [{item.Kind}] {item.Title}");
        }

        sb.AppendLine("Knowledge:");
        foreach (var source in knowledge.Sources.Take(6))
        {
            sb.AppendLine($"- {source.Title} ({source.SourceType}): {source.Snippet}");
        }

        var result = await _aiTextGenerator.GenerateSafeAsync(
            new AiTextRequest(
                LearningAiTaskTypes.Recommend,
                """
                You are a HelpDev learning assistant. Explain recommendations using ONLY the provided profile, signals, catalog items, and knowledge snippets.
                Do not invent courses, enroll the user, change progress, or claim confidence scores.
                Reply in Persian with:
                REASON: <one short paragraph>
                NEXT:
                - step 1
                - step 2
                """,
                sb.ToString(),
                MaxTokens: 700),
            cancellationToken);

        await TryRecordUsageAsync(userId, result, cancellationToken);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Content))
        {
            return (fallbackReason, fallbackSteps);
        }

        return ParseExplanation(result.Content!, fallbackReason, fallbackSteps);
    }

    private static (string Reason, IReadOnlyList<string> NextSteps) ParseExplanation(
        string content,
        string fallbackReason,
        IReadOnlyList<string> fallbackSteps)
    {
        var reason = fallbackReason;
        var steps = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var mode = "none";
        foreach (var line in lines)
        {
            if (line.StartsWith("REASON:", StringComparison.OrdinalIgnoreCase))
            {
                reason = line["REASON:".Length..].Trim();
                mode = "reason";
                continue;
            }

            if (line.StartsWith("NEXT:", StringComparison.OrdinalIgnoreCase))
            {
                mode = "next";
                continue;
            }

            if (mode == "reason" && !line.StartsWith('-'))
            {
                reason = string.IsNullOrWhiteSpace(reason) || reason == fallbackReason
                    ? line
                    : reason + " " + line;
            }
            else if (mode == "next" && (line.StartsWith('-') || line.StartsWith('•')))
            {
                var step = line.TrimStart('-', '•', ' ').Trim();
                if (step.Length > 0)
                {
                    steps.Add(step);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            reason = fallbackReason;
        }

        return (reason, steps.Count > 0 ? steps : fallbackSteps);
    }

    private async Task TryTrackAsync(Guid userId, int itemCount, CancellationToken cancellationToken)
    {
        try
        {
            await _analytics.IngestAsync(
                new AnalyticsEventEnvelope(
                    Guid.NewGuid(),
                    AnalyticsEventTypes.LearningRecommendationRequested,
                    _clock.UtcNow,
                    userId,
                    SubjectId: null,
                    SubjectType: null,
                    Dimensions: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["item_count"] = itemCount.ToString(),
                        ["generation_type"] = "recommendation",
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Learning recommendation analytics skipped.");
        }

        try
        {
            await _audit.RecordAsync(
                new AuditRecordInput(
                    AuditCategories.LearningAi,
                    AuditActions.LearningRecommendationRequested,
                    AuditOutcomes.Success,
                    userId,
                    AuditActorTypes.User,
                    Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["item_count"] = itemCount.ToString(),
                        ["generation_type"] = "recommendation",
                    }),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Learning recommendation audit skipped.");
        }
    }

    private async Task TryRecordUsageAsync(Guid userId, AiGenerationResult result, CancellationToken cancellationToken)
    {
        try
        {
            await _usageRecorder.RecordAsync(
                new AiUsageRecordInput(
                    userId,
                    AiOperationNames.LearningRecommend,
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
            _logger.LogWarning(ex, "Learning recommendation usage recording skipped.");
        }
    }
}
