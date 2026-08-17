using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Specifications;

namespace HelpDev.Modules.PromptLab.Domain.Specifications;

public sealed class PublicPromptSpecification : Specification<Prompt>
{
    public PublicPromptSpecification()
    {
        Where(prompt => prompt.Status == PromptStatus.Approved);
    }
}
