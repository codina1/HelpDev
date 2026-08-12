using HelpDev.Administration.Application.Tests.Fakes;
using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.Settings;
using HelpDev.Modules.Administration.Domain.Settings;

namespace HelpDev.Administration.Application.Tests;

public sealed class SystemSettingServiceTests
{
    private readonly FakeSystemSettingRepository _repository = new();
    private readonly FakeSystemSettingQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly SystemSettingService _sut;

    public SystemSettingServiceTests()
    {
        _sut = ServiceFactory.CreateSettingService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_commits_once()
    {
        using var cts = new CancellationTokenSource();

        var dto = await _sut.CreateAsync(
            new CreateSystemSettingRequest("SiteName", "HelpDev", "String", null, true),
            Guid.NewGuid(),
            cts.Token);

        Assert.Equal("SiteName", dto.Key);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
    }

    [Fact]
    public async Task Create_rejects_invalid_typed_value_before_commit()
    {
        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.CreateAsync(new CreateSystemSettingRequest("DefaultPageSize", "abc", "Integer", null, false)));

        Assert.Equal(AdministrationApplicationErrorCodes.SettingValueInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Create_rejects_duplicate()
    {
        _repository.Seed(SystemSetting.Create(
            Guid.NewGuid(),
            "SiteName",
            "Old",
            SystemSettingValueType.String,
            null,
            true,
            _clock.UtcNow));

        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.CreateAsync(new CreateSystemSettingRequest("sitename", "New", "String", null, true)));

        Assert.Equal(AdministrationApplicationErrorCodes.SettingKeyDuplicate, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Create_rejects_sensitive_key()
    {
        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.CreateAsync(new CreateSystemSettingRequest("JwtSecret", "x", "String", null, false)));

        Assert.Equal(AdministrationApplicationErrorCodes.SettingSensitiveKeyForbidden, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Update_commits_once()
    {
        _repository.Seed(SystemSetting.Create(
            Guid.NewGuid(),
            "SupportEmail",
            "a@b.com",
            SystemSettingValueType.String,
            null,
            true,
            _clock.UtcNow));

        await _sut.UpdateAsync("SupportEmail", new UpdateSystemSettingRequest("c@d.com", null, null));

        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task GetByKey_missing_returns_not_found()
    {
        _queries.ByKey = null;

        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.GetByKeyAsync("Missing"));

        Assert.Equal(AdministrationApplicationErrorCodes.SettingNotFound, ex.Code);
    }
}
