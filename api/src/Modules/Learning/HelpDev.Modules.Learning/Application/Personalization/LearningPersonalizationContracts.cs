using HelpDev.Modules.Learning.Domain.Personalization;

namespace HelpDev.Modules.Learning.Application.Personalization;

public sealed record LearningPreferenceDto(string Topic, int Priority, int InterestLevel);

public sealed record LearningProfileDto(
    Guid UserId,
    string ExperienceLevel,
    string LearningGoals,
    string CurrentSkills,
    IReadOnlyList<LearningPreferenceDto> PreferredTopics,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record UpdateLearningProfileRequest(
    string ExperienceLevel,
    string? LearningGoals,
    string? CurrentSkills,
    IReadOnlyList<LearningPreferenceDto>? PreferredTopics);

public sealed record LearningSignalEnrollmentDto(
    Guid CourseId,
    string CourseTitle,
    string Status,
    int ProgressPercentage,
    int CompletedLessonCount);

public sealed record LearningSignalsDto(
    Guid UserId,
    int EnrolledCourseCount,
    int ActiveEnrollmentCount,
    int CompletedCourseCount,
    int CompletedLessonCount,
    int ContentLinkedLessonCompletions,
    IReadOnlyList<LearningSignalEnrollmentDto> Enrollments,
    DateTime GeneratedAtUtc);

public sealed record RecommendedLearningItemDto(
    string Kind,
    Guid? CourseId,
    string Title,
    string? Slug,
    string? Rationale);

public sealed record LearningRecommendationDto(
    IReadOnlyList<RecommendedLearningItemDto> RecommendedItems,
    string Reason,
    IReadOnlyList<string> NextSteps,
    DateTime GeneratedAtUtc);

public sealed record LearningRoadmapStepDto(
    int StepOrder,
    string Title,
    string Description,
    Guid? RelatedCourseId);

public sealed record LearningRoadmapDto(
    Guid Id,
    string Goal,
    string Status,
    IReadOnlyList<LearningRoadmapStepDto> Steps,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? ApprovedAtUtc);

public sealed record GenerateLearningRoadmapRequest(string? Goal);

public sealed record LearningPersonalizationAdminDto(
    int ProfileCount,
    int PreferenceCount,
    int RoadmapCount,
    int ApprovedRoadmapCount,
    int SuggestedRoadmapCount,
    DateTime GeneratedAtUtc);

public interface ILearningProfileService
{
    Task<LearningProfileDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<LearningProfileDto> UpsertAsync(
        Guid userId,
        UpdateLearningProfileRequest request,
        CancellationToken cancellationToken = default);
}

public interface ILearningSignalsService
{
    Task<LearningSignalsDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ILearningRecommendationService
{
    Task<LearningRecommendationDto> GetRecommendationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public interface ILearningRoadmapService
{
    Task<LearningRoadmapDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<LearningRoadmapDto> GenerateAsync(
        Guid userId,
        GenerateLearningRoadmapRequest request,
        CancellationToken cancellationToken = default);

    Task<LearningRoadmapDto> ApproveAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ILearningPersonalizationAdminQueries
{
    Task<LearningPersonalizationAdminDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

/// <summary>Port for HelpDev knowledge retrieval (implemented via RAG adapter in Infrastructure).</summary>
public interface ILearningKnowledgeRetriever
{
    Task<LearningKnowledgeContext> RetrieveAsync(
        string topic,
        int take = 8,
        CancellationToken cancellationToken = default);
}

public sealed record LearningKnowledgeSnippet(
    string Title,
    string Url,
    string SourceType,
    string Snippet);

public sealed record LearningKnowledgeContext(
    string Topic,
    IReadOnlyList<LearningKnowledgeSnippet> Sources);

public interface ILearningProfileRepository
{
    Task<LearningProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(LearningProfile profile, CancellationToken cancellationToken = default);
}

public interface ILearningRoadmapRepository
{
    Task<LearningRoadmap?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(LearningRoadmap roadmap, CancellationToken cancellationToken = default);
}

public static class LearningAiTaskTypes
{
    public const string Recommend = "LearningRecommend";
    public const string Roadmap = "LearningRoadmap";
}

public static class LearningPersonalizationTopics
{
    public static IReadOnlyList<string> Defaults { get; } =
    [
        ".NET",
        "AI",
        "Frontend",
        "Architecture",
        "DevOps",
    ];
}
