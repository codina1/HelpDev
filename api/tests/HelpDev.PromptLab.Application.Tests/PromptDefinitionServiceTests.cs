using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.Categories;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.PromptLab.Application.Tests.Fakes;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptDefinitionServiceTests
{
    private readonly FakePromptDefinitionRepository _repository = new();
    private readonly FakePromptCategoryRepository _categoryRepository = new();
    private readonly FakePromptDefinitionQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptDefinitionService _sut;

    public PromptDefinitionServiceTests()
    {
        _sut = ServiceFactory.CreateDefinitionService(
            _repository,
            _categoryRepository,
            _queries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task CreateVersion_commits_once()
    {
        var category = SeedActiveCategory();
        var prompt = ServiceFactory.CreateDraftPrompt(_clock.UtcNow, category.Id);
        _repository.Seed(prompt);

        var dto = await _sut.CreateVersionAsync(
            prompt.Id,
            new CreatePromptVersionRequest(
                "Review {{code}}",
                "initial",
                [
                    new CreatePromptVariableRequest(
                        "code",
                        "Code",
                        null,
                        nameof(PromptVariableType.MultilineText),
                        true,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        0),
                ]));

        Assert.Equal(1, dto.VersionNumber);
        Assert.Equal(1, prompt.LatestVersionNumber);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Publish_with_inactive_category_fails_without_commit()
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
        _categoryRepository.Seed(category);

        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow, category.Id);
        // Unpublish so we can attempt publish again after seeding with version.
        prompt.Unpublish(_clock.UtcNow);
        _repository.Seed(prompt);

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.PublishVersionAsync(prompt.Id, 1));

        Assert.Equal(PromptLabApplicationErrorCodes.CategoryInactive, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.False(prompt.IsPublished);
    }

    [Fact]
    public async Task Publish_success_commits_once()
    {
        var category = SeedActiveCategory();
        var prompt = ServiceFactory.CreateDraftPrompt(_clock.UtcNow, category.Id);
        var versionId = Guid.NewGuid();
        prompt.RegisterVersion(
            versionId,
            "Review {{code}}",
            null,
            null,
            [
                PromptVariable.Create(
                    Guid.NewGuid(),
                    versionId,
                    "code",
                    "Code",
                    null,
                    PromptVariableType.MultilineText,
                    true,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    0),
            ],
            ["code"],
            _clock.UtcNow);
        _repository.Seed(prompt);

        var dto = await _sut.PublishVersionAsync(prompt.Id, 1);

        Assert.True(dto.IsPublished);
        Assert.Equal(1, dto.PublishedVersionNumber);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    private PromptCategory SeedActiveCategory()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        return category;
    }
}
