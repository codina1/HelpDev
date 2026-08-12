using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Roadmaps;

/// <summary>
/// Roadmap satellite on Content. Content owns lifecycle; this stores level, duration,
/// goal, prerequisites and ordered phases (steps). No EF in Domain.
/// </summary>
public sealed class RoadmapMetadata
{
    public const int MaxEstimatedDurationLength = 120;
    public const int MaxGoalLength = 2000;
    public const int MaxPrerequisitesLength = 2000;

    private readonly List<RoadmapStep> _steps = [];

    private RoadmapMetadata()
    {
    }

    private RoadmapMetadata(
        Guid id,
        Guid contentId,
        RoadmapLevel level,
        string estimatedDuration,
        string goal,
        string? prerequisites,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        ContentId = contentId;
        Level = level;
        EstimatedDuration = estimatedDuration;
        Goal = goal;
        Prerequisites = prerequisites;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    public RoadmapLevel Level { get; private set; }

    public string EstimatedDuration { get; private set; } = string.Empty;

    public string Goal { get; private set; } = string.Empty;

    public string? Prerequisites { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<RoadmapStep> Steps => _steps.AsReadOnly();

    public static RoadmapMetadata Create(
        Guid id,
        Guid contentId,
        RoadmapLevel level,
        string estimatedDuration,
        string goal,
        string? prerequisites,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه متادیتای نقشه راه الزامی است.");
        }

        if (contentId == Guid.Empty)
        {
            throw new DomainException("شناسه محتوا الزامی است.");
        }

        if (!Enum.IsDefined(level))
        {
            throw new DomainException("سطح نقشه راه معتبر نیست.");
        }

        return new RoadmapMetadata(
            id,
            contentId,
            level,
            NormalizeRequired(estimatedDuration, MaxEstimatedDurationLength, "مدت تخمینی"),
            NormalizeRequired(goal, MaxGoalLength, "هدف"),
            NormalizeOptional(prerequisites, MaxPrerequisitesLength, "پیش‌نیازها"),
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(
        RoadmapLevel level,
        string estimatedDuration,
        string goal,
        string? prerequisites,
        DateTime updatedAtUtc)
    {
        if (!Enum.IsDefined(level))
        {
            throw new DomainException("سطح نقشه راه معتبر نیست.");
        }

        Level = level;
        EstimatedDuration = NormalizeRequired(estimatedDuration, MaxEstimatedDurationLength, "مدت تخمینی");
        Goal = NormalizeRequired(goal, MaxGoalLength, "هدف");
        Prerequisites = NormalizeOptional(prerequisites, MaxPrerequisitesLength, "پیش‌نیازها");
        UpdatedAtUtc = updatedAtUtc;
    }

    public RoadmapStep AddStep(
        Guid stepId,
        string title,
        string? description,
        int order,
        int estimatedHours,
        string? projectTitle,
        string? projectDescription,
        DateTime updatedAtUtc)
    {
        var step = RoadmapStep.Create(
            stepId,
            Id,
            title,
            description,
            order,
            estimatedHours,
            projectTitle,
            projectDescription);
        _steps.Add(step);
        UpdatedAtUtc = updatedAtUtc;
        return step;
    }

    public RoadmapStep GetRequiredStep(Guid stepId)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId);
        if (step is null)
        {
            throw new DomainException("گام نقشه راه یافت نشد.");
        }

        return step;
    }

    public void RemoveStep(Guid stepId, DateTime updatedAtUtc)
    {
        var removed = _steps.RemoveAll(s => s.Id == stepId);
        if (removed == 0)
        {
            throw new DomainException("گام نقشه راه یافت نشد.");
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    public void ReorderSteps(IReadOnlyList<Guid> orderedStepIds, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(orderedStepIds);

        if (orderedStepIds.Count != _steps.Count)
        {
            throw new DomainException("فهرست ترتیب باید شامل همه گام‌ها باشد.");
        }

        if (orderedStepIds.Distinct().Count() != orderedStepIds.Count)
        {
            throw new DomainException("شناسه گام‌ها در ترتیب تکراری است.");
        }

        var byId = _steps.ToDictionary(s => s.Id);
        for (var i = 0; i < orderedStepIds.Count; i++)
        {
            if (!byId.TryGetValue(orderedStepIds[i], out var step))
            {
                throw new DomainException("گام نقشه راه یافت نشد.");
            }

            step.SetOrder(i);
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    public void Touch(DateTime updatedAtUtc) => UpdatedAtUtc = updatedAtUtc;

    private static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} الزامی است.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} بیش از حد مجاز است.");
        }

        return normalized;
    }
}
