namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed class UpdateContentRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    public string? CoverImage { get; set; }
}
