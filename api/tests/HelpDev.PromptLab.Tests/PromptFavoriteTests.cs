using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class PromptFavoriteTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_valid_favorite()
    {
        var userId = Guid.NewGuid();
        var promptId = Guid.NewGuid();

        var favorite = PromptFavorite.Create(Guid.NewGuid(), userId, promptId, Now);

        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(promptId, favorite.PromptDefinitionId);
        Assert.Equal(Now, favorite.CreatedAtUtc);
    }

    [Fact]
    public void Create_rejects_empty_user_or_prompt()
    {
        var userEx = Assert.Throws<DomainException>(() =>
            PromptFavorite.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Now));
        Assert.Equal(PromptLabErrorCodes.FavoriteRequiresAuthentication, userEx.Code);

        var promptEx = Assert.Throws<DomainException>(() =>
            PromptFavorite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Now));
        Assert.Equal(PromptLabErrorCodes.FavoriteInvalid, promptEx.Code);
    }
}
