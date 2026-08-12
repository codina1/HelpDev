namespace HelpDev.Modules.Content.Application.Articles.Dtos;

public sealed class UpdateArticleMetadataRequest
{
    public Guid? CategoryId { get; set; }

    /// <summary>Beginner | Intermediate | Advanced</summary>
    public string DifficultyLevel { get; set; } = "Beginner";

    public int ReadingTimeMinutes { get; set; } = 1;

    public bool IsFeatured { get; set; }

    public bool AllowComments { get; set; } = true;

    public bool TableOfContentsEnabled { get; set; } = true;
}
