using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HelpDev.Modules.Search.Application.Chunking;

/// <summary>
/// Deterministic markdown/plain-text chunker. Splits by headings and paragraphs with a size cap.
/// No AI and no external libraries.
/// </summary>
public sealed class MarkdownContentChunker : IContentChunker
{
    public const int DefaultMaxChunkChars = 1200;
    public const int DefaultMinChunkChars = 40;

    private static readonly Regex HeadingRegex = new(
        @"^(#{1,6})\s+(.+)$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private readonly int _maxChunkChars;
    private readonly int _minChunkChars;

    public MarkdownContentChunker(
        int maxChunkChars = DefaultMaxChunkChars,
        int minChunkChars = DefaultMinChunkChars)
    {
        if (maxChunkChars < 200 || maxChunkChars > 8000)
        {
            throw new ArgumentOutOfRangeException(nameof(maxChunkChars));
        }

        if (minChunkChars < 1 || minChunkChars >= maxChunkChars)
        {
            throw new ArgumentOutOfRangeException(nameof(minChunkChars));
        }

        _maxChunkChars = maxChunkChars;
        _minChunkChars = minChunkChars;
    }

    public IReadOnlyList<ContentChunkDto> Chunk(string title, string body, string? sourceUrl = null)
    {
        var safeTitle = string.IsNullOrWhiteSpace(title) ? "Untitled" : title.Trim();
        var text = Normalize(body);
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var sections = SplitIntoSections(safeTitle, text);
        var chunks = new List<ContentChunkDto>();
        var index = 0;

        foreach (var section in sections)
        {
            foreach (var piece in SplitBySize(section.Body))
            {
                if (piece.Length < _minChunkChars && chunks.Count > 0)
                {
                    // Append tiny leftovers to previous chunk when possible.
                    var previous = chunks[^1];
                    var merged = previous.Content + "\n\n" + piece;
                    if (merged.Length <= _maxChunkChars)
                    {
                        chunks[^1] = previous with { Content = merged };
                        continue;
                    }
                }

                var metadata = JsonSerializer.Serialize(new
                {
                    heading = section.Heading,
                    url = sourceUrl,
                });

                chunks.Add(new ContentChunkDto(
                    index++,
                    piece,
                    string.IsNullOrWhiteSpace(section.Heading) ? safeTitle : $"{safeTitle} — {section.Heading}",
                    metadata));
            }
        }

        return chunks;
    }

    private static string Normalize(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        return body.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
    }

    private static List<(string Heading, string Body)> SplitIntoSections(string documentTitle, string text)
    {
        var matches = HeadingRegex.Matches(text);
        if (matches.Count == 0)
        {
            return [("Introduction", text)];
        }

        var sections = new List<(string Heading, string Body)>();
        if (matches[0].Index > 0)
        {
            var preface = text[..matches[0].Index].Trim();
            if (preface.Length > 0)
            {
                sections.Add(("Introduction", preface));
            }
        }

        for (var i = 0; i < matches.Count; i++)
        {
            var heading = matches[i].Groups[2].Value.Trim();
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var body = text[start..end].Trim();
            if (body.Length == 0 && heading.Length == 0)
            {
                continue;
            }

            sections.Add((
                string.IsNullOrWhiteSpace(heading) ? documentTitle : heading,
                string.IsNullOrWhiteSpace(body) ? heading : body));
        }

        return sections.Count == 0 ? [("Introduction", text)] : sections;
    }

    private IEnumerable<string> SplitBySize(string sectionBody)
    {
        var paragraphs = Regex.Split(sectionBody, @"\n{2,}")
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        if (paragraphs.Count == 0)
        {
            yield break;
        }

        var buffer = new StringBuilder();
        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > _maxChunkChars)
            {
                if (buffer.Length > 0)
                {
                    yield return buffer.ToString();
                    buffer.Clear();
                }

                foreach (var hard in HardSplit(paragraph, _maxChunkChars))
                {
                    yield return hard;
                }

                continue;
            }

            if (buffer.Length == 0)
            {
                buffer.Append(paragraph);
                continue;
            }

            if (buffer.Length + 2 + paragraph.Length <= _maxChunkChars)
            {
                buffer.Append("\n\n").Append(paragraph);
                continue;
            }

            yield return buffer.ToString();
            buffer.Clear();
            buffer.Append(paragraph);
        }

        if (buffer.Length > 0)
        {
            yield return buffer.ToString();
        }
    }

    private static IEnumerable<string> HardSplit(string text, int maxChars)
    {
        for (var i = 0; i < text.Length; i += maxChars)
        {
            var length = Math.Min(maxChars, text.Length - i);
            yield return text.Substring(i, length).Trim();
        }
    }
}
