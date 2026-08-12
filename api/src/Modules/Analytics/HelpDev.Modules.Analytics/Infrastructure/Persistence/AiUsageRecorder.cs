using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.AiUsage;
using HelpDev.SharedContracts.Ai;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AiUsageRecorder : IAiUsageRecorder
{
    private readonly IAiUsageRecordRepository _repository;
    private readonly IDateTimeProvider _clock;

    public AiUsageRecorder(IAiUsageRecordRepository repository, IDateTimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task RecordAsync(AiUsageRecordInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var record = AiUsageRecord.Create(
            Guid.NewGuid(),
            input.UserId,
            input.TaskType,
            input.Provider,
            input.Model,
            input.InputTokens,
            input.OutputTokens,
            input.ContentId,
            _clock.UtcNow,
            input.Success,
            input.DurationMs,
            input.ErrorCode);

        await _repository.AddAsync(record, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AiUsageRecordRepository : IAiUsageRecordRepository
{
    private readonly IAnalyticsDbContext _dbContext;

    public AiUsageRecordRepository(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AiUsageRecord record, CancellationToken cancellationToken = default)
    {
        await _dbContext.AiUsageRecords.AddAsync(record, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
