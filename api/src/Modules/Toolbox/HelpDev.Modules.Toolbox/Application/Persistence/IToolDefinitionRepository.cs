using HelpDev.Modules.Toolbox.Domain.Tools;

namespace HelpDev.Modules.Toolbox.Application.Persistence;

public interface IToolDefinitionRepository
{
    Task<ToolDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ToolDefinition?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task AddAsync(ToolDefinition tool, CancellationToken cancellationToken = default);
}
