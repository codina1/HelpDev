using HelpDev.Modules.Analytics.Application.Persistence;
using HelpDev.Modules.Analytics.Domain.Metrics;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Analytics.Infrastructure.Persistence;

public sealed class AnalyticsSubjectSnapshotRepository : IAnalyticsSubjectSnapshotRepository
{
    private readonly IAnalyticsDbContext _dbContext;

    public AnalyticsSubjectSnapshotRepository(IAnalyticsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AnalyticsSubjectSnapshot?> GetAsync(
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken = default) =>
        _dbContext.AnalyticsSubjectSnapshots.FirstOrDefaultAsync(
            snapshot => snapshot.SubjectType == subjectType && snapshot.SubjectId == subjectId,
            cancellationToken);

    public async Task AddAsync(AnalyticsSubjectSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _dbContext.AnalyticsSubjectSnapshots.AddAsync(snapshot, cancellationToken);
    }
}
