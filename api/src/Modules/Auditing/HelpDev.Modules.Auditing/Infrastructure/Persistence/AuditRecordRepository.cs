using HelpDev.Modules.Auditing;
using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Domain.Records;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Auditing.Infrastructure.Persistence;

public sealed class AuditRecordRepository : IAuditRecordRepository
{
    private readonly IAuditDbContext _dbContext;

    public AuditRecordRepository(IAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditRecord record, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditRecords.AddAsync(record, cancellationToken);
    }

    public Task<AuditRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.AuditRecords.FirstOrDefaultAsync(record => record.Id == id, cancellationToken);
}
