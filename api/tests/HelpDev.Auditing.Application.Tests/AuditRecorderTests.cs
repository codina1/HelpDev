using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Application.Queries;
using HelpDev.Modules.Auditing.Application.Recording;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Auditing.Domain.Records;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HelpDev.Auditing.Application.Tests;

public sealed class AuditRecorderTests
{
    [Fact]
    public async Task RecordAsync_persists_sanitized_record_when_enabled()
    {
        var repository = new FakeAuditRecordRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FakeDateTimeProvider(new DateTime(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc));
        var recorder = CreateRecorder(repository, unitOfWork, clock, enabled: true);

        await recorder.RecordAsync(new AuditRecordInput(
            Category: AuditCategories.Administration,
            Action: AuditActions.AdministrationFeatureFlagCreated,
            Outcome: AuditOutcomes.Success,
            ActorUserId: Guid.NewGuid(),
            ActorType: AuditActorTypes.User,
            Metadata: new Dictionary<string, string>
            {
                ["key"] = "feature.test",
                ["previousState"] = "none",
                ["newState"] = "enabled",
            }));

        Assert.Single(repository.Items);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Equal(AuditActions.AdministrationFeatureFlagCreated, repository.Items[0].Action);
    }

    [Fact]
    public async Task RecordAsync_is_noop_when_disabled()
    {
        var repository = new FakeAuditRecordRepository();
        var unitOfWork = new FakeUnitOfWork();
        var recorder = CreateRecorder(repository, unitOfWork, new FakeDateTimeProvider(DateTime.UtcNow), enabled: false);

        await recorder.RecordAsync(new AuditRecordInput(
            Category: AuditCategories.Administration,
            Action: AuditActions.AdministrationFeatureFlagCreated,
            Outcome: AuditOutcomes.Success,
            ActorUserId: null,
            ActorType: AuditActorTypes.System));

        Assert.Empty(repository.Items);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    private static AuditRecorder CreateRecorder(
        FakeAuditRecordRepository repository,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        bool enabled) =>
        new(
            repository,
            unitOfWork,
            new AuditMetadataSanitizer(Options.Create(new AuditOptions())),
            clock,
            Options.Create(new AuditOptions { Enabled = enabled }),
            new NoOpAuditPersistenceFailureInjector(),
            NullLogger<AuditRecorder>.Instance);
}

public sealed class AuditQueriesTests
{
    [Fact]
    public async Task GetPageAsync_rejects_invalid_page_size()
    {
        var queries = new AuditQueries(new FakeAuditDbContext());

        var ex = await Assert.ThrowsAsync<AuditException>(() =>
            queries.GetPageAsync(new AuditQueryFilter(null, null, null, null, null, null, null, null, 1, 101)));

        Assert.Equal(AuditErrorCodes.PageInvalid, ex.Code);
    }
}

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) => UtcNow = utcNow;

    public DateTime UtcNow { get; }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeAuditRecordRepository : IAuditRecordRepository
{
    public List<AuditRecord> Items { get; } = [];

    public Task AddAsync(AuditRecord record, CancellationToken cancellationToken = default)
    {
        Items.Add(record);
        return Task.CompletedTask;
    }

    public Task<AuditRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(item => item.Id == id));
}

internal sealed class FakeAuditDbContext : IAuditDbContext
{
    public Microsoft.EntityFrameworkCore.DbSet<AuditRecord> AuditRecords =>
        throw new NotSupportedException("Query tests validate filter rules only.");

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(0);
}
