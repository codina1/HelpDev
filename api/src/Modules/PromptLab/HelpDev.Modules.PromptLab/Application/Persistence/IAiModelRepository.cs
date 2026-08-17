using HelpDev.Modules.PromptLab.Domain.AiModels;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IAiModelRepository
{
    Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
