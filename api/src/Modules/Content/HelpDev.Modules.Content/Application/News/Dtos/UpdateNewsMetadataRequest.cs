namespace HelpDev.Modules.Content.Application.News.Dtos;

public sealed class UpdateNewsMetadataRequest
{
    public string SourceName { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public DateTime NewsDateUtc { get; set; }

    /// <summary>Normal | Featured | Breaking</summary>
    public string Priority { get; set; } = "Normal";

    public string? ExternalReference { get; set; }
}
