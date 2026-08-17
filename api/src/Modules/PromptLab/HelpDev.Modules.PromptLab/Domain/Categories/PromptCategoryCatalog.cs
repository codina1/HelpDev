namespace HelpDev.Modules.PromptLab.Domain.Categories;

public static class PromptCategoryCatalog
{
    public static readonly IReadOnlyList<PromptCategorySeed> Defaults =
    [
        new("Image", "image", "image"),
        new("Video", "video", "video"),
        new("Coding", "coding", "code"),
        new("Writing", "writing", "pen"),
        new("Marketing", "marketing", "megaphone"),
        new("Design", "design", "palette"),
        new("Education", "education", "book"),
    ];

    public static IReadOnlyList<PromptCategory> CreateDefaults(DateTime utcNow)
    {
        return Defaults
            .Select((seed, index) => PromptCategory.Create(
                Guid.NewGuid(),
                seed.Name,
                seed.Slug,
                description: null,
                seed.Icon,
                index,
                utcNow))
            .ToArray();
    }

    public sealed record PromptCategorySeed(string Name, string Slug, string Icon);
}
