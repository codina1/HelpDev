using HelpDev.Modules.Content.Application.StructuredData;

namespace HelpDev.Content.Tests.SeoAnalysis;

public sealed class StructuredDataGeneratorTests
{
    private readonly StructuredDataGenerator _generator = new();

    [Fact]
    public void GenerateArticle_returns_null_when_headline_missing()
    {
        var result = _generator.GenerateArticle(
            new ArticleSchemaRequest(string.Empty, null, null, null, null, null));

        Assert.Null(result);
    }

    [Fact]
    public void GenerateArticle_builds_article_json_ld_dto()
    {
        var published = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var result = _generator.GenerateArticle(
            new ArticleSchemaRequest(
                "  Headline  ",
                "Desc",
                "https://example.com/cover.png",
                published,
                published,
                "Author"));

        Assert.NotNull(result);
        Assert.Equal("https://schema.org", result.Context);
        Assert.Equal("Article", result.Type);
        Assert.Equal("Headline", result.Headline);
        Assert.Equal("Desc", result.Description);
    }

    [Fact]
    public void GenerateSoftwareApplication_returns_null_when_name_missing()
    {
        var result = _generator.GenerateSoftwareApplication(
            new SoftwareApplicationSchemaRequest(string.Empty, null, null, null, null, null));

        Assert.Null(result);
    }

    [Fact]
    public void GenerateSoftwareApplication_builds_schema_foundation_dto()
    {
        var result = _generator.GenerateSoftwareApplication(
            new SoftwareApplicationSchemaRequest(
                "  Cursor  ",
                "AI IDE",
                "DeveloperApplication",
                "Windows, macOS, Web",
                "Freemium",
                "https://cursor.com"));

        Assert.NotNull(result);
        Assert.Equal("https://schema.org", result.Context);
        Assert.Equal("SoftwareApplication", result.Type);
        Assert.Equal("Cursor", result.Name);
        Assert.Equal("AI IDE", result.Description);
        Assert.Equal("https://cursor.com", result.Url);
    }

    [Fact]
    public void GenerateLearningRoadmap_returns_null_when_name_missing()
    {
        var result = _generator.GenerateLearningRoadmap(
            new LearningRoadmapSchemaRequest(string.Empty, null, null, null, null));

        Assert.Null(result);
    }

    [Fact]
    public void GenerateLearningRoadmap_builds_course_schema_foundation()
    {
        var result = _generator.GenerateLearningRoadmap(
            new LearningRoadmapSchemaRequest(
                "  Frontend Roadmap  ",
                "Learn frontend",
                "Beginner",
                "P12W",
                "https://helpdev.example/roadmaps/frontend"));

        Assert.NotNull(result);
        Assert.Equal("https://schema.org", result.Context);
        Assert.Equal("Course", result.Type);
        Assert.Equal("Frontend Roadmap", result.Name);
        Assert.Equal("Beginner", result.EducationalLevel);
    }
}
