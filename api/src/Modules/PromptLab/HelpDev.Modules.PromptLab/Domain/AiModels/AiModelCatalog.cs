namespace HelpDev.Modules.PromptLab.Domain.AiModels;

public static class AiModelCatalog
{
    public static readonly IReadOnlyList<AiModelSeed> Defaults =
    [
        new("ChatGPT", "chatgpt", "OpenAI", "chatgpt"),
        new("GPT Image", "gpt-image", "OpenAI", "gpt-image"),
        new("Gemini", "gemini", "Google", "gemini"),
        new("Claude", "claude", "Anthropic", "claude"),
        new("Midjourney", "midjourney", "Midjourney", "midjourney"),
        new("Veo", "veo", "Google", "veo"),
    ];

    public static IReadOnlyList<AiModel> CreateDefaults(DateTime utcNow)
    {
        return Defaults
            .Select(seed => AiModel.Create(
                Guid.NewGuid(),
                seed.Name,
                seed.Slug,
                seed.Provider,
                seed.Logo,
                utcNow))
            .ToArray();
    }

    public sealed record AiModelSeed(string Name, string Slug, string Provider, string Logo);
}
