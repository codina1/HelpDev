using HelpDev.Modules.Administration.Domain;
using HelpDev.Modules.Administration.Domain.Announcements;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Administration.Tests;

public sealed class AnnouncementTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateDraft_trims_title_and_body()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "  Title  ",
            "  Body  ",
            AnnouncementType.Information,
            null,
            null,
            Now);

        Assert.Equal("Title", announcement.Title);
        Assert.Equal("Body", announcement.Body);
        Assert.Equal(AnnouncementStatus.Draft, announcement.Status);
        Assert.Null(announcement.PublishedAtUtc);
    }

    [Fact]
    public void CreateDraft_rejects_empty_title_and_body()
    {
        var titleEx = Assert.Throws<DomainException>(() =>
            Announcement.CreateDraft(Guid.NewGuid(), " ", "Body", AnnouncementType.Warning, null, null, Now));
        Assert.Equal(AdministrationErrorCodes.AnnouncementTitleRequired, titleEx.Code);

        var bodyEx = Assert.Throws<DomainException>(() =>
            Announcement.CreateDraft(Guid.NewGuid(), "Title", " ", AnnouncementType.Warning, null, null, Now));
        Assert.Equal(AdministrationErrorCodes.AnnouncementBodyRequired, bodyEx.Code);
    }

    [Fact]
    public void CreateDraft_rejects_oversized_title_and_body()
    {
        var titleEx = Assert.Throws<DomainException>(() =>
            Announcement.CreateDraft(
                Guid.NewGuid(),
                new string('t', Announcement.TitleMaxLength + 1),
                "Body",
                AnnouncementType.Release,
                null,
                null,
                Now));
        Assert.Equal(AdministrationErrorCodes.AnnouncementTitleInvalid, titleEx.Code);

        var bodyEx = Assert.Throws<DomainException>(() =>
            Announcement.CreateDraft(
                Guid.NewGuid(),
                "Title",
                new string('b', Announcement.BodyMaxLength + 1),
                AnnouncementType.Release,
                null,
                null,
                Now));
        Assert.Equal(AdministrationErrorCodes.AnnouncementBodyInvalid, bodyEx.Code);
    }

    [Fact]
    public void CreateDraft_rejects_invalid_schedule()
    {
        var start = Now;
        var end = Now.AddHours(-1);

        var ex = Assert.Throws<DomainException>(() =>
            Announcement.CreateDraft(
                Guid.NewGuid(),
                "Title",
                "Body",
                AnnouncementType.Maintenance,
                start,
                end,
                Now));

        Assert.Equal(AdministrationErrorCodes.AnnouncementScheduleInvalid, ex.Code);
    }

    [Fact]
    public void Publish_sets_published_at_once_and_archive_works()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Important,
            null,
            null,
            Now);

        var publishedAt = Now.AddMinutes(1);
        Assert.True(announcement.Publish(publishedAt));
        Assert.Equal(AnnouncementStatus.Published, announcement.Status);
        Assert.Equal(publishedAt, announcement.PublishedAtUtc);

        Assert.False(announcement.Publish(publishedAt.AddMinutes(1)));
        Assert.Equal(publishedAt, announcement.PublishedAtUtc);

        var archivedAt = publishedAt.AddMinutes(2);
        Assert.True(announcement.Archive(archivedAt));
        Assert.Equal(AnnouncementStatus.Archived, announcement.Status);
        Assert.Equal(archivedAt, announcement.UpdatedAtUtc);
    }

    [Fact]
    public void UpdateDetails_noop_does_not_change_timestamp()
    {
        var announcement = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Information,
            null,
            null,
            Now);

        Assert.False(announcement.UpdateDetails("Title", "Body", AnnouncementType.Information, Now.AddHours(1)));
        Assert.Equal(Now, announcement.UpdatedAtUtc);
    }

    [Fact]
    public void EnsureCanHardDelete_allows_draft_only()
    {
        var draft = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Information,
            null,
            null,
            Now);
        draft.EnsureCanHardDelete();

        draft.Publish(Now.AddMinutes(1));
        var ex = Assert.Throws<DomainException>(() => draft.EnsureCanHardDelete());
        Assert.Equal(AdministrationErrorCodes.AnnouncementCannotDeletePublished, ex.Code);
    }

    [Fact]
    public void Archive_rejects_draft()
    {
        var draft = Announcement.CreateDraft(
            Guid.NewGuid(),
            "Title",
            "Body",
            AnnouncementType.Information,
            null,
            null,
            Now);

        var ex = Assert.Throws<DomainException>(() => draft.Archive(Now));
        Assert.Equal(AdministrationErrorCodes.AnnouncementStatusInvalid, ex.Code);
    }
}
