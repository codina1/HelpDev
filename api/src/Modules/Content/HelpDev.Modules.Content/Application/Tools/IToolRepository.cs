using HelpDev.Modules.Content.Domain.Tools;

namespace HelpDev.Modules.Content.Application.Tools;

public interface IToolRepository
{
    Task<ToolMetadata?> GetByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default);

    Task<ToolMetadata?> GetByIdAsync(Guid toolId, CancellationToken cancellationToken = default);

    Task AddAsync(ToolMetadata metadata, CancellationToken cancellationToken = default);

    Task AddFeatureAsync(ToolFeature feature, CancellationToken cancellationToken = default);

    Task<int> GetNextFeatureOrderAsync(Guid toolId, CancellationToken cancellationToken = default);
}
