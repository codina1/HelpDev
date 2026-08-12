using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Favorites;

public sealed class PromptFavorite : AggregateRoot<Guid>
{
    private PromptFavorite()
    {
    }

    private PromptFavorite(Guid id)
        : base(id)
    {
    }

    public Guid UserId { get; private set; }

    public Guid PromptDefinitionId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static PromptFavorite Create(Guid id, Guid userId, Guid promptDefinitionId, DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Favorite id must not be empty.", PromptLabErrorCodes.FavoriteInvalid);
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.", PromptLabErrorCodes.FavoriteRequiresAuthentication);
        }

        if (promptDefinitionId == Guid.Empty)
        {
            throw new DomainException("Prompt id is required.", PromptLabErrorCodes.FavoriteInvalid);
        }

        return new PromptFavorite(id)
        {
            UserId = userId,
            PromptDefinitionId = promptDefinitionId,
            CreatedAtUtc = utcNow,
        };
    }
}
