using HelpDev.Modules.Toolbox.Domain.Categories;

namespace HelpDev.Modules.Toolbox.Application.Persistence;

public interface IToolCategoryRepository
{
    Task<ToolCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ToolCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(ToolCategory category, CancellationToken cancellationToken = default);
}
