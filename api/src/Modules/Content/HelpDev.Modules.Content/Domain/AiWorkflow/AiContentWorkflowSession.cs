using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.AiWorkflow;

/// <summary>
/// Tracks AI-assisted production progress. Generated text is not persisted here.
/// </summary>
public sealed class AiContentWorkflowSession : AggregateRoot<Guid>
{
    private AiContentWorkflowSession()
    {
    }

    private AiContentWorkflowSession(Guid id)
        : base(id)
    {
    }

    public Guid IdeaId { get; private set; }

    public AiContentWorkflowStep CurrentStep { get; private set; } = AiContentWorkflowStep.Research;

    public Guid CreatedByUserId { get; private set; }

    public Guid? LinkedContentId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static AiContentWorkflowSession Create(
        Guid id,
        Guid ideaId,
        Guid createdByUserId,
        DateTime utcNow)
    {
        if (id == Guid.Empty || ideaId == Guid.Empty || createdByUserId == Guid.Empty)
        {
            throw new DomainException("Workflow session ids are required.");
        }

        return new AiContentWorkflowSession(id)
        {
            IdeaId = ideaId,
            CreatedByUserId = createdByUserId,
            CurrentStep = AiContentWorkflowStep.Research,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void AdvanceTo(AiContentWorkflowStep step, DateTime utcNow)
    {
        if ((int)step < (int)CurrentStep)
        {
            // Allow revisiting earlier steps for human edits; still update timestamp.
            CurrentStep = step;
            UpdatedAtUtc = utcNow;
            return;
        }

        CurrentStep = step;
        UpdatedAtUtc = utcNow;
    }

    public void LinkContent(Guid contentId, DateTime utcNow)
    {
        if (contentId == Guid.Empty)
        {
            throw new DomainException("Content id is required.");
        }

        if (LinkedContentId.HasValue && LinkedContentId != contentId)
        {
            throw new DomainException("Workflow already linked to content.");
        }

        LinkedContentId = contentId;
        CurrentStep = AiContentWorkflowStep.Review;
        UpdatedAtUtc = utcNow;
    }
}
