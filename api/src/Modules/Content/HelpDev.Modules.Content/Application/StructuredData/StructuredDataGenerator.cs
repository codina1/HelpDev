namespace HelpDev.Modules.Content.Application.StructuredData;

public sealed class StructuredDataGenerator : IStructuredDataGenerator
{
    public ArticleSchemaDto? GenerateArticle(ArticleSchemaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var headline = request.Headline?.Trim();
        if (string.IsNullOrEmpty(headline))
        {
            return null;
        }

        return new ArticleSchemaDto(
            Context: "https://schema.org",
            Type: "Article",
            Headline: headline,
            Description: NormalizeOptional(request.Description),
            Image: NormalizeOptional(request.ImageUrl),
            DatePublished: FormatUtc(request.DatePublishedUtc),
            DateModified: FormatUtc(request.DateModifiedUtc),
            AuthorName: NormalizeOptional(request.AuthorName));
    }

    public SoftwareApplicationSchemaDto? GenerateSoftwareApplication(SoftwareApplicationSchemaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return new SoftwareApplicationSchemaDto(
            Context: "https://schema.org",
            Type: "SoftwareApplication",
            Name: name,
            Description: NormalizeOptional(request.Description),
            ApplicationCategory: NormalizeOptional(request.ApplicationCategory),
            OperatingSystem: NormalizeOptional(request.OperatingSystem),
            Offers: NormalizeOptional(request.OffersPrice),
            Url: NormalizeOptional(request.Url));
    }

    public LearningRoadmapSchemaDto? GenerateLearningRoadmap(LearningRoadmapSchemaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return new LearningRoadmapSchemaDto(
            Context: "https://schema.org",
            Type: "Course",
            Name: name,
            Description: NormalizeOptional(request.Description),
            EducationalLevel: NormalizeOptional(request.EducationalLevel),
            TimeRequired: NormalizeOptional(request.TimeRequired),
            Url: NormalizeOptional(request.Url));
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string? FormatUtc(DateTime? value) =>
        value.HasValue ? value.Value.ToUniversalTime().ToString("O") : null;
}
