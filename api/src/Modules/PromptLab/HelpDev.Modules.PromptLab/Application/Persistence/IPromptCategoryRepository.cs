using HelpDev.Modules.PromptLab.Domain.Categories;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptCategoryRepository
{
    Task<PromptCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PromptCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(PromptCategory category, CancellationToken cancellationToken = default);
}
