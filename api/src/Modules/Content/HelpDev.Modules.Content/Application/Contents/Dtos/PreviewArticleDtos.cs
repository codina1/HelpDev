namespace HelpDev.Modules.Content.Application.Contents.Dtos;

public sealed record PreviewArticleRequest(string? ContentJson, string? Body);

public sealed record PreviewArticleDto(
    string Html,
    string PlainText,
    int WordCount,
    int ReadingTimeMinutes,
    IReadOnlyList<PreviewHeadingDto> Headings);

public sealed record PreviewHeadingDto(string Id, int Level, string Text);
