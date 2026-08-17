using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Prompts;

namespace HelpDev.Modules.PromptLab.Application.Prompts;

public sealed record AdminPromptReviewListItemDto(
    Guid Id,
    string Title,
    string Slug,
    Guid AuthorId,
    Guid CategoryId,
    string CategoryName,
    string Preview,
    string Status,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public sealed record AdminPromptReviewDetailsDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string Content,
    string? CoverImage,
    string MediaType,
    Guid AuthorId,
    Guid CategoryId,
    string CategoryName,
    Guid AiModelId,
    string Status,
    string? RejectionReason,
    int Views,
    int CopyCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? PublishedAt);

public sealed record AdminPromptReviewPageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<AdminPromptReviewListItemDto> Items);

public sealed record AdminPromptReviewFilter(
    string Status,
    int Page,
    int PageSize);

public sealed record RejectAdminPromptRequest(string Reason);

public interface IPromptAdminReviewQueries
{
    Task<AdminPromptReviewPageDto> GetPromptsAsync(
        AdminPromptReviewFilter filter,
        CancellationToken cancellationToken = default);

    Task<AdminPromptReviewDetailsDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public interface IPromptAdminReviewService
{
    Task<AdminPromptReviewDetailsDto> ApproveAsync(
        Guid actorUserId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminPromptReviewDetailsDto> RejectAsync(
        Guid actorUserId,
        Guid id,
        RejectAdminPromptRequest request,
        CancellationToken cancellationToken = default);
}

public static class AdminPromptReviewStatuses
{
    public static bool TryParse(string? value, out PromptStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Enum.TryParse(value.Trim(), ignoreCase: true, out status) || !Enum.IsDefined(status))
        {
            return false;
        }

        return status is PromptStatus.Submitted or PromptStatus.Approved or PromptStatus.Rejected;
    }

    public static string Preview(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.Length <= PromptLabLimits.AdminPromptPreviewLength)
        {
            return trimmed;
        }

        return trimmed[..PromptLabLimits.AdminPromptPreviewLength].TrimEnd() + "…";
    }
}
