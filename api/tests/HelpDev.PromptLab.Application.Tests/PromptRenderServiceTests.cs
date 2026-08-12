using System.Text.Json;
using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Rendering;
using HelpDev.Modules.PromptLab.Domain;
using HelpDev.PromptLab.Application.Tests.Fakes;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptRenderServiceTests
{
    private readonly FakePromptDefinitionRepository _promptRepository = new();
    private readonly FakePromptCategoryRepository _categoryRepository = new();
    private readonly FakePromptRenderRecordRepository _renderRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptRenderService _sut;

    public PromptRenderServiceTests()
    {
        _sut = ServiceFactory.CreateRenderService(
            _promptRepository,
            _categoryRepository,
            _renderRepository,
            _unitOfWork,
            _clock);
    }

    [Fact]
    public async Task Render_published_version_only_succeeds()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow, category.Id);
        _promptRepository.Seed(prompt);

        var result = await _sut.RenderAsync(
            prompt.Slug.Value,
            new RenderPromptRequest(ServiceFactory.RenderValues("class Foo {}")));

        Assert.True(result.Succeeded);
        Assert.Contains("class Foo {}", result.RenderedText, StringComparison.Ordinal);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Render_unpublished_throws_not_found()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreateDraftPrompt(_clock.UtcNow, category.Id);
        _promptRepository.Seed(prompt);

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.RenderAsync(prompt.Slug.Value, new RenderPromptRequest(ServiceFactory.RenderValues("x"))));

        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, ex.Code);
    }

    [Fact]
    public async Task Render_requires_authentication()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreatePublishedPrompt(
            _clock.UtcNow,
            category.Id,
            requiresAuthentication: true);
        _promptRepository.Seed(prompt);

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.RenderAsync(prompt.Slug.Value, new RenderPromptRequest(ServiceFactory.RenderValues("x"))));

        Assert.Equal(PromptLabApplicationErrorCodes.RenderRequiresAuthentication, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Render_without_history_does_not_commit()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow, category.Id, allowHistory: false);
        _promptRepository.Seed(prompt);

        var result = await _sut.RenderAsync(
            prompt.Slug.Value,
            new RenderPromptRequest(ServiceFactory.RenderValues("hi")),
            Guid.NewGuid());

        Assert.True(result.Succeeded);
        Assert.Null(result.RenderId);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
        Assert.Equal(0, _renderRepository.AddCallCount);
    }

    [Fact]
    public async Task Render_with_history_and_user_commits_once()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow, category.Id, allowHistory: true);
        _promptRepository.Seed(prompt);
        var userId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        var result = await _sut.RenderAsync(
            prompt.Slug.Value,
            new RenderPromptRequest(ServiceFactory.RenderValues("hi")),
            userId,
            cts.Token);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.RenderId);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.Equal(1, _renderRepository.AddCallCount);
        Assert.Equal(cts.Token, _unitOfWork.LastToken);
        Assert.Equal(cts.Token, _renderRepository.LastToken);
    }

    [Fact]
    public async Task Render_oversized_value_is_wrapped()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categoryRepository.Seed(category);
        var prompt = ServiceFactory.CreatePublishedPrompt(_clock.UtcNow, category.Id);
        _promptRepository.Seed(prompt);

        var oversized = new string('x', PromptLabLimits.MaxVariableValueLength + 1);
        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
        {
            ["code"] = JsonSerializer.SerializeToElement(oversized),
        };

        var ex = await Assert.ThrowsAsync<PromptLabException>(() =>
            _sut.RenderAsync(prompt.Slug.Value, new RenderPromptRequest(values)));

        Assert.Equal(PromptLabApplicationErrorCodes.RenderValueTooLong, ex.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
