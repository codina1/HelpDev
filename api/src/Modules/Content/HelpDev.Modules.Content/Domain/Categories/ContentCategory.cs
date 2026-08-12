using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Categories;

/// <summary>
/// Extension point for content taxonomy / categories.
/// Full taxonomy engine is intentionally out of scope for Article/News CMS v1.
/// <see cref="Articles.ArticleMetadata.CategoryId"/> may reference a future catalog row.
/// </summary>
public sealed class ContentCategory
{
    public const int MaxNameLength = 120;
    public const int MaxSlugLength = 160;

    private ContentCategory()
    {
    }

    private ContentCategory(Guid id, string name, string slug, DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Slug = slug;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Factory for in-memory / future persistence. No DbSet in v1.
    /// </summary>
    public static ContentCategory Create(Guid id, string name, string slug, DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه دسته الزامی است.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("نام دسته الزامی است.");
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > MaxNameLength)
        {
            throw new DomainException("نام دسته بیش از حد مجاز است.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("اسلاگ دسته الزامی است.");
        }

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        if (normalizedSlug.Length > MaxSlugLength)
        {
            throw new DomainException("اسلاگ دسته بیش از حد مجاز است.");
        }

        return new ContentCategory(id, normalizedName, normalizedSlug, createdAtUtc);
    }
}
