using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public static class PromptWorkflowRules
{
    public static void EnsureAllowed(PromptStatus from, PromptStatus to)
    {
        if (!IsAllowed(from, to))
        {
            throw new DomainException(
                $"Transition from {from} to {to} is not allowed.",
                PromptLabErrorCodes.PromptStatusInvalid);
        }
    }

    public static bool IsAllowed(PromptStatus from, PromptStatus to) =>
        (from, to) switch
        {
            (PromptStatus.Draft, PromptStatus.Submitted) => true,
            (PromptStatus.Submitted, PromptStatus.Approved) => true,
            (PromptStatus.Submitted, PromptStatus.Rejected) => true,
            (PromptStatus.Rejected, PromptStatus.Draft) => true,
            _ => false,
        };
}
