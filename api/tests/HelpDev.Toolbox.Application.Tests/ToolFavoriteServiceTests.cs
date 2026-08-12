using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Favorites;
using HelpDev.Modules.Toolbox.Domain.Favorites;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Toolbox.Application.Tests.Fakes;

namespace HelpDev.Toolbox.Application.Tests;

public sealed class ToolFavoriteServiceTests
{
    private readonly FakeToolFavoriteRepository _favoriteRepository = new();
    private readonly FakeToolDefinitionRepository _toolRepository = new();
    private readonly FakeToolFavoriteQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly ToolFavoriteService _sut;

    public ToolFavoriteServiceTests()
    {
        _sut = ServiceFactory.CreateFavoriteService(
            _favoriteRepository,
            _toolRepository,
            _queries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task Add_commits_once()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow);
        _toolRepository.Seed(tool);
        var userId = Guid.NewGuid();

        await _sut.AddAsync(userId, tool.Id);

        Assert.Equal(1, _favoriteRepository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Add_idempotent_does_not_commit_second_time()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow);
        _toolRepository.Seed(tool);
        var userId = Guid.NewGuid();

        await _sut.AddAsync(userId, tool.Id);
        await _sut.AddAsync(userId, tool.Id);

        Assert.Equal(1, _favoriteRepository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Remove_is_idempotent()
    {
        var tool = ServiceFactory.CreatePublishedTool(_clock.UtcNow);
        _toolRepository.Seed(tool);
        var userId = Guid.NewGuid();
        var favorite = ToolFavorite.Create(Guid.NewGuid(), userId, tool.Id, _clock.UtcNow);
        _favoriteRepository.Seed(favorite);

        await _sut.RemoveAsync(userId, tool.Id);
        Assert.Equal(1, _favoriteRepository.RemoveCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        await _sut.RemoveAsync(userId, tool.Id);
        Assert.Equal(1, _favoriteRepository.RemoveCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Add_rejects_unpublished_tool()
    {
        var tool = ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Draft",
            "draft-tool",
            "Draft",
            null,
            ToolType.TextStatistics,
            ServiceFactory.DefaultSchema,
            null,
            false,
            false,
            0,
            _clock.UtcNow);
        _toolRepository.Seed(tool);

        var ex = await Assert.ThrowsAsync<ToolboxException>(() =>
            _sut.AddAsync(Guid.NewGuid(), tool.Id));

        Assert.Equal(ToolboxApplicationErrorCodes.ToolNotFound, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
