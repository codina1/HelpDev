using HelpDev.Modules.PromptLab.Domain.Prompts;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptDefinitionRepository
{
    Task<PromptDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromptDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(PromptDefinition prompt, CancellationToken cancellationToken = default);
}
