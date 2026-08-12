using HelpDev.Modules.Toolbox.Domain;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Toolbox.Tests;

public sealed class ToolFavoriteTests
{
    private static readonly DateTime Now = new(2026, 7, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_valid_favorite()
    {
        var userId = Guid.NewGuid();
        var toolId = Guid.NewGuid();

        var favorite = ToolFavorite.Create(Guid.NewGuid(), userId, toolId, Now);

        Assert.Equal(userId, favorite.UserId);
        Assert.Equal(toolId, favorite.ToolId);
        Assert.Equal(Now, favorite.CreatedAtUtc);
    }

    [Fact]
    public void Create_rejects_empty_user_or_tool()
    {
        var userEx = Assert.Throws<DomainException>(() =>
            ToolFavorite.Create(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Now));
        Assert.Equal(ToolboxErrorCodes.FavoriteRequiresAuthentication, userEx.Code);

        var toolEx = Assert.Throws<DomainException>(() =>
            ToolFavorite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Now));
        Assert.Equal(ToolboxErrorCodes.FavoriteInvalid, toolEx.Code);
    }
}
