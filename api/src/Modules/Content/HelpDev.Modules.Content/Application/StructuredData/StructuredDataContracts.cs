namespace HelpDev.Modules.Content.Application.StructuredData;

/// <summary>Generates JSON-LD shaped DTOs from known content fields. No page injection in v1.</summary>
public interface IStructuredDataGenerator
{
    ArticleSchemaDto? GenerateArticle(ArticleSchemaRequest request);

    /// <summary>SoftwareApplication schema foundation for Tool Library (not injected publicly yet).</summary>
    SoftwareApplicationSchemaDto? GenerateSoftwareApplication(SoftwareApplicationSchemaRequest request);

    /// <summary>Course / LearningRoadmap schema foundation for Roadmap Engine (not injected publicly yet).</summary>
    LearningRoadmapSchemaDto? GenerateLearningRoadmap(LearningRoadmapSchemaRequest request);
}

public sealed record ArticleSchemaRequest(
    string Headline,
    string? Description,
    string? ImageUrl,
    DateTime? DatePublishedUtc,
    DateTime? DateModifiedUtc,
    string? AuthorName);

/// <summary>Schema.org Article JSON-LD projection (DTO only).</summary>
public sealed record ArticleSchemaDto(
    string Context,
    string Type,
    string Headline,
    string? Description,
    string? Image,
    string? DatePublished,
    string? DateModified,
    string? AuthorName);

public sealed record SoftwareApplicationSchemaRequest(
    string Name,
    string? Description,
    string? ApplicationCategory,
    string? OperatingSystem,
    string? OffersPrice,
    string? Url);

/// <summary>Schema.org SoftwareApplication JSON-LD projection — foundation only.</summary>
public sealed record SoftwareApplicationSchemaDto(
    string Context,
    string Type,
    string Name,
    string? Description,
    string? ApplicationCategory,
    string? OperatingSystem,
    string? Offers,
    string? Url);

public sealed record LearningRoadmapSchemaRequest(
    string Name,
    string? Description,
    string? EducationalLevel,
    string? TimeRequired,
    string? Url);

/// <summary>schema.org Course / LearningRoadmap projection — foundation only.</summary>
public sealed record LearningRoadmapSchemaDto(
    string Context,
    string Type,
    string Name,
    string? Description,
    string? EducationalLevel,
    string? TimeRequired,
    string? Url);
