using HelpDev.Modules.Toolbox.Domain.Execution;

namespace HelpDev.Modules.Toolbox.Application.Persistence;

public interface IToolExecutionRecordRepository
{
    Task AddAsync(ToolExecutionRecord record, CancellationToken cancellationToken = default);
}
