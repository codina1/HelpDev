namespace HelpDev.SharedKernel.Common;

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    protected AuditableEntity(TId id)
        : base(id)
    {
    }

    protected AuditableEntity()
    {
    }
}
