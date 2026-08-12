using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Toolbox.Domain.Favorites;

public sealed class ToolFavorite : AggregateRoot<Guid>
{
    private ToolFavorite()
    {
    }

    private ToolFavorite(Guid id)
        : base(id)
    {
    }

    public Guid UserId { get; private set; }

    public Guid ToolId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static ToolFavorite Create(Guid id, Guid userId, Guid toolId, DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Favorite id must not be empty.", ToolboxErrorCodes.FavoriteInvalid);
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.", ToolboxErrorCodes.FavoriteRequiresAuthentication);
        }

        if (toolId == Guid.Empty)
        {
            throw new DomainException("Tool id is required.", ToolboxErrorCodes.FavoriteInvalid);
        }

        return new ToolFavorite(id)
        {
            UserId = userId,
            ToolId = toolId,
            CreatedAtUtc = utcNow,
        };
    }
}
