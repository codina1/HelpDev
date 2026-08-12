using HelpDev.Modules.Auditing.Domain.Records;
using Microsoft.EntityFrameworkCore;

namespace HelpDev.Modules.Auditing.Application.Persistence;

public interface IAuditDbContext
{
    DbSet<AuditRecord> AuditRecords { get; }
}

public interface IAuditRecordRepository
{
    Task AddAsync(AuditRecord record, CancellationToken cancellationToken = default);

    Task<AuditRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
