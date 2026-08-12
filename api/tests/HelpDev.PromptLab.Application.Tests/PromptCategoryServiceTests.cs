using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Categories;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.PromptLab.Application.Tests.Fakes;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptCategoryServiceTests
{
    private readonly FakePromptCategoryRepository _repository = new();
    private readonly FakePromptCategoryQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptCategoryService _sut;

    public PromptCategoryServiceTests()
    {
        _sut = ServiceFactory.CreateCategoryService(_repository, _queries, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_commits_once_and_forwards_cancellation_token()
    {
        using var cts = new CancellationTokenSource();

        var dto = await _sut.CreateAsync(
            new CreatePromptCategoryRequest("Coding", "coding", null, null, 1),
            Guid.NewGuid(),
            cts.Token);

        Assert.Equal("coding", dto.Slug);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
        Assert.Equal(cts.Token, _repository.LastToken);
    }

    [Fact]
    public async Task Create_duplicate_slug_does_not_commit()
    {
        _repository.Seed(PromptCategory.Create(
            Guid.NewGuid(),
            "Coding",
            "coding",
            null,
            null,
            0,
            _clock.UtcNow));

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.CreateAsync(new CreatePromptCategoryRequest("Other", "coding", null, null, 0)));

        Assert.Equal(PromptLabApplicationErrorCodes.CategorySlugDuplicate, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(0, _repository.AddCallCount);
    }

    [Fact]
    public async Task Activate_commits_once_and_noop_does_not_commit()
    {
        var category = PromptCategory.Create(
            Guid.NewGuid(),
            "Coding",
            "coding",
            null,
            null,
            0,
            _clock.UtcNow);
        category.Deactivate(_clock.UtcNow);
        _repository.Seed(category);

        await _sut.ActivateAsync(category.Id);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.True(category.IsActive);

        await _sut.ActivateAsync(category.Id);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }
}
