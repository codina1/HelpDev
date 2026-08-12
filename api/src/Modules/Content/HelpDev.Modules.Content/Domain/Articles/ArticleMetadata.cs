using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Articles;

/// <summary>
/// Article-specific metadata satellite. Content owns lifecycle; this entity holds
/// only article settings (category, difficulty, reading time, featured, comments, TOC).
/// No EF dependencies.
/// </summary>
public sealed class ArticleMetadata
{
    public const int MinReadingTimeMinutes = 1;
    public const int MaxReadingTimeMinutes = 600;

    /// <summary>Required for EF Core materialization.</summary>
    private ArticleMetadata()
    {
    }

    private ArticleMetadata(
        Guid id,
        Guid contentId,
        Guid? categoryId,
        DifficultyLevel difficultyLevel,
        int readingTimeMinutes,
        bool isFeatured,
        bool allowComments,
        bool tableOfContentsEnabled,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        ContentId = contentId;
        CategoryId = categoryId;
        DifficultyLevel = difficultyLevel;
        ReadingTimeMinutes = readingTimeMinutes;
        IsFeatured = isFeatured;
        AllowComments = allowComments;
        TableOfContentsEnabled = tableOfContentsEnabled;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    /// <summary>Optional reference to <see cref="Categories.ContentCategory"/> (taxonomy engine not in v1).</summary>
    public Guid? CategoryId { get; private set; }

    public DifficultyLevel DifficultyLevel { get; private set; }

    public int ReadingTimeMinutes { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool AllowComments { get; private set; }

    public bool TableOfContentsEnabled { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static ArticleMetadata Create(
        Guid id,
        Guid contentId,
        Guid? categoryId,
        DifficultyLevel difficultyLevel,
        int readingTimeMinutes,
        bool isFeatured,
        bool allowComments,
        bool tableOfContentsEnabled,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه متادیتای مقاله الزامی است.");
        }

        if (contentId == Guid.Empty)
        {
            throw new DomainException("شناسه محتوا الزامی است.");
        }

        if (!Enum.IsDefined(difficultyLevel))
        {
            throw new DomainException("سطح دشواری معتبر نیست.");
        }

        ValidateReadingTime(readingTimeMinutes);
        ValidateCategoryId(categoryId);

        return new ArticleMetadata(
            id,
            contentId,
            categoryId,
            difficultyLevel,
            readingTimeMinutes,
            isFeatured,
            allowComments,
            tableOfContentsEnabled,
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(
        Guid? categoryId,
        DifficultyLevel difficultyLevel,
        int readingTimeMinutes,
        bool isFeatured,
        bool allowComments,
        bool tableOfContentsEnabled,
        DateTime updatedAtUtc)
    {
        if (!Enum.IsDefined(difficultyLevel))
        {
            throw new DomainException("سطح دشواری معتبر نیست.");
        }

        ValidateReadingTime(readingTimeMinutes);
        ValidateCategoryId(categoryId);

        CategoryId = categoryId;
        DifficultyLevel = difficultyLevel;
        ReadingTimeMinutes = readingTimeMinutes;
        IsFeatured = isFeatured;
        AllowComments = allowComments;
        TableOfContentsEnabled = tableOfContentsEnabled;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static void ValidateReadingTime(int readingTimeMinutes)
    {
        if (readingTimeMinutes < MinReadingTimeMinutes)
        {
            throw new DomainException("زمان مطالعه باید بزرگ‌تر از صفر باشد.");
        }

        if (readingTimeMinutes > MaxReadingTimeMinutes)
        {
            throw new DomainException("زمان مطالعه بیش از حد مجاز است.");
        }
    }

    private static void ValidateCategoryId(Guid? categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("شناسه دسته معتبر نیست.");
        }
    }
}
