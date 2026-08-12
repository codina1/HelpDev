namespace HelpDev.Modules.Learning.Domain.Personalization;

public enum ExperienceLevel
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2,
}

public enum LearningRoadmapStatus
{
    Suggested = 0,
    Approved = 1,
}

/// <summary>
/// User-owned learning profile. AI never overwrites this aggregate.
/// </summary>
public sealed class LearningProfile
{
    private readonly List<LearningPreference> _preferences = [];

    private LearningProfile()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ExperienceLevel ExperienceLevel { get; private set; }

    public string LearningGoals { get; private set; } = string.Empty;

    public string CurrentSkills { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<LearningPreference> Preferences => _preferences;

    public static LearningProfile Create(
        Guid id,
        Guid userId,
        ExperienceLevel experienceLevel,
        string? learningGoals,
        string? currentSkills,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new ArgumentException("Id and UserId are required.");
        }

        return new LearningProfile
        {
            Id = id,
            UserId = userId,
            ExperienceLevel = experienceLevel,
            LearningGoals = NormalizeText(learningGoals, 2000),
            CurrentSkills = NormalizeText(currentSkills, 1000),
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
        };
    }

    public void Update(
        ExperienceLevel experienceLevel,
        string? learningGoals,
        string? currentSkills,
        IEnumerable<LearningPreferenceInput> preferences,
        DateTime updatedAtUtc)
    {
        ExperienceLevel = experienceLevel;
        LearningGoals = NormalizeText(learningGoals, 2000);
        CurrentSkills = NormalizeText(currentSkills, 1000);
        ReplacePreferences(preferences);
        UpdatedAtUtc = updatedAtUtc;
    }

    private void ReplacePreferences(IEnumerable<LearningPreferenceInput> preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        _preferences.Clear();
        var order = 0;
        foreach (var item in preferences)
        {
            _preferences.Add(LearningPreference.Create(
                Guid.NewGuid(),
                Id,
                item.Topic,
                item.Priority,
                item.InterestLevel,
                order++));
        }
    }

    private static string NormalizeText(string? value, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}

public sealed record LearningPreferenceInput(string Topic, int Priority, int InterestLevel);

public sealed class LearningPreference
{
    private LearningPreference()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProfileId { get; private set; }

    public string Topic { get; private set; } = string.Empty;

    public int Priority { get; private set; }

    public int InterestLevel { get; private set; }

    public int SortOrder { get; private set; }

    public static LearningPreference Create(
        Guid id,
        Guid profileId,
        string topic,
        int priority,
        int interestLevel,
        int sortOrder)
    {
        if (id == Guid.Empty || profileId == Guid.Empty)
        {
            throw new ArgumentException("Ids are required.");
        }

        var normalizedTopic = (topic ?? string.Empty).Trim();
        if (normalizedTopic.Length is < 1 or > 64)
        {
            throw new ArgumentException("Topic is invalid.");
        }

        if (priority is < 1 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        if (interestLevel is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(interestLevel));
        }

        return new LearningPreference
        {
            Id = id,
            ProfileId = profileId,
            Topic = normalizedTopic,
            Priority = priority,
            InterestLevel = interestLevel,
            SortOrder = sortOrder,
        };
    }
}
