using HelpDev.Administration.Application.Tests.Fakes;
using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.FeatureFlags;
using HelpDev.Modules.Administration.Domain.FeatureFlags;
using HelpDev.SharedContracts.Auditing;
using HelpDev.Testing.Auditing;

namespace HelpDev.Administration.Application.Tests;

public sealed class FeatureFlagServiceTests
{
    private readonly FakeFeatureFlagRepository _repository = new();
    private readonly FakeFeatureFlagQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly FeatureFlagService _sut;

    public FeatureFlagServiceTests()
    {
        _sut = ServiceFactory.CreateFeatureFlagService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_commits_once()
    {
        using var cts = new CancellationTokenSource();

        var dto = await _sut.CreateAsync(
            new CreateFeatureFlagRequest("SearchEnabled", true, "desc"),
            Guid.NewGuid(),
            cts.Token);

        Assert.Equal("SearchEnabled", dto.Key);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
    }

    [Fact]
    public async Task Create_rejects_duplicate_without_commit()
    {
        _repository.Seed(FeatureFlag.Create(Guid.NewGuid(), "SearchEnabled", false, null, _clock.UtcNow));

        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.CreateAsync(new CreateFeatureFlagRequest("searchenabled", true, null)));

        Assert.Equal(AdministrationApplicationErrorCodes.FeatureKeyDuplicate, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task SetEnabled_noop_does_not_commit()
    {
        _repository.Seed(FeatureFlag.Create(Guid.NewGuid(), "LearningEnabled", true, null, _clock.UtcNow));

        await _sut.SetEnabledAsync("LearningEnabled", true);

        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task SetEnabled_commits_once_on_change()
    {
        _repository.Seed(FeatureFlag.Create(Guid.NewGuid(), "LearningEnabled", false, null, _clock.UtcNow));

        var dto = await _sut.SetEnabledAsync("LearningEnabled", true);

        Assert.True(dto.IsEnabled);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task GetByKey_missing_returns_not_found()
    {
        _queries.ByKey = null;

        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.GetByKeyAsync("Missing"));

        Assert.Equal(AdministrationApplicationErrorCodes.FeatureNotFound, ex.Code);
    }

    [Fact]
    public async Task Update_commits_once_when_description_changes()
    {
        _repository.Seed(FeatureFlag.Create(Guid.NewGuid(), "MaintenanceMode", false, "old", _clock.UtcNow));

        await _sut.UpdateAsync("MaintenanceMode", new UpdateFeatureFlagRequest("new"));

        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Create_records_audit_after_commit()
    {
        var audit = new FakeAuditRecorder();
        var sut = ServiceFactory.CreateFeatureFlagService(_repository, _queries, _unitOfWork, _clock, audit);

        await sut.CreateAsync(new CreateFeatureFlagRequest("AuditFlag", true, null), Guid.NewGuid());

        var record = Assert.Single(audit.Recorded);
        Assert.Equal(AuditActions.AdministrationFeatureFlagCreated, record.Action);
        Assert.Equal(AuditOutcomes.Success, record.Outcome);
        Assert.Equal("AuditFlag", record.Metadata!["key"]);
    }

    [Fact]
    public async Task SetEnabled_noop_does_not_record_audit()
    {
        var audit = new FakeAuditRecorder();
        var sut = ServiceFactory.CreateFeatureFlagService(_repository, _queries, _unitOfWork, _clock, audit);
        _repository.Seed(FeatureFlag.Create(Guid.NewGuid(), "NoAuditFlag", true, null, _clock.UtcNow));

        await sut.SetEnabledAsync("NoAuditFlag", true);

        Assert.Empty(audit.Recorded);
    }
}
