using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class AiModelRepository : IAiModelRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public AiModelRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.AiModels.FirstOrDefaultAsync(model => model.Id == id, cancellationToken);
}
