using HelpDev.Modules.PromptLab.Domain.Rendering;

namespace HelpDev.Modules.PromptLab.Application.Persistence;

public interface IPromptRenderRecordRepository
{
    Task AddAsync(PromptRenderRecord record, CancellationToken cancellationToken = default);
}
