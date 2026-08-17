using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Specifications;

namespace HelpDev.Modules.PromptLab.Domain.Specifications;

public sealed class PromptsInCategorySpecification : Specification<Prompt>
{
    public PromptsInCategorySpecification(Guid categoryId)
    {
        Where(prompt => prompt.CategoryId == categoryId);
    }
}
