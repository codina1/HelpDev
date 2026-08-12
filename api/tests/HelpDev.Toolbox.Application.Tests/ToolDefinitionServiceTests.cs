using HelpDev.Modules.Toolbox.Application.Execution;
using HelpDev.Modules.Toolbox.Application.Tools;
using HelpDev.Modules.Toolbox.Domain.Categories;
using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.Toolbox.Application.Tests.Fakes;

namespace HelpDev.Toolbox.Application.Tests;

public sealed class ToolDefinitionServiceTests
{
    private readonly FakeToolDefinitionRepository _repository = new();
    private readonly FakeToolCategoryRepository _categoryRepository = new();
    private readonly FakeToolDefinitionQueries _queries = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly ToolDefinitionService _sut;

    public ToolDefinitionServiceTests()
    {
        _sut = ServiceFactory.CreateToolService(
            _repository,
            _categoryRepository,
            _queries,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task CreateDraft_commits_once()
    {
        var category = SeedActiveCategory();

        var dto = await _sut.CreateDraftAsync(new CreateToolDefinitionRequest(
            category.Id,
            "JSON Formatter",
            "json-formatter",
            "Formats JSON",
            null,
            nameof(ToolType.JsonFormatter),
            ServiceFactory.DefaultSchema,
            null,
            RequiresAuthentication: false,
            AllowHistory: false,
            DisplayOrder: 0));

        Assert.False(dto.IsPublished);
        Assert.Equal(1, _repository.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Publish_with_inactive_category_fails_without_commit()
    {
        var category = ToolCategory.Create(
            Guid.NewGuid(),
            "Encoding",
            "encoding",
            null,
            null,
            0,
            _clock.UtcNow);
        category.Deactivate(_clock.UtcNow);
        _categoryRepository.Seed(category);

        var tool = ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            category.Id,
            "JSON Formatter",
            "json-formatter",
            "Formats JSON",
            null,
            ToolType.JsonFormatter,
            ServiceFactory.DefaultSchema,
            null,
            false,
            false,
            0,
            _clock.UtcNow);
        _repository.Seed(tool);

        var ex = await Assert.ThrowsAsync<ToolboxException>(() => _sut.PublishAsync(tool.Id));

        Assert.Equal(ToolboxApplicationErrorCodes.CategoryInactive, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.False(tool.IsPublished);
    }

    [Fact]
    public async Task Disable_commits_once()
    {
        var category = SeedActiveCategory();
        var tool = ToolDefinition.CreateDraft(
            Guid.NewGuid(),
            category.Id,
            "JSON Formatter",
            "json-formatter",
            "Formats JSON",
            null,
            ToolType.JsonFormatter,
            ServiceFactory.DefaultSchema,
            null,
            false,
            false,
            0,
            _clock.UtcNow);
        _repository.Seed(tool);

        var dto = await _sut.DisableAsync(tool.Id);

        Assert.False(dto.IsEnabled);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    private ToolCategory SeedActiveCategory()
    {
        var category = ToolCategory.Create(
            Guid.NewGuid(),
            "Encoding",
            "encoding",
            null,
            null,
            0,
            _clock.UtcNow);
        _categoryRepository.Seed(category);
        return category;
    }
}
