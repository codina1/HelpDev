using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Roadmaps;

public sealed class RoadmapStep
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxProjectTitleLength = 200;
    public const int MaxProjectDescriptionLength = 4000;

    private readonly List<RoadmapTopic> _topics = [];
    private readonly List<RoadmapResource> _resources = [];

    private RoadmapStep()
    {
    }

    private RoadmapStep(
        Guid id,
        Guid roadmapId,
        string title,
        string? description,
        int order,
        int estimatedHours,
        string? projectTitle,
        string? projectDescription)
    {
        Id = id;
        RoadmapId = roadmapId;
        Title = title;
        Description = description;
        Order = order;
        EstimatedHours = estimatedHours;
        ProjectTitle = projectTitle;
        ProjectDescription = projectDescription;
    }

    public Guid Id { get; private set; }

    public Guid RoadmapId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public int EstimatedHours { get; private set; }

    public string? ProjectTitle { get; private set; }

    public string? ProjectDescription { get; private set; }

    public IReadOnlyList<RoadmapTopic> Topics => _topics.AsReadOnly();

    public IReadOnlyList<RoadmapResource> Resources => _resources.AsReadOnly();

    public static RoadmapStep Create(
        Guid id,
        Guid roadmapId,
        string title,
        string? description,
        int order,
        int estimatedHours,
        string? projectTitle,
        string? projectDescription)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه گام الزامی است.");
        }

        if (roadmapId == Guid.Empty)
        {
            throw new DomainException("شناسه نقشه راه الزامی است.");
        }

        if (order < 0)
        {
            throw new DomainException("ترتیب گام نمی‌تواند منفی باشد.");
        }

        if (estimatedHours < 0)
        {
            throw new DomainException("ساعت تخمینی نمی‌تواند منفی باشد.");
        }

        return new RoadmapStep(
            id,
            roadmapId,
            NormalizeTitle(title),
            NormalizeOptional(description, MaxDescriptionLength, "توضیح گام"),
            order,
            estimatedHours,
            NormalizeOptional(projectTitle, MaxProjectTitleLength, "عنوان پروژه"),
            NormalizeOptional(projectDescription, MaxProjectDescriptionLength, "توضیح پروژه"));
    }

    public void Update(
        string title,
        string? description,
        int order,
        int estimatedHours,
        string? projectTitle,
        string? projectDescription)
    {
        var updated = Create(
            Id,
            RoadmapId,
            title,
            description,
            order,
            estimatedHours,
            projectTitle,
            projectDescription);
        Title = updated.Title;
        Description = updated.Description;
        Order = updated.Order;
        EstimatedHours = updated.EstimatedHours;
        ProjectTitle = updated.ProjectTitle;
        ProjectDescription = updated.ProjectDescription;
    }

    public void SetOrder(int order)
    {
        if (order < 0)
        {
            throw new DomainException("ترتیب گام نمی‌تواند منفی باشد.");
        }

        Order = order;
    }

    public void ReplaceTopics(IEnumerable<RoadmapTopic> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);
        _topics.Clear();
        foreach (var topic in topics)
        {
            if (topic.StepId != Id)
            {
                throw new DomainException("موضوع به گام دیگری تعلق دارد.");
            }

            _topics.Add(topic);
        }
    }

    public void ReplaceResources(IEnumerable<RoadmapResource> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        _resources.Clear();
        foreach (var resource in resources)
        {
            if (resource.StepId != Id)
            {
                throw new DomainException("منبع به گام دیگری تعلق دارد.");
            }

            _resources.Add(resource);
        }
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("عنوان گام الزامی است.");
        }

        var normalized = title.Trim();
        if (normalized.Length > MaxTitleLength)
        {
            throw new DomainException("عنوان گام بیش از حد مجاز است.");
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
