namespace HelpDev.Infrastructure.Outbox;

/// <summary>
/// Infrastructure persistence record for the transactional outbox.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime? ProcessedAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? LastAttemptAtUtc { get; set; }

    public string? Error { get; set; }

    public DateTime? LockedUntilUtc { get; set; }

    public string? LockId { get; set; }

    /// <summary>
    /// Clears processing state so the OutboxProcessor can claim the message again.
    /// Does not dispatch and does not modify identity, type, payload, or occurrence time.
    /// </summary>
    public void ResetForRetry(DateTime nowUtc)
    {
        if (ProcessedAtUtc is not null)
        {
            throw new InvalidOperationException("Processed outbox messages cannot be reset for retry.");
        }

        if (LockedUntilUtc is not null && LockedUntilUtc > nowUtc)
        {
            throw new InvalidOperationException("Actively locked outbox messages cannot be reset for retry.");
        }

        AttemptCount = 0;
        LastAttemptAtUtc = null;
        Error = null;
        LockedUntilUtc = null;
        LockId = null;
    }
}
