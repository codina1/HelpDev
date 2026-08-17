using HelpDev.Modules.PromptLab.Domain;
using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.PromptLab.Tests;

public sealed class AiModelTests
{
    private static readonly DateTime Now = new(2026, 8, 17, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_trims_fields_and_starts_active()
    {
        var model = AiModel.Create(
            Guid.NewGuid(),
            "  ChatGPT  ",
            "ChatGPT",
            "  OpenAI  ",
            "  chatgpt  ",
            Now);

        Assert.Equal("ChatGPT", model.Name);
        Assert.Equal("chatgpt", model.Slug.Value);
        Assert.Equal("OpenAI", model.Provider);
        Assert.Equal("chatgpt", model.Logo);
        Assert.True(model.IsActive);
        Assert.Equal(Now, model.CreatedAtUtc);
        Assert.Equal(Now, model.UpdatedAtUtc);
    }

    [Fact]
    public void Create_rejects_invalid_fields()
    {
        var nameEx = Assert.Throws<DomainException>(() =>
            AiModel.Create(Guid.NewGuid(), " ", "chatgpt", "OpenAI", null, Now));
        Assert.Equal(PromptLabErrorCodes.AiModelNameRequired, nameEx.Code);

        var slugEx = Assert.Throws<DomainException>(() =>
            AiModel.Create(Guid.NewGuid(), "ChatGPT", "Bad Slug!", "OpenAI", null, Now));
        Assert.Equal(PromptLabErrorCodes.AiModelSlugInvalid, slugEx.Code);

        var providerEx = Assert.Throws<DomainException>(() =>
            AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", " ", null, Now));
        Assert.Equal(PromptLabErrorCodes.AiModelProviderRequired, providerEx.Code);

        var logoEx = Assert.Throws<DomainException>(() =>
            AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", "OpenAI", "<script>", Now));
        Assert.Equal(PromptLabErrorCodes.AiModelLogoInvalid, logoEx.Code);
    }

    [Fact]
    public void Admin_can_update_activate_and_deactivate()
    {
        var model = AiModel.Create(Guid.NewGuid(), "ChatGPT", "chatgpt", "OpenAI", "chatgpt", Now);

        Assert.True(model.UpdateDetails("GPT Chat", "OpenAI", "gpt", Now.AddMinutes(1)));
        Assert.Equal("GPT Chat", model.Name);
        Assert.Equal("gpt", model.Logo);
        Assert.Equal(Now.AddMinutes(1), model.UpdatedAtUtc);

        Assert.True(model.Deactivate(Now.AddMinutes(2)));
        Assert.False(model.IsActive);
        var inactive = Assert.Throws<DomainException>(model.EnsureActive);
        Assert.Equal(PromptLabErrorCodes.AiModelInactive, inactive.Code);

        Assert.True(model.Activate(Now.AddMinutes(3)));
        Assert.True(model.IsActive);
        model.EnsureActive();
    }

    [Fact]
    public void Catalog_creates_the_default_ai_models()
    {
        var models = AiModelCatalog.CreateDefaults(Now);

        Assert.Equal(
            new[] { "ChatGPT", "GPT Image", "Gemini", "Claude", "Midjourney", "Veo" },
            models.Select(model => model.Name));
        Assert.Equal(
            new[] { "chatgpt", "gpt-image", "gemini", "claude", "midjourney", "veo" },
            models.Select(model => model.Slug.Value));
        Assert.Equal(
            new[] { "OpenAI", "OpenAI", "Google", "Anthropic", "Midjourney", "Google" },
            models.Select(model => model.Provider));
        Assert.All(models, model => Assert.True(model.IsActive));
        Assert.Equal(AiModelCatalog.Defaults.Count, models.Count);
    }
}
