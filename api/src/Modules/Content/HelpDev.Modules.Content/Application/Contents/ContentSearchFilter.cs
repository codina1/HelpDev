namespace HelpDev.Modules.Content.Application.Contents;

/// <summary>
/// Normalized filter for admin content queries. Page/PageSize are clamped to safe bounds.
/// <see cref="AuthorId"/> is populated by the API from the authenticated actor to enforce
/// ownership (writers see only their own content); it is not a client-supplied field.
/// </summary>
public sealed record ContentSearchFilter
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    public const int MaxPageSize = 100;

    private ContentSearchFilter(
        string? search,
        string? status,
        string? type,
        int page,
        int pageSize,
        Guid? authorId)
    {
        Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
        Page = page;
        PageSize = pageSize;
        AuthorId = authorId;
    }

    public string? Search { get; }

    public string? Status { get; }

    public string? Type { get; }

    public int Page { get; }

    public int PageSize { get; }

    public Guid? AuthorId { get; }

    /// <summary>
    /// Builds a filter with page/size clamped to [1..] and [1..MaxPageSize] with defaults applied.
    /// </summary>
    public static ContentSearchFilter Create(
        string? search = null,
        string? status = null,
        string? type = null,
        int? page = null,
        int? pageSize = null,
        Guid? authorId = null)
    {
        var normalizedPage = page is null or < 1 ? DefaultPage : page.Value;
        var normalizedPageSize = pageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize.Value,
        };

        return new ContentSearchFilter(search, status, type, normalizedPage, normalizedPageSize, authorId);
    }
}
