using HelpDev.Modules.PromptLab.Domain.Prompts;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptRepository
{
    Task<Prompt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        string slug,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Prompt prompt, CancellationToken cancellationToken = default);
}
