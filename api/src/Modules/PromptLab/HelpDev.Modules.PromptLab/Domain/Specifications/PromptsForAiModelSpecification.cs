using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Specifications;

namespace HelpDev.Modules.PromptLab.Domain.Specifications;

public sealed class PromptsForAiModelSpecification : Specification<Prompt>
{
    public PromptsForAiModelSpecification(Guid aiModelId)
    {
        Where(prompt => prompt.AiModelId == aiModelId);
    }
}
