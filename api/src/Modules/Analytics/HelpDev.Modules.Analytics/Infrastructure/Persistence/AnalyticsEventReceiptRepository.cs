using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AnalyticsEventReceiptRepository : IAnalyticsEventReceiptRepository
{
    private readonly IAnalyticsDbContext _dbContext;

    public AnalyticsEventReceiptRepository(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        _dbContext.AnalyticsEventReceipts
            .AsNoTracking()
            .AnyAsync(receipt => receipt.EventId == eventId, cancellationToken);

    public Task<AnalyticsEventReceipt?> GetAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        _dbContext.AnalyticsEventReceipts
            .FirstOrDefaultAsync(receipt => receipt.EventId == eventId, cancellationToken);

    public async Task AddAsync(AnalyticsEventReceipt receipt, CancellationToken cancellationToken = default)
    {
        await _dbContext.AnalyticsEventReceipts.AddAsync(receipt, cancellationToken);
    }
}
