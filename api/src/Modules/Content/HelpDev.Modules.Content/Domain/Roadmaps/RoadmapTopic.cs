using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Roadmaps;

public sealed class RoadmapTopic
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    private RoadmapTopic()
    {
    }

    private RoadmapTopic(Guid id, Guid stepId, string title, string? description, int order)
    {
        Id = id;
        StepId = stepId;
        Title = title;
        Description = description;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid StepId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Order { get; private set; }

    public static RoadmapTopic Create(Guid id, Guid stepId, string title, string? description, int order)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه موضوع الزامی است.");
        }

        if (stepId == Guid.Empty)
        {
            throw new DomainException("شناسه گام الزامی است.");
        }

        if (order < 0)
        {
            throw new DomainException("ترتیب موضوع نمی‌تواند منفی باشد.");
        }

        return new RoadmapTopic(id, stepId, NormalizeTitle(title), NormalizeDescription(description), order);
    }

    public void Update(string title, string? description, int order)
    {
        var updated = Create(Id, StepId, title, description, order);
        Title = updated.Title;
        Description = updated.Description;
        Order = updated.Order;
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("عنوان موضوع الزامی است.");
        }

        var normalized = title.Trim();
        if (normalized.Length > MaxTitleLength)
        {
            throw new DomainException("عنوان موضوع بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var normalized = description.Trim();
        if (normalized.Length > MaxDescriptionLength)
        {
            throw new DomainException("توضیح موضوع بیش از حد مجاز است.");
        }

        return normalized;
    }
}
