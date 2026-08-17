using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Packs;

public static class PromptPackWorkflowRules
{
    public static void EnsureAllowed(PromptPackStatus from, PromptPackStatus to)
    {
        if (!IsAllowed(from, to))
        {
            throw new DomainException(
                $"Transition from {from} to {to} is not allowed.",
                PromptLabErrorCodes.PackStatusInvalid);
        }
    }

    public static bool IsAllowed(PromptPackStatus from, PromptPackStatus to) =>
        (from, to) switch
        {
            (PromptPackStatus.Draft, PromptPackStatus.Submitted) => true,
            (PromptPackStatus.Submitted, PromptPackStatus.Approved) => true,
            (PromptPackStatus.Submitted, PromptPackStatus.Rejected) => true,
            (PromptPackStatus.Rejected, PromptPackStatus.Draft) => true,
            _ => false,
        };
}
