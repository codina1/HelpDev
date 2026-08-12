using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Administration.Domain.Announcements;

public sealed class Announcement : AggregateRoot<Guid>
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 5000;

    /// <summary>Required for EF Core materialization.</summary>
    private Announcement()
    {
    }

    private Announcement(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public AnnouncementType Type { get; private set; }

    public AnnouncementStatus Status { get; private set; } = AnnouncementStatus.Draft;

    public DateTime? StartsAtUtc { get; private set; }

    public DateTime? EndsAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public static Announcement CreateDraft(
        Guid id,
        string title,
        string body,
        AnnouncementType type,
        DateTime? startsAtUtc,
        DateTime? endsAtUtc,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(
                "Announcement id must not be empty.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException(
                "Announcement type is invalid.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        var announcement = new Announcement(id)
        {
            Type = type,
            Status = AnnouncementStatus.Draft,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        announcement.ApplyDetails(title, body, type, force: true);
        announcement.ApplySchedule(startsAtUtc, endsAtUtc, force: true);
        return announcement;
    }

    public bool UpdateDetails(string title, string body, AnnouncementType type, DateTime utcNow)
    {
        EnsureEditable();
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(
                "Announcement type is invalid.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        var changed = ApplyDetails(title, body, type, force: false);
        if (!changed)
        {
            return false;
        }

        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool UpdateSchedule(DateTime? startsAtUtc, DateTime? endsAtUtc, DateTime utcNow)
    {
        EnsureEditable();
        var changed = ApplySchedule(startsAtUtc, endsAtUtc, force: false);
        if (!changed)
        {
            return false;
        }

        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Publish(DateTime utcNow)
    {
        if (Status == AnnouncementStatus.Published)
        {
            return false;
        }

        if (Status == AnnouncementStatus.Archived)
        {
            throw new DomainException(
                "Archived announcements cannot be published.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        ValidateSchedule(StartsAtUtc, EndsAtUtc);
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Body))
        {
            throw new DomainException(
                "Announcement title and body are required to publish.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        Status = AnnouncementStatus.Published;
        PublishedAtUtc ??= utcNow;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Archive(DateTime utcNow)
    {
        if (Status == AnnouncementStatus.Archived)
        {
            return false;
        }

        if (Status == AnnouncementStatus.Draft)
        {
            throw new DomainException(
                "Draft announcements cannot be archived; delete them instead.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }

        Status = AnnouncementStatus.Archived;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public void EnsureCanHardDelete()
    {
        if (Status != AnnouncementStatus.Draft)
        {
            throw new DomainException(
                "Only draft announcements can be deleted. Archive published announcements instead.",
                AdministrationErrorCodes.AnnouncementCannotDeletePublished);
        }
    }

    private void EnsureEditable()
    {
        if (Status == AnnouncementStatus.Archived)
        {
            throw new DomainException(
                "Archived announcements cannot be edited.",
                AdministrationErrorCodes.AnnouncementStatusInvalid);
        }
    }

    private bool ApplyDetails(string title, string body, AnnouncementType type, bool force)
    {
        var normalizedTitle = NormalizeTitle(title);
        var normalizedBody = NormalizeBody(body);

        var changed =
            force
            || !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || !string.Equals(Body, normalizedBody, StringComparison.Ordinal)
            || Type != type;

        Title = normalizedTitle;
        Body = normalizedBody;
        Type = type;
        return changed;
    }

    private bool ApplySchedule(DateTime? startsAtUtc, DateTime? endsAtUtc, bool force)
    {
        ValidateSchedule(startsAtUtc, endsAtUtc);

        var changed =
            force
            || StartsAtUtc != startsAtUtc
            || EndsAtUtc != endsAtUtc;

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        return changed;
    }

    private static void ValidateSchedule(DateTime? startsAtUtc, DateTime? endsAtUtc)
    {
        if (startsAtUtc is not null && endsAtUtc is not null && endsAtUtc <= startsAtUtc)
        {
            throw new DomainException(
                "Announcement end time must be greater than start time.",
                AdministrationErrorCodes.AnnouncementScheduleInvalid);
        }
    }

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException(
                "Announcement title is required.",
                AdministrationErrorCodes.AnnouncementTitleRequired);
        }

        var trimmed = title.Trim();
        if (trimmed.Length > TitleMaxLength)
        {
            throw new DomainException(
                $"Announcement title must be at most {TitleMaxLength} characters.",
                AdministrationErrorCodes.AnnouncementTitleInvalid);
        }

        return trimmed;
    }

    private static string NormalizeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException(
                "Announcement body is required.",
                AdministrationErrorCodes.AnnouncementBodyRequired);
        }

        var trimmed = body.Trim();
        if (trimmed.Length > BodyMaxLength)
        {
            throw new DomainException(
                $"Announcement body must be at most {BodyMaxLength} characters.",
                AdministrationErrorCodes.AnnouncementBodyInvalid);
        }

        return trimmed;
    }
}
