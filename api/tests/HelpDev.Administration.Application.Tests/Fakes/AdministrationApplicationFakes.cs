using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Application.Persistence;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Administration.Domain.Announcements;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using HelpDev.Modules.Administration.Domain.Settings;
using HelpDev.SharedApplication.Abstractions.Persistence;
using HelpDev.SharedContracts.Auditing;
using HelpDev.SharedKernel.Time;
using HelpDev.Testing.Auditing;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpDev.Administration.Application.Tests.Fakes;

internal sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTime utcNow) =>
        UtcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

    public DateTime UtcNow { get; private set; }

    public void Advance(TimeSpan delta) => UtcNow = UtcNow.Add(delta);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public CancellationToken LastToken { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        LastToken = cancellationToken;
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeFeatureFlagRepository : IFeatureFlagRepository
{
    private readonly List<FeatureFlag> _items = [];

    public int AddCallCount { get; private set; }

    public IReadOnlyList<FeatureFlag> Items => _items;

    public void Seed(FeatureFlag flag) => _items.Add(flag);

    public Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(item =>
            string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.Any(item =>
            string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _items.Add(featureFlag);
        return Task.CompletedTask;
    }
}

internal sealed class FakeFeatureFlagQueries : IFeatureFlagQueries
{
    public IReadOnlyList<FeatureFlagDto> All { get; set; } = [];

    public FeatureFlagDto? ByKey { get; set; }

    public Task<IReadOnlyList<FeatureFlagDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(All);

    public Task<FeatureFlagDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(ByKey);
}

internal sealed class FakeSystemSettingRepository : ISystemSettingRepository
{
    private readonly List<SystemSetting> _items = [];

    public int AddCallCount { get; private set; }

    public IReadOnlyList<SystemSetting> Items => _items;

    public void Seed(SystemSetting setting) => _items.Add(setting);

    public Task<SystemSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(item =>
            string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.Any(item =>
            string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(SystemSetting setting, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _items.Add(setting);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSystemSettingQueries : ISystemSettingQueries
{
    public IReadOnlyList<SystemSettingDto> All { get; set; } = [];

    public SystemSettingDto? ByKey { get; set; }

    public Task<IReadOnlyList<SystemSettingDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(All);

    public Task<SystemSettingDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(ByKey);
}

internal sealed class FakeAnnouncementRepository : IAnnouncementRepository
{
    private readonly List<Announcement> _items = [];

    public int AddCallCount { get; private set; }

    public int RemoveCallCount { get; private set; }

    public IReadOnlyList<Announcement> Items => _items;

    public void Seed(Announcement announcement) => _items.Add(announcement);

    public Task<Announcement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(item => item.Id == id));

    public Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        AddCallCount++;
        _items.Add(announcement);
        return Task.CompletedTask;
    }

    public void Remove(Announcement announcement)
    {
        RemoveCallCount++;
        _items.Remove(announcement);
    }
}

internal sealed class FakeAnnouncementQueries : IAnnouncementQueries
{
    public AnnouncementDto? ById { get; set; }

    public AnnouncementPageDto Page { get; set; } = new(1, 20, 0, []);

    public IReadOnlyList<ActiveAnnouncementDto> Active { get; set; } = [];

    public Task<AnnouncementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(ById);

    public Task<AnnouncementPageDto> GetPageAsync(
        AnnouncementFilter filter,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Page);

    public Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Active);
}

internal static class ServiceFactory
{
    public static FeatureFlagService CreateFeatureFlagService(
        FakeFeatureFlagRepository repository,
        FakeFeatureFlagQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            queries,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<FeatureFlagService>.Instance);

    public static SystemSettingService CreateSettingService(
        FakeSystemSettingRepository repository,
        FakeSystemSettingQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            queries,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<SystemSettingService>.Instance);

    public static AnnouncementService CreateAnnouncementService(
        FakeAnnouncementRepository repository,
        FakeAnnouncementQueries queries,
        FakeUnitOfWork unitOfWork,
        FakeDateTimeProvider clock,
        IAuditRecorder? auditRecorder = null,
        IAuditRequestContext? auditRequestContext = null) =>
        new(
            repository,
            queries,
            unitOfWork,
            clock,
            auditRecorder ?? new NoOpAuditRecorder(),
            auditRequestContext ?? new FakeAuditRequestContext(),
            NullLogger<AnnouncementService>.Instance);
}
