using HelpDev.Modules.PromptLab.Application.Persistence;
using HelpDev.Modules.PromptLab.Domain.Rendering;

namespace HelpDev.Modules.PromptLab.Infrastructure.Persistence;

public sealed class PromptRenderRecordRepository : IPromptRenderRecordRepository
{
    private readonly IPromptLabDbContext _dbContext;

    public PromptRenderRecordRepository(IPromptLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PromptRenderRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        await _dbContext.PromptRenderRecords.AddAsync(record, cancellationToken);
    }
}
