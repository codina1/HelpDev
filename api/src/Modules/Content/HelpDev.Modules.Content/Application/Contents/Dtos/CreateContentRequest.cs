namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed class CreateContentRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft";
}
