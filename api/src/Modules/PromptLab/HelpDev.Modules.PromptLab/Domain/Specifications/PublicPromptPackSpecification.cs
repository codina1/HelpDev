using HelpDev.Modules.PromptLab.Domain.Packs;
using HelpDev.SharedKernel.Specifications;

namespace HelpDev.Modules.PromptLab.Domain.Specifications;

public sealed class PublicPromptPackSpecification : Specification<PromptPack>
{
    public PublicPromptPackSpecification()
    {
        Where(pack => pack.Status == PromptPackStatus.Approved);
    }
}
