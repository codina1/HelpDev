using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Packs;

public sealed class PromptPackItem : Entity<Guid>
{
    private PromptPackItem()
    {
    }

    private PromptPackItem(Guid id)
        : base(id)
    {
    }

    public Guid PackId { get; private set; }

    public Guid PromptId { get; private set; }

    public int Order { get; private set; }

    internal static PromptPackItem Create(Guid id, Guid packId, Guid promptId, int order)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Pack item id must not be empty.", PromptLabErrorCodes.PackItemInvalid);
        }

        if (packId == Guid.Empty)
        {
            throw new DomainException("Pack id is required.", PromptLabErrorCodes.PackItemInvalid);
        }

        if (promptId == Guid.Empty)
        {
            throw new DomainException("Prompt id is required.", PromptLabErrorCodes.PackItemInvalid);
        }

        if (order < 1)
        {
            throw new DomainException("Pack item order must be >= 1.", PromptLabErrorCodes.PackItemOrderInvalid);
        }

        return new PromptPackItem(id)
        {
            PackId = packId,
            PromptId = promptId,
            Order = order,
        };
    }

    internal void SetOrder(int order)
    {
        if (order < 1)
        {
            throw new DomainException("Pack item order must be >= 1.", PromptLabErrorCodes.PackItemOrderInvalid);
        }

        Order = order;
    }
}
