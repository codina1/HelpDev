using HelpDev.Administration.Application.Tests.Fakes;
using HelpDev.Modules.Administration.Application;
using HelpDev.Modules.Administration.Application.Announcements;
using HelpDev.Modules.Administration.Domain.Announcements;

namespace HelpDev.Administration.Application.Tests;

public sealed class AnnouncementServiceTests
{
    private readonly FakeAnnouncementRepository _repository = new();
    private readonly FakeAnnouncementQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly AnnouncementService _sut;

    public AnnouncementServiceTests()
    {
        _sut = ServiceFactory.CreateAnnouncementService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_draft_commits_once()
    {
        using var cts = new CancellationTokenSource();

        var dto = await _sut.CreateAsync(
            new CreateAnnouncementRequest("Title", "Body", "Information", null, null),
            Guid.NewGuid(),
            cts.Token);

        Assert.Equal(nameof(AnnouncementStatus.Draft), dto.Status);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
    }

    [Fact]
    public async Task Create_rejects_invalid_schedule_without_commit()
    {
        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.CreateAsync(new CreateAnnouncementRequest(
                "Title",
                "Body",
                "Maintenance",
                _clock.UtcNow,
                _clock.UtcNow.AddHours(-1))));

        Assert.Equal(AdministrationApplicationErrorCodes.AnnouncementScheduleInvalid, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Publish_and_archive_commit_once_each()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Release,
            null,
            null,
            _clock.UtcNow);
        _repository.Seed(announcement);

        await _sut.PublishAsync(announcement.Id);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        await _sut.ArchiveAsync(announcement.Id);
        Assert.Equal(2, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Delete_draft_commits_once()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Information,
            null,
            null,
            _clock.UtcNow);
        _repository.Seed(announcement);

        await _sut.DeleteAsync(announcement.Id);

        Assert.Equal(1, _repository.RemoveCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Delete_published_is_rejected_without_commit()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Information,
            null,
            null,
            _clock.UtcNow);
        announcement.Publish(_clock.UtcNow);
        _repository.Seed(announcement);

        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.DeleteAsync(announcement.Id));

        Assert.Equal(AdministrationApplicationErrorCodes.AnnouncementCannotDeletePublished, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(0, _repository.RemoveCallCount);
    }

    [Fact]
    public async Task Update_missing_returns_not_found()
    {
        var ex = await Assert.ThrowsAsync<AdministrationException>(() =>
            _sut.UpdateAsync(
                Guid.NewGuid(),
                new UpdateAnnouncementRequest("T", "B", "Information", null, null)));

        Assert.Equal(AdministrationApplicationErrorCodes.AnnouncementNotFound, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
