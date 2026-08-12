using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Categories;
using HelpDev.Modules.Content.Domain.News;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Content.Tests.Articles;

public sealed class ArticleMetadataTests
{
    [Fact]
    public void Create_rejects_non_positive_reading_time()
    {
        var ex = Assert.Throws<DomainException>(() =>
            ArticleMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                DifficultyLevel.Beginner,
                0,
                false,
                true,
                true,
                DateTime.UtcNow));

        Assert.Contains("زمان مطالعه", ex.Message);
    }

    [Fact]
    public void Update_changes_settings_without_touching_content_id()
    {
        var contentId = Guid.NewGuid();
        var metadata = ArticleMetadata.Create(
            Guid.NewGuid(),
            contentId,
            null,
            DifficultyLevel.Beginner,
            5,
            false,
            true,
            true,
            DateTime.UtcNow);

        var updatedAt = DateTime.UtcNow.AddMinutes(1);
        metadata.Update(
            Guid.NewGuid(),
            DifficultyLevel.Advanced,
            12,
            true,
            false,
            false,
            updatedAt);

        Assert.Equal(contentId, metadata.ContentId);
        Assert.Equal(DifficultyLevel.Advanced, metadata.DifficultyLevel);
        Assert.Equal(12, metadata.ReadingTimeMinutes);
        Assert.True(metadata.IsFeatured);
        Assert.False(metadata.AllowComments);
        Assert.False(metadata.TableOfContentsEnabled);
        Assert.Equal(updatedAt, metadata.UpdatedAtUtc);
    }

    [Fact]
    public void Content_category_foundation_validates_name_and_slug()
    {
        var category = ContentCategory.Create(
            Guid.NewGuid(),
            "Backend",
            "backend",
            DateTime.UtcNow);

        Assert.Equal("Backend", category.Name);
        Assert.Equal("backend", category.Slug);

        Assert.Throws<DomainException>(() =>
            ContentCategory.Create(Guid.NewGuid(), " ", "x", DateTime.UtcNow));
    }
}

public sealed class NewsMetadataTests
{
    [Fact]
    public void Create_requires_source_name()
    {
        var ex = Assert.Throws<DomainException>(() =>
            NewsMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "  ",
                null,
                DateTime.UtcNow,
                NewsPriority.Normal,
                null,
                DateTime.UtcNow));

        Assert.Contains("منبع", ex.Message);
    }

    [Fact]
    public void Create_rejects_invalid_source_url()
    {
        Assert.Throws<DomainException>(() =>
            NewsMetadata.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Wire",
                "not-a-url",
                DateTime.UtcNow,
                NewsPriority.Featured,
                null,
                DateTime.UtcNow));
    }

    [Fact]
    public void Update_accepts_breaking_priority_and_https_url()
    {
        var metadata = NewsMetadata.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Wire",
            "https://helpdev.example/source",
            DateTime.UtcNow,
            NewsPriority.Normal,
            null,
            DateTime.UtcNow);

        metadata.Update(
            "Agency",
            "https://helpdev.example/agency",
            DateTime.UtcNow.AddDays(-1),
            NewsPriority.Breaking,
            "ext-1",
            DateTime.UtcNow);

        Assert.Equal("Agency", metadata.SourceName);
        Assert.Equal(NewsPriority.Breaking, metadata.Priority);
        Assert.Equal("ext-1", metadata.ExternalReference);
    }
}
