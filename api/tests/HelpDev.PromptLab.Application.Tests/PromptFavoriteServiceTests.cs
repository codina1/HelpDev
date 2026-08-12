using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Favorites;
using HelpDev.Modules.PromptLab.Domain.Favorites;
using HelpDev.PromptLab.Application.Tests.Fakes;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptFavoriteServiceTests
{
    private readonly FakePromptFavoriteRepository _favoriteRepository = new();
    private readonly FakePromptDefinitionRepository _promptRepository = new();
    private readonly FakePromptFavoriteQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptFavoriteService _sut;

    public PromptFavoriteServiceTests()
    {
        _sut = ServiceFactory.CreateFavoriteService(
            _favoriteRepository,
            _promptRepository,
            _queries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task Add_commits_once()
    {
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow);
        _promptRepository.Seed(prompt);
        var userId = Guid.NewGuid();

        await _sut.AddAsync(userId, prompt.Id);

        Assert.Equal(1, _favoriteRepository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Add_idempotent_does_not_commit_second_time()
    {
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow);
        _promptRepository.Seed(prompt);
        var userId = Guid.NewGuid();

        await _sut.AddAsync(userId, prompt.Id);
        await _sut.AddAsync(userId, prompt.Id);

        Assert.Equal(1, _favoriteRepository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Remove_is_idempotent()
    {
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow);
        _promptRepository.Seed(prompt);
        var userId = Guid.NewGuid();
        var favorite = PromptFavorite.Create(Guid.NewGuid(), userId, prompt.Id, _clock.UtcNow);
        _favoriteRepository.Seed(favorite);

        await _sut.RemoveAsync(userId, prompt.Id);
        Assert.Equal(1, _favoriteRepository.RemoveCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);

        await _sut.RemoveAsync(userId, prompt.Id);
        Assert.Equal(1, _favoriteRepository.RemoveCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Add_rejects_unpublished_prompt()
    {
        var prompt = ServiceFactory.CreateDraftPrompt(_clock.UtcNow, Guid.NewGuid());
        _promptRepository.Seed(prompt);

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.AddAsync(Guid.NewGuid(), prompt.Id));

        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
