using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Workflow;

public static class ContentWorkflowRules
{
    public static void EnsureAllowed(ContentStatus from, ContentStatus to)
    {
        if (!IsAllowed(from, to))
        {
            throw new DomainException($"انتقال از {from} به {to} مجاز نیست.");
        }
    }

    public static bool IsAllowed(ContentStatus from, ContentStatus to) =>
        (from, to) switch
        {
            (ContentStatus.Draft, ContentStatus.ReviewPending) => true,
            (ContentStatus.ReviewPending, ContentStatus.Draft) => true,
            (ContentStatus.ReviewPending, ContentStatus.Approved) => true,
            (ContentStatus.Approved, ContentStatus.Published) => true,
            (ContentStatus.Published, ContentStatus.Archived) => true,
            _ => false,
        };
}
