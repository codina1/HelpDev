using HelpDev.Modules.PromptLab.Application;
using HelpDev.Modules.PromptLab.Application.Prompts;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.PromptLab.Application.Tests.Fakes;

namespace HelpDev.PromptLab.Application.Tests;

public sealed class PromptWriterServiceTests
{
    private static readonly Guid AuthorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherAuthorId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly FakePromptRepository _prompts = new();
    private readonly FakePromptCategoryRepository _categories = new();
    private readonly FakeAiModelRepository _aiModels = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeDateTimeProvider _clock = new(new DateTime(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc));
    private readonly PromptWriterService _sut;

    public PromptWriterServiceTests()
    {
        _sut = ServiceFactory.CreateWriterService(_prompts, _categories, _aiModels, _unitOfWork, _clock);
    }

    [Fact]
    public async Task Create_starts_as_draft_owned_by_writer()
    {
        var category = SeedCategory();
        var model = SeedModel();

        var dto = await _sut.CreateAsync(AuthorId, CreateRequest(category.Id, model.Id));

        Assert.Equal(nameof(PromptStatus.Draft), dto.Status);
        Assert.Null(dto.PublishedAt);
        Assert.Equal(AuthorId, _prompts.Items.Single().AuthorId);
        Assert.Equal(1, _prompts.AddCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
        Assert.False(_prompts.Items.Single().IsPublic);
    }

    [Fact]
    public async Task Submit_moves_draft_to_submitted_without_publishing()
    {
        var prompt = SeedOwnedDraft();

        var dto = await _sut.SubmitAsync(AuthorId, prompt.Id);

        Assert.Equal(nameof(PromptStatus.Submitted), dto.Status);
        Assert.Equal(PromptStatus.Submitted, prompt.Status);
        Assert.Null(dto.PublishedAt);
        Assert.Null(prompt.PublishedAt);
        Assert.False(prompt.IsPublic);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Writer_cannot_read_or_submit_another_authors_prompt()
    {
        var prompt = SeedOwnedDraft(OtherAuthorId);

        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => _sut.SubmitAsync(AuthorId, prompt.Id));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, missing.Code);
        Assert.Equal(PromptStatus.Draft, prompt.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Writer_cannot_update_another_authors_prompt()
    {
        var prompt = SeedOwnedDraft(OtherAuthorId);
        var request = CreateUpdateRequest(prompt);

        var missing = await Assert.ThrowsAsync<PromptLabException>(
            () => _sut.UpdateAsync(AuthorId, prompt.Id, request));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotFound, missing.Code);
        Assert.Equal("Review helper", prompt.Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Submitted_prompt_cannot_be_edited()
    {
        var prompt = SeedOwnedDraft();
        prompt.Submit(AuthorId, _clock.UtcNow);
        var request = CreateUpdateRequest(prompt) with { Title = "Changed after submit" };

        var ex = await Assert.ThrowsAsync<PromptLabException>(
            () => _sut.UpdateAsync(AuthorId, prompt.Id, request));
        Assert.Equal(PromptLabApplicationErrorCodes.PromptNotDraft, ex.Code);
        Assert.Equal("Review helper", prompt.Title);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Submit_does_not_approve()
    {
        var prompt = SeedOwnedDraft();

        var dto = await _sut.SubmitAsync(AuthorId, prompt.Id);

        Assert.NotEqual(nameof(PromptStatus.Approved), dto.Status);
        Assert.NotEqual(PromptStatus.Approved, prompt.Status);
    }

    private Prompt SeedOwnedDraft(Guid? authorId = null)
    {
        var category = SeedCategory();
        var model = SeedModel();
        var prompt = Prompt.Create(
            Guid.NewGuid(),
            "Review helper",
            "review-helper",
            "Helps review code",
            "Review {{code}}",
            null,
            PromptMediaType.Text,
            model,
            category,
            authorId ?? AuthorId,
            _clock.UtcNow);
        _prompts.Seed(prompt);
        return prompt;
    }

    private HelpDev.Modules.PromptLab.Domain.Categories.PromptCategory SeedCategory()
    {
        var category = ServiceFactory.CreateActiveCategory(_clock.UtcNow);
        _categories.Seed(category);
        return category;
    }

    private AiModel SeedModel()
    {
        var model = AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", "OpenAI", "chatgpt", _clock.UtcNow);
        _aiModels.Seed(model);
        return model;
    }

    private static CreateWriterPromptRequest CreateRequest(Guid categoryId, Guid aiModelId) =>
        new(
            "Review helper",
            "review-helper",
            "Helps review code",
            "Review {{code}}",
            null,
            nameof(PromptMediaType.Text),
            categoryId,
            aiModelId);

    private static UpdateWriterPromptRequest CreateUpdateRequest(Prompt prompt) =>
        new(
            prompt.Title,
            prompt.Slug.Value,
            prompt.Description,
            prompt.Content,
            prompt.CoverImage,
            prompt.MediaType.ToString(),
            prompt.CategoryId,
            prompt.AiModelId);
}
