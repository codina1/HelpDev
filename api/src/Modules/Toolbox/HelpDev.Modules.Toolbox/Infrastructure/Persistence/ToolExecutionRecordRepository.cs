using HelpDev.Modules.Toolbox.Application.Persistence;
using HelpDev.Modules.Toolbox.Domain.Execution;

namespace HelpDev.Modules.Toolbox.Infrastructure.Persistence;

public sealed class ToolExecutionRecordRepository : IToolExecutionRecordRepository
{
    private readonly IToolboxDbContext _dbContext;

    public ToolExecutionRecordRepository(IToolboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ToolExecutionRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        await _dbContext.ToolExecutionRecords.AddAsync(record, cancellationToken);
    }
}
