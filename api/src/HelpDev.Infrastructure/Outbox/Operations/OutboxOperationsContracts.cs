using HelpDev.SharedKernel.Time;

namespace HelpDev.Infrastructure.Outbox.Operations;

public static class OutboxMessageStatuses
{
    public const string Pending = "pending";
    public const string Processing = "processing";
    public const string Failed = "failed";
    public const string Processed = "processed";

    public static bool IsKnown(string? value) =>
        value is Pending or Processing or Failed or Processed;

    public static string Derive(
        DateTime? processedAtUtc,
        int attemptCount,
        DateTime? lockedUntilUtc,
        DateTime nowUtc,
        int maxAttempts)
    {
        if (processedAtUtc is not null)
        {
            return Processed;
        }

        if (lockedUntilUtc is not null && lockedUntilUtc > nowUtc)
        {
            return Processing;
        }

        if (attemptCount >= maxAttempts)
        {
            return Failed;
        }

        return Pending;
    }

    public static bool IsActivelyLocked(DateTime? lockedUntilUtc, DateTime nowUtc) =>
        lockedUntilUtc is not null && lockedUntilUtc > nowUtc;
}

public sealed record OutboxStatusDto(
    int Pending,
    int Processing,
    int Failed,
    int Processed,
    DateTime? OldestPendingAtUtc,
    DateTime? LastProcessedAtUtc);

public sealed record OutboxMessageListItemDto(
    Guid Id,
    string Type,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    int AttemptCount,
    DateTime? LastAttemptAtUtc,
    string? Error,
    DateTime? LockedUntilUtc,
    string Status);

public sealed record OutboxMessageDetailDto(
    Guid Id,
    string Type,
    DateTime OccurredAtUtc,
    DateTime? ProcessedAtUtc,
    int AttemptCount,
    DateTime? LastAttemptAtUtc,
    string? Error,
    DateTime? LockedUntilUtc,
    string Status);

public sealed record OutboxMessagePageDto(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<OutboxMessageListItemDto> Items);

public sealed record OutboxMessageFilter(
    string? Status,
    string? Type,
    int Page,
    int PageSize);

public sealed record RetryFailedOutboxRequest(int Limit, string? Type);

public sealed record RetryFailedOutboxResultDto(
    int RequestedLimit,
    int ResetCount,
    string? Type,
    DateTime CompletedAtUtc);

public interface IOutboxOperationsQueries
{
    Task<OutboxStatusDto> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<OutboxMessagePageDto> ListAsync(
        OutboxMessageFilter filter,
        CancellationToken cancellationToken = default);

    Task<OutboxMessageDetailDto?> GetByIdAsync(
        Guid messageId,
        CancellationToken cancellationToken = default);
}

public interface IOutboxOperationsService
{
    Task<OutboxMessageDetailDto> RetryAsync(
        Guid messageId,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);

    Task<RetryFailedOutboxResultDto> RetryFailedAsync(
        RetryFailedOutboxRequest request,
        Guid? administratorId = null,
        CancellationToken cancellationToken = default);
}

public sealed class OutboxOperationsException : Exception
{
    public OutboxOperationsException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public OutboxOperationsException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class OutboxOperationsErrorCodes
{
    public const string MessageNotFound = "outbox_message_not_found";
    public const string MessageAlreadyProcessed = "outbox_message_already_processed";
    public const string MessageCurrentlyProcessing = "outbox_message_currently_processing";
    public const string StatusInvalid = "outbox_status_invalid";
    public const string PageInvalid = "outbox_page_invalid";
    public const string PageSizeInvalid = "outbox_page_size_invalid";
    public const string RetryLimitInvalid = "outbox_retry_limit_invalid";
    public const string OperationInvalid = "outbox_operation_invalid";
}
