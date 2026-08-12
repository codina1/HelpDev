using HelpDev.Modules.Administration.Domain.Announcements;

namespace HelpDev.Modules.Administration.Application.Announcements;

public sealed record AnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

public sealed record AnnouncementListItemDto(
    Guid Id,
    string Title,
    string Type,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    DateTime UpdatedAtUtc,
    DateTime? PublishedAtUtc);

public sealed record AnnouncementPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<AnnouncementListItemDto> Items);

public sealed record ActiveAnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    string Type,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc);

public sealed record CreateAnnouncementRequest(
    string Title,
    string Body,
    string Type,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc);

public sealed record UpdateAnnouncementRequest(
    string Title,
    string Body,
    string Type,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc);

public sealed record AnnouncementFilter(
    string? Status,
    string? Type,
    int Page,
    int PageSize);

public static class AnnouncementPaging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}

public interface IAnnouncementQueries
{
    Task<AnnouncementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AnnouncementPageDto> GetPageAsync(
        AnnouncementFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActiveAnnouncementDto>> GetActiveAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

public interface IAnnouncementService
{
    Task<AnnouncementDto> CreateAsync(
        CreateAnnouncementRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDto> UpdateAsync(
        Guid id,
        UpdateAnnouncementRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDto> PublishAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDto> ArchiveAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<AnnouncementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AnnouncementPageDto> GetPageAsync(
        AnnouncementFilter filter,
        CancellationToken cancellationToken = default);
}

public static class AnnouncementEnumParser
{
    public static AnnouncementType ParseType(string type)
    {
        if (string.IsNullOrWhiteSpace(type)
            || !Enum.TryParse<AnnouncementType>(type.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new AdministrationException(
                "Announcement type is invalid.",
                AdministrationApplicationErrorCodes.AnnouncementStatusInvalid);
        }

        return parsed;
    }

    public static AnnouncementStatus? ParseStatusOrNull(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        if (!Enum.TryParse<AnnouncementStatus>(status.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new AdministrationException(
                "Announcement status filter is invalid.",
                AdministrationApplicationErrorCodes.AnnouncementStatusInvalid);
        }

        return parsed;
    }

    public static AnnouncementType? ParseTypeOrNull(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }

        return ParseType(type);
    }
}
