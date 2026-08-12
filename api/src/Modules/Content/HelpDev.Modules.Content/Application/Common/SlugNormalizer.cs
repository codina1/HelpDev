using HelpDev.Modules.Content.Domain.ValueObjects;

namespace HelpDev.Modules.Content.Application.Common;

public static class SlugNormalizer
{
    public static bool TryNormalize(string? slug, out string normalized)
    {
        normalized = string.Empty;

        if (!Slug.TryCreate(slug, out var created) || created is null)
        {
            return false;
        }

        normalized = created.Value;
        return true;
    }
}