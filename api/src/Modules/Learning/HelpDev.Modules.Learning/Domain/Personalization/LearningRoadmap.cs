namespace HelpDev.Modules.Learning.Domain.Personalization;

/// <summary>
/// User-owned personal roadmap. AI can only suggest; approval is explicit.
/// </summary>
public sealed class LearningRoadmap
{
    private readonly List<LearningRoadmapStep> _steps = [];

    private LearningRoadmap()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Goal { get; private set; } = string.Empty;

    public LearningRoadmapStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? ApprovedAtUtc { get; private set; }

    public IReadOnlyCollection<LearningRoadmapStep> Steps => _steps;

    public static LearningRoadmap CreateSuggested(
        Guid id,
        Guid userId,
        string goal,
        IEnumerable<LearningRoadmapStepInput> steps,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        var roadmap = new LearningRoadmap
        {
            Id = id,
            UserId = userId,
            Goal = NormalizeGoal(goal),
            Status = LearningRoadmapStatus.Suggested,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
        };
        roadmap.ReplaceSteps(steps);
        return roadmap;
    }

    public void ReplaceSuggestion(
        string goal,
        IEnumerable<LearningRoadmapStepInput> steps,
        DateTime updatedAtUtc)
    {
        Goal = NormalizeGoal(goal);
        Status = LearningRoadmapStatus.Suggested;
        ApprovedAtUtc = null;
        ReplaceSteps(steps);
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Approve(DateTime approvedAtUtc)
    {
        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("Cannot approve an empty roadmap.");
        }

        Status = LearningRoadmapStatus.Approved;
        ApprovedAtUtc = approvedAtUtc;
        UpdatedAtUtc = approvedAtUtc;
    }

    private void ReplaceSteps(IEnumerable<LearningRoadmapStepInput> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps.Clear();
        var order = 1;
        foreach (var step in steps.Take(12))
        {
            _steps.Add(LearningRoadmapStep.Create(
                Guid.NewGuid(),
                Id,
                order++,
                step.Title,
                step.Description,
                step.RelatedCourseId));
        }

        if (_steps.Count == 0)
        {
            throw new ArgumentException("At least one roadmap step is required.");
        }
    }

    private static string NormalizeGoal(string goal)
    {
        var trimmed = (goal ?? string.Empty).Trim();
        if (trimmed.Length is < 2 or > 200)
        {
            throw new ArgumentException("Goal is invalid.");
        }

        return trimmed;
    }
}

public sealed record LearningRoadmapStepInput(
    string Title,
    string? Description,
    Guid? RelatedCourseId);

public sealed class LearningRoadmapStep
{
    private LearningRoadmapStep()
    {
    }

    public Guid Id { get; private set; }

    public Guid RoadmapId { get; private set; }

    public int StepOrder { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Guid? RelatedCourseId { get; private set; }

    public static LearningRoadmapStep Create(
        Guid id,
        Guid roadmapId,
        int stepOrder,
        string title,
        string? description,
        Guid? relatedCourseId)
    {
        if (id == Guid.Empty || roadmapId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        var normalizedTitle = (title ?? string.Empty).Trim();
        if (normalizedTitle.Length is < 1 or > 160)
        {
            throw new ArgumentException("Step title is invalid.");
        }

        var normalizedDescription = (description ?? string.Empty).Trim();
        if (normalizedDescription.Length > 1000)
        {
            normalizedDescription = normalizedDescription[..1000];
        }

        return new LearningRoadmapStep
        {
            Id = id,
            RoadmapId = roadmapId,
            StepOrder = stepOrder,
            Title = normalizedTitle,
            Description = normalizedDescription,
            RelatedCourseId = relatedCourseId == Guid.Empty ? null : relatedCourseId,
        };
    }
}
