using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.AiWorkflow;

/// <summary>
/// Human-owned content idea. Status changes only via explicit commands — never automatic.
/// </summary>
public sealed class ContentIdea : AggregateRoot<Guid>
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int TargetTypeMaxLength = 40;

    private ContentIdea()
    {
    }

    private ContentIdea(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string TargetType { get; private set; } = "Article";

    public ContentIdeaStatus Status { get; private set; } = ContentIdeaStatus.Draft;

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static ContentIdea Create(
        Guid id,
        string title,
        string description,
        string targetType,
        Guid createdByUserId,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Idea id is required.");
        }

        if (createdByUserId == Guid.Empty)
        {
            throw new DomainException("Creator is required.");
        }

        var idea = new ContentIdea(id)
        {
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Status = ContentIdeaStatus.Draft,
        };

        idea.ApplyDetails(title, description, targetType, utcNow);
        return idea;
    }

    public void UpdateDetails(string title, string description, string targetType, DateTime utcNow)
    {
        EnsureMutable();
        ApplyDetails(title, description, targetType, utcNow);
    }

    public void MarkResearching(DateTime utcNow) => TransitionTo(ContentIdeaStatus.Researching, utcNow);

    public void MarkWriting(DateTime utcNow) => TransitionTo(ContentIdeaStatus.Writing, utcNow);

    public void MarkReview(DateTime utcNow) => TransitionTo(ContentIdeaStatus.Review, utcNow);

    public void MarkCompleted(DateTime utcNow) => TransitionTo(ContentIdeaStatus.Completed, utcNow);

    public void Cancel(DateTime utcNow) => TransitionTo(ContentIdeaStatus.Cancelled, utcNow);

    private void TransitionTo(ContentIdeaStatus next, DateTime utcNow)
    {
        if (Status is ContentIdeaStatus.Completed or ContentIdeaStatus.Cancelled)
        {
            throw new DomainException("Idea is closed and cannot change status.");
        }

        if (Status == next)
        {
            return;
        }

        Status = next;
        UpdatedAtUtc = utcNow;
    }

    private void EnsureMutable()
    {
        if (Status is ContentIdeaStatus.Completed or ContentIdeaStatus.Cancelled)
        {
            throw new DomainException("Idea is closed and cannot be edited.");
        }
    }

    private void ApplyDetails(string title, string description, string targetType, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Idea title is required.");
        }

        var normalizedTitle = title.Trim();
        if (normalizedTitle.Length > TitleMaxLength)
        {
            throw new DomainException($"Idea title max length is {TitleMaxLength}.");
        }

        var normalizedDescription = (description ?? string.Empty).Trim();
        if (normalizedDescription.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Idea description max length is {DescriptionMaxLength}.");
        }

        var normalizedType = string.IsNullOrWhiteSpace(targetType) ? "Article" : targetType.Trim();
        if (normalizedType.Length > TargetTypeMaxLength)
        {
            throw new DomainException($"Target type max length is {TargetTypeMaxLength}.");
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        TargetType = normalizedType;
        UpdatedAtUtc = utcNow;
    }
}
